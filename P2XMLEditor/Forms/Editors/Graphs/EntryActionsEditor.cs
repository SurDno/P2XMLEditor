using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using P2XMLEditor.Core;
using P2XMLEditor.Forms.Editors.Actions;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.GameData.VirtualMachineElements.Enums;
using P2XMLEditor.GameData.VirtualMachineElements.Helper;
using P2XMLEditor.GameData.VirtualMachineElements.Interfaces;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes;
using P2XMLEditor.Helper;
using VmAction = P2XMLEditor.GameData.VirtualMachineElements.Action;

namespace P2XMLEditor.Forms.Editors.Graphs;

/// <summary>
/// What a node runs when a link arrives — its actions, and nothing about entry points.
///
/// A node has exactly one entry point. Not "usually": across PathologicSandbox, MarbleNest and
/// the alpha corpus, all 17 412 + 1865 + 9604 states, 3200 + 274 + 1392 branches, 760 + 126 + 222
/// talkings and 5089 + 609 + 1367 speeches declare exactly one, and not a single element of any
/// kind declares two. So the list of them, the button to add another and the index a link picks
/// from were a whole layer of interface over a set that never has more than one member — and the
/// thing anybody actually opens this for, the actions, was squeezed into what was left.
///
/// Only a graph can have none: 866 of the Sandbox's, 5 of MarbleNest's, 777 of the alpha's — and
/// every one of those has no links arriving at it. Whether a node has one is therefore not a
/// question to put to the user: it follows from what the node is and what has been done to it.
/// So there is no button. Adding an action makes the entry point that holds it, removing the last
/// one takes it away again where the node can do without it, and linking into a graph makes it
/// too — see <see cref="GraphTopology.EnsureEntryPoint"/> and
/// <see cref="GraphTopology.PruneEntryPointIfUnneeded"/>.
/// </summary>
public sealed class EntryActionsEditor : UserControl {
	private readonly VirtualMachine _vm;
	private readonly TreeView _tree;
	private readonly CheckBox _loop;
	private readonly ComboBox _lineType;
	private readonly Label _lineTypeCaption;
	private readonly Label _note;
	private readonly Button _addAction;
	private readonly Button _edit;
	private readonly Button _remove;
	private readonly Button _up;
	private readonly Button _down;

	private VmElement? _node;
	private bool _loading;

	public event EventHandler? Changed;

	public EntryActionsEditor(VirtualMachine vm) {
		_vm = vm;

		_tree = new TreeView {
			Dock = DockStyle.Fill, FullRowSelect = true, HideSelection = false, ShowLines = true,
			ShowPlusMinus = true, Indent = 18
		};
		_tree.NodeMouseDoubleClick += (_, e) => Edit(e.Node);
		_tree.AfterSelect += (_, _) => UpdateButtons();

		_loop = new CheckBox { Text = "Loop — run the actions once per iteration", AutoSize = true, Margin = new Padding(0, 4, 12, 4) };
		_loop.CheckedChanged += (_, _) => SetLoop(_loop.Checked);

		// Four other line types exist — Inventory, Market, GateSystem, CustomGroup — on 510 lines
		// across the two corpora. A tick box cannot say which one a line is, so on those the real
		// list appears instead of a checkbox that would answer "not a loop" and mean nothing.
		_lineTypeCaption = new Label { Text = "Line type", AutoSize = true, Margin = new Padding(0, 8, 6, 4) };
		_lineType = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 150, Margin = new Padding(0, 4, 12, 4) };
		foreach (var value in Enum.GetValues<ActionLineType>()) _lineType.Items.Add(value);
		_lineType.SelectedIndexChanged += (_, _) => {
			if (_loading || Line() is not { } line || _lineType.SelectedItem is not ActionLineType chosen) return;
			line.ActionLineType = chosen;
			EnsureLoopInfo(line);
			Reload();
			Changed?.Invoke(this, EventArgs.Empty);
		};

