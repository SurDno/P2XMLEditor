using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using P2XMLEditor.Core;
using P2XMLEditor.Forms.Editors.Actions;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.GameData.VirtualMachineElements.Helper;
using P2XMLEditor.Helper;
using VmAction = P2XMLEditor.GameData.VirtualMachineElements.Action;

namespace P2XMLEditor.Forms.Editors.Graphs;

/// <summary>
/// The entry points of one node, and the actions each runs on arrival.
///
/// An entry point is not a label: it owns an <see cref="ActionLine"/> that runs when a link
/// arrives through it, which is why a link chooses one by name rather than by number. Showing
/// the actions inline is the point of the control — the whole reason a node has more than one
/// entry is that arriving different ways should do different things, and that is invisible
/// until the actions are on screen next to the name.
///
/// Order is identity here: a link stores DestEntryPointIndex, so moving an entry point silently
/// re-points every link that named it. Reordering is therefore not offered, and removing one
/// says how many links it would break before it does anything.
/// </summary>
public sealed class EntryPointsEditor : UserControl {
	private readonly VirtualMachine _vm;
	private readonly TreeView _tree;
	private readonly Button _addPoint;
	private readonly Button _addAction;
	private readonly Button _edit;
	private readonly Button _remove;

	private VmElement? _node;

	public event EventHandler? Changed;

	public EntryPointsEditor(VirtualMachine vm) {
		_vm = vm;

		_tree = new TreeView {
			Dock = DockStyle.Fill, FullRowSelect = true, HideSelection = false, ShowLines = true
		};
		_tree.NodeMouseDoubleClick += (_, e) => Edit(e.Node);
		_tree.AfterSelect += (_, _) => UpdateButtons();

		_addPoint = NewButton("Add entry point", AddEntryPoint);
		_addAction = NewButton("Add action", AddAction);
		_edit = NewButton("Edit…", () => Edit(_tree.SelectedNode));
		_remove = NewButton("Remove", RemoveSelected);

		var buttons = new FlowLayoutPanel {
			Dock = DockStyle.Bottom, FlowDirection = FlowDirection.LeftToRight, AutoSize = true,
			WrapContents = true, Padding = new Padding(0, 4, 0, 0)
		};
		buttons.Controls.AddRange([_addPoint, _addAction, _edit, _remove]);

		Controls.Add(_tree);
		Controls.Add(buttons);
	}

	private static Button NewButton(string text, System.Action onClick) {
		var button = new Button { Text = text, AutoSize = true, Margin = new Padding(0, 0, 6, 0) };
		button.Click += (_, _) => onClick();
		return button;
	}

	public void SetNode(VmElement? node) {
		_node = node;
		Reload();
	}

	private void Reload() {
		_tree.BeginUpdate();
		_tree.Nodes.Clear();

		var points = GraphTopology.EntryPointsOf(_node);
		for (var i = 0; i < points.Count; i++) {
			var point = points[i];
			var node = new TreeNode($"{i}:  {Describe(point)}") { Tag = point };
			foreach (var child in point.ActionLine?.Actions ?? [])
				node.Nodes.Add(ActionNode(child.Element));
			node.Expand();
			_tree.Nodes.Add(node);
		}

		_tree.EndUpdate();
		UpdateButtons();
	}

	private static string Describe(EntryPoint point) {
		var name = string.IsNullOrWhiteSpace(point.Name) ? point.Id.ToString() : point.Name;
		var actions = point.ActionLine?.Actions?.Count ?? 0;
		return actions == 0 ? $"{name}   (does nothing)" : $"{name}   ({actions} action(s))";
	}

	private TreeNode ActionNode(VmElement element) {
		var text = element switch {
			VmAction action => ActionText(action),
			ActionLine line => $"[line] {line.Name}",
			_ => element.Id.ToString()
		};
		var node = new TreeNode(text) { Tag = element, ForeColor = Color.DarkBlue };
		if (element is ActionLine nested)
			foreach (var child in nested.Actions ?? [])
				node.Nodes.Add(ActionNode(child.Element));
		return node;
	}

	private string ActionText(VmAction action) {
		try {
			var name = string.IsNullOrWhiteSpace(action.Name) ? "" : $"{action.Name}:  ";
			return $"{name}{action.ActionType.Serialize()}";
		} catch {
			return action.Id.ToString();
		}
	}

	private void UpdateButtons() {
		var selected = _tree.SelectedNode?.Tag;
		_addPoint.Enabled = _node != null;
		_addAction.Enabled = SelectedEntryPoint() != null;
		_edit.Enabled = selected is VmAction or EntryPoint;
		_remove.Enabled = selected is VmAction or EntryPoint;
	}

	/// <summary>The entry point a selection sits under, whether it or one of its actions is selected.</summary>
	private EntryPoint? SelectedEntryPoint() {
		for (var node = _tree.SelectedNode; node != null; node = node.Parent)
			if (node.Tag is EntryPoint point) return point;
		return null;
	}

	// ---------------------------------------------------------------- commands