		// A graph's entry actions never run: ProcessMoveToState sends a link into a graph to
		// MoveIntoSubGraph, which applies the link's index to the subgraph's initial state. The
		// entry point still has to exist where links arrive, which the editor handles itself.
		_note = new Label {
			AutoSize = true, ForeColor = SystemColors.GrayText, Margin = new Padding(0, 6, 0, 4),
			Text = "A graph runs nothing on arrival — a link entering it goes to its initial node."
		};
		_addAction = NewButton("Add action", AddAction);
		_edit = NewButton("Edit…", () => Edit(_tree.SelectedNode));
		_remove = NewButton("Remove", RemoveSelected);
		_up = NewButton("Move up", () => Move(-1));
		_down = NewButton("Move down", () => Move(1));

		var header = new FlowLayoutPanel {
			Dock = DockStyle.Top, FlowDirection = FlowDirection.LeftToRight, AutoSize = true,
			WrapContents = true, Padding = new Padding(0, 0, 0, 4)
		};
		header.Controls.AddRange([_loop, _lineTypeCaption, _lineType, _note]);

		var buttons = new FlowLayoutPanel {
			Dock = DockStyle.Bottom, FlowDirection = FlowDirection.LeftToRight, AutoSize = true,
			WrapContents = true, Padding = new Padding(0, 4, 0, 0)
		};
		buttons.Controls.AddRange([_addAction, _edit, _remove, _up, _down]);

		Controls.Add(_tree);
		Controls.Add(buttons);
		Controls.Add(header);
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

	// ---------------------------------------------------------------- the one entry point

	private EntryPoint? Point() => GraphTopology.EntryPointsOf(_node).FirstOrDefault();

	private ActionLine? Line() => Point()?.ActionLine;

	// ---------------------------------------------------------------- display

	private void Reload() {
		_loading = true;
		try {
			_tree.BeginUpdate();
			_tree.Nodes.Clear();

			var line = Line();
			foreach (var child in line?.Actions ?? [])
				_tree.Nodes.Add(ActionNode(child.Element));
			_tree.ExpandAll();
			_tree.EndUpdate();

			var unusual = line != null && line.ActionLineType is not (ActionLineType.Common or ActionLineType.Loop);
			_loop.Visible = line != null && !unusual;
			_loop.Checked = line?.ActionLineType == ActionLineType.Loop;
			_lineType.Visible = _lineTypeCaption.Visible = unusual;
			if (line != null) _lineType.SelectedItem = line.ActionLineType;
			// A graph's actions are inert, so the note appears and the action buttons go. The
			// entry point itself is not offered either way — it is made and unmade by what is
			// done to the node, not by a button.
			var inert = _node is Graph;
			_note.Visible = inert;
			_addAction.Visible = _edit.Visible = _remove.Visible = _up.Visible = _down.Visible =
				!inert || line != null;
		} finally {
			_loading = false;
		}

		UpdateButtons();
	}

	private TreeNode ActionNode(VmElement element) {
		var text = element switch {
			VmAction action => PreviewHelper.Preview(action),
			ActionLine nested => $"[{nested.ActionLineType.Serialize()}]  {nested.Name}",
			_ => element.Id.ToString()
		};

		var node = new TreeNode(text) {
			Tag = element,
			ForeColor = element is ActionLine ? Color.DarkGreen : Color.DarkBlue
		};

		if (element is ActionLine line)
			foreach (var child in line.Actions ?? [])
				node.Nodes.Add(ActionNode(child.Element));

		return node;
	}

	private void UpdateButtons() {
		var line = Line();
		var selected = _tree.SelectedNode?.Tag;

		_addAction.Enabled = line != null;
		_edit.Enabled = selected is VmAction;
		_remove.Enabled = selected is VmAction or ActionLine;
		_up.Enabled = _down.Enabled = selected != null && ListOf(_tree.SelectedNode) != null;
	}

	// ---------------------------------------------------------------- commands

	private void SetLoop(bool loop) {
		if (_loading || Line() is not { } line) return;
		line.ActionLineType = loop ? ActionLineType.Loop : ActionLineType.Common;
		EnsureLoopInfo(line);
		Changed?.Invoke(this, EventArgs.Empty);
	}

	/// <summary>
	/// A loop line needs its bounds, or nothing decides how many times it runs. The defaults
	/// match what the writer emits for a line the editor of the day created.
	/// </summary>
	private void EnsureLoopInfo(ActionLine line) {
		if (line.ActionLineType != ActionLineType.Loop || line.LoopInfo != null) return;
		line.LoopInfo = new ActionLoopInfo(
			ParameterSource.Create("", _vm),
			ParameterSource.Create("0", _vm, null, GameData.VmTypeInfo.Int32),
			ParameterSource.Create("10", _vm, null, GameData.VmTypeInfo.Int32),
			false);
	}

	/// <summary>Adds an action to whichever line the selection sits in, so a nested line fills too.</summary>
	/// <summary>
	/// Adds an action, making whatever has to exist for it to sit in. An entry point and its line
	/// are not things to be created on their own — they exist because something runs in them.
	/// </summary>
	private void AddAction() {
		if (_node == null) return;

		var line = SelectedLine();
		if (line == null) {
			var point = GraphTopology.EnsureEntryPoint(_node, _vm);
			line = point.ActionLine ??= VmElement.CreateDefault<ActionLine>(_vm, _node);
		}

		var action = VmElement.CreateDefault<VmAction>(_vm, line);
		(line.Actions ??= []).Add(new(action));

		using var editor = new ActionEditorForm(_vm, action);
		if (editor.ShowDialog(FindForm()) != DialogResult.OK) {
			line.Actions.RemoveAll(a => a.Element == action);
			_vm.RemoveElement(action);
			// Cancelling leaves nothing behind, including whatever was made to hold the action.
			if (line.Actions.Count == 0) GraphTopology.PruneEntryPointIfUnneeded(_node, _vm);
		}

		Reload();
		Changed?.Invoke(this, EventArgs.Empty);
	}

	private void Edit(TreeNode? node) {
		if (node?.Tag is not VmAction action) return;

		using var editor = new ActionEditorForm(_vm, action);
		if (editor.ShowDialog(FindForm()) != DialogResult.OK) return;

		Reload();
		Changed?.Invoke(this, EventArgs.Empty);
	}

	private void RemoveSelected() {
		if (_tree.SelectedNode is not { } node || node.Tag is not VmElement element) return;
		if (ListOf(node) is not { } list) return;

		if (element is ActionLine nested && (nested.Actions?.Count ?? 0) > 0 &&
			MessageBox.Show(this, $"Remove the line '{nested.Name}' and its {nested.Actions!.Count} action(s)?",
				"Remove", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
			return;

		list.RemoveAll(a => a.Element == element);
		_vm.RemoveElement(element);

		// And unmade when the last thing in it goes, where the node can do without one.
		if (Line() is { Actions.Count: 0 }) GraphTopology.PruneEntryPointIfUnneeded(_node, _vm);

		Reload();
		Changed?.Invoke(this, EventArgs.Empty);
	}

	/// <summary>
	/// Actions run in the order they are listed in, so moving one is a real edit rather than a
	/// display preference — which is why it is offered at all.
	/// </summary>
	private void Move(int delta) {
		if (_tree.SelectedNode is not { } node || node.Tag is not VmElement element) return;
		if (ListOf(node) is not { } list) return;

		var index = list.FindIndex(a => a.Element == element);
		var target = index + delta;
		if (index < 0 || target < 0 || target >= list.Count) return;

		(list[index], list[target]) = (list[target], list[index]);
		Reload();
		SelectElement(element);
		Changed?.Invoke(this, EventArgs.Empty);
	}

	/// <summary>The list a tree node's element lives in — the entry's line, or an enclosing one.</summary>
	private List<VmEither<VmAction, ActionLine>>? ListOf(TreeNode? node) =>
		node?.Parent?.Tag is ActionLine parent ? parent.Actions : Line()?.Actions;

	/// <summary>The line a new action would go into: the selected one, or the one holding the selection.</summary>
	private ActionLine? SelectedLine() {
		for (var node = _tree.SelectedNode; node != null; node = node.Parent)
			if (node.Tag is ActionLine line) return line;
		return null;
	}

	/// <summary>Not named Select: Control.Select() is a different thing and one typo away.</summary>
	private void SelectElement(VmElement element) {
		foreach (var node in All(_tree.Nodes))
			if (ReferenceEquals(node.Tag, element)) {
				_tree.SelectedNode = node;
				return;
			}
	}

	private static IEnumerable<TreeNode> All(TreeNodeCollection nodes) {
		foreach (TreeNode node in nodes) {
			yield return node;
			foreach (var child in All(node.Nodes)) yield return child;
		}
	}
}