	private void AddEntryPoint() {
		if (_node == null) return;

		var points = GraphTopology.EntryPointsOf(_node);
		var point = VmElement.CreateDefault<EntryPoint>(_vm, _node);
		point.Name = $"Entry_{points.Count}";
		// An entry point with no line has nothing to run; every one in the corpus has one.
		point.ActionLine ??= VmElement.CreateDefault<ActionLine>(_vm, _node);
		points.Add(point);

		Reload();
		Changed?.Invoke(this, EventArgs.Empty);
	}

	private void AddAction() {
		if (SelectedEntryPoint() is not { } point) return;

		point.ActionLine ??= VmElement.CreateDefault<ActionLine>(_vm, _node!);
		var action = VmElement.CreateDefault<VmAction>(_vm, point.ActionLine);
		(point.ActionLine.Actions ??= []).Add(new(action));

		using var editor = new ActionEditorForm(_vm, action);
		if (editor.ShowDialog(FindForm()) != DialogResult.OK) {
			point.ActionLine.Actions.RemoveAll(a => a.Element == action);
			_vm.RemoveElement(action);
		}

		Reload();
		Changed?.Invoke(this, EventArgs.Empty);
	}

	private void Edit(TreeNode? node) {
		switch (node?.Tag) {
			case VmAction action: {
				using var editor = new ActionEditorForm(_vm, action);
				if (editor.ShowDialog(FindForm()) == DialogResult.OK) {
					Reload();
					Changed?.Invoke(this, EventArgs.Empty);
				}
				break;
			}
			case EntryPoint point:
				Rename(point);
				break;
		}
	}

	private void Rename(EntryPoint point) {
		using var dialog = new RenameDialog("Entry point name", point.Name ?? "");
		if (dialog.ShowDialog(FindForm()) != DialogResult.OK) return;
		point.Name = dialog.Value;
		Reload();
		Changed?.Invoke(this, EventArgs.Empty);
	}

	private void RemoveSelected() {
		switch (_tree.SelectedNode?.Tag) {
			case EntryPoint point:
				RemoveEntryPoint(point);
				break;
			case VmAction action:
				RemoveAction(action);
				break;
		}
	}

	/// <summary>
	/// Removes an entry point, but only after saying what it costs. Links address entry points
	/// by position, so taking one out shifts every later index and silently re-points the links
	/// that used them — the one operation here that can quietly change behaviour elsewhere.
	/// </summary>
	private void RemoveEntryPoint(EntryPoint point) {
		if (_node == null) return;
		var points = GraphTopology.EntryPointsOf(_node);
		var index = points.IndexOf(point);
		if (index < 0) return;

		var arriving = LinksInto(_node, index);
		var shifted = points.Count - index - 1;
		var warning = $"Remove entry point '{point.Name}'?";
		if (arriving > 0) warning += $"\n\n{arriving} link(s) arrive through it and will point at nothing.";
		if (shifted > 0) warning += $"\n\n{shifted} later entry point(s) shift down, so links naming them will arrive somewhere else.";

		if (MessageBox.Show(this, warning, "Remove entry point", MessageBoxButtons.YesNo,
				MessageBoxIcon.Warning) != DialogResult.Yes)
			return;

		points.RemoveAt(index);
		_vm.RemoveElement(point);
		Reload();
		Changed?.Invoke(this, EventArgs.Empty);
	}

	private int LinksInto(VmElement node, int entryIndex) =>
		_vm.GetElementsByType<GraphLink>()
			.Count(l => l.Destination?.Element == node && l.DestEntryPointIndex == entryIndex);

	private void RemoveAction(VmAction action) {
		if (SelectedEntryPoint()?.ActionLine is not { } line) return;
		line.Actions?.RemoveAll(a => a.Element == action);
		_vm.RemoveElement(action);
		Reload();
		Changed?.Invoke(this, EventArgs.Empty);
	}

	/// <summary>A one-line prompt; WinForms has no built-in.</summary>
	private sealed class RenameDialog : Form {
		private readonly TextBox _text;

		public RenameDialog(string prompt, string value) {
			Text = prompt;
			Size = new Size(420, 150);
			FormBorderStyle = FormBorderStyle.FixedDialog;
			StartPosition = FormStartPosition.CenterParent;
			MinimizeBox = false;
			MaximizeBox = false;
			ShowInTaskbar = false;

			_text = new TextBox { Dock = DockStyle.Top, Text = value, Margin = new Padding(12) };

			var buttons = new FlowLayoutPanel {
				Dock = DockStyle.Bottom, FlowDirection = FlowDirection.RightToLeft, Height = 48,
				Padding = new Padding(10)
			};
			var cancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel };
			var ok = new Button { Text = "OK", DialogResult = DialogResult.OK, Margin = new Padding(8, 0, 0, 0) };
			buttons.Controls.AddRange([cancel, ok]);
			AcceptButton = ok;
			CancelButton = cancel;

			var host = new Panel { Dock = DockStyle.Fill, Padding = new Padding(12, 16, 12, 0) };
			host.Controls.Add(_text);

			Controls.Add(host);
			Controls.Add(buttons);
		}

		public string Value => _text.Text;
	}
}
