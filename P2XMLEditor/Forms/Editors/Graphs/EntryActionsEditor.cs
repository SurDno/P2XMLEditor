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
	private readonly Label _lineLabel;
	private readonly TableLayoutPanel _loopRow;
	private ParameterSourceEditor? _loopList;
	private ParameterSourceEditor? _loopStart;
	private ParameterSourceEditor? _loopEnd;
	private CheckBox? _loopRandom;
	private ActionLine? _loopLine;
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
		// The header describes the line the selection sits in, so it follows the selection: a
		// nested loop line has bounds of its own and nothing else reaches them.
		_tree.AfterSelect += (_, _) => {
			RefreshHeader();
			UpdateButtons();
		};

		_loop = new CheckBox { Text = "Loop — run the actions once per iteration", AutoSize = true, Margin = new Padding(0, 4, 12, 4) };
		_loop.CheckedChanged += (_, _) => SetLoop(_loop.Checked);

		// Four other line types exist — Inventory, Market, GateSystem, CustomGroup — on 510 lines
		// across the two corpora. A tick box cannot say which one a line is, so on those the real
		// list appears instead of a checkbox that would answer "not a loop" and mean nothing.
		_lineTypeCaption = new Label { Text = "Line type", AutoSize = true, Margin = new Padding(0, 8, 6, 4) };
		_lineType = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 150, Margin = new Padding(0, 4, 12, 4) };
		foreach (var value in Enum.GetValues<ActionLineType>()) _lineType.Items.Add(value);
		_lineType.SelectedIndexChanged += (_, _) => {
			if (_loading || CurrentLine() is not { } line || _lineType.SelectedItem is not ActionLineType chosen) return;
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
		_lineLabel = new Label {
			AutoSize = true, ForeColor = SystemColors.GrayText, Margin = new Padding(0, 8, 0, 4), Visible = false
		};

		// What a loop actually loops over. The line carries a list and a range — "over this list,
		// from here to there" — and none of it was editable, so a line could be turned into a loop
		// and then only ever run with the bounds it was created with: 0 to 2147483647 over nothing.
		// Filled in by BuildLoopRow, because each editor is bound to one scope for its lifetime and
		// the scope belongs to whichever line is being edited.
		_loopRow = new TableLayoutPanel {
			Dock = DockStyle.Top, ColumnCount = 2, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink,
			Padding = new Padding(0, 0, 0, 6), Visible = false
		};
		_loopRow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 96));
		_loopRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

		_addAction = NewButton("Add action", AddAction);
		_edit = NewButton("Edit…", () => Edit(_tree.SelectedNode));
		_remove = NewButton("Remove", RemoveSelected);
		_up = NewButton("Move up", () => Move(-1));
		_down = NewButton("Move down", () => Move(1));

		var header = new FlowLayoutPanel {
			Dock = DockStyle.Top, FlowDirection = FlowDirection.LeftToRight, AutoSize = true,
			WrapContents = true, Padding = new Padding(0, 0, 0, 4)
		};
		header.Controls.AddRange([_loop, _lineTypeCaption, _lineType, _lineLabel, _note]);

		var buttons = new FlowLayoutPanel {
			Dock = DockStyle.Bottom, FlowDirection = FlowDirection.LeftToRight, AutoSize = true,
			WrapContents = true, Padding = new Padding(0, 4, 0, 0)
		};
		buttons.Controls.AddRange([_addAction, _edit, _remove, _up, _down]);

		Controls.Add(_tree);
		Controls.Add(buttons);
		Controls.Add(_loopRow);
		Controls.Add(header);
	}

	private static Label Caption(string text) =>
		new() { Text = text, AutoSize = true, Margin = new Padding(6, 8, 4, 4), ForeColor = SystemColors.GrayText };

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

	/// <summary>
	/// The line the header edits: the one the selection sits in, so a nested line's own type and
	/// loop bounds can be reached, and the entry's own line when nothing is selected.
	/// </summary>
	private ActionLine? CurrentLine() => SelectedLine() ?? Line();

	// ---------------------------------------------------------------- display

	private void Reload() {
		var wasLoading = _loading;
		_loading = true;
		try {
			var selected = _tree.SelectedNode?.Tag as VmElement;

			_tree.BeginUpdate();
			_tree.Nodes.Clear();
			foreach (var child in Line()?.Actions ?? [])
				_tree.Nodes.Add(ActionNode(child.Element));
			_tree.ExpandAll();
			_tree.EndUpdate();

			// Carried across the rebuild: the header follows the selection, so dropping it would
			// quietly move the header back to the entry's own line mid-edit.
			if (selected != null) SelectElement(selected);
		} finally {
			_loading = wasLoading;
		}

		RefreshHeader();
		UpdateButtons();
	}

	private void RefreshHeader() {
		var wasLoading = _loading;
		_loading = true;
		try {
			var line = CurrentLine();

			var unusual = line != null && line.ActionLineType is not (ActionLineType.Common or ActionLineType.Loop);
			_loop.Visible = line != null && !unusual;
			_loop.Checked = line?.ActionLineType == ActionLineType.Loop;
			_lineType.Visible = _lineTypeCaption.Visible = unusual;
			if (line != null) _lineType.SelectedItem = line.ActionLineType;

			// Said out loud only when the header is about something other than the entry's own
			// line, which is the case the reader has no other way of noticing.
			var nested = line != null && !ReferenceEquals(line, Line());
			_lineLabel.Visible = nested;
			if (nested) _lineLabel.Text = $"— of the line '{line!.Name}'";

			// A graph's actions are inert, so the note appears and the action buttons go. The
			// entry point itself is not offered either way — it is made and unmade by what is
			// done to the node, not by a button.
			var inert = _node is Graph;
			_note.Visible = inert;
			_addAction.Visible = _edit.Visible = _remove.Visible = _up.Visible = _down.Visible =
				!inert || Line() != null;

			LoadLoop(line);
		} finally {
			_loading = wasLoading;
		}
	}

	/// <summary>
	/// Shows the bounds of the line now being edited, rebuilding them when that line changes.
	///
	/// Rebuilt rather than reloaded because a <see cref="ParameterSourceEditor"/> takes its
	/// <see cref="ActionScope"/> once, and the scope is the line's own — the messages reaching it,
	/// its graph's input parameters, the loops it sits inside.
	/// </summary>
	private void LoadLoop(ActionLine? line) {
		var info = line is { ActionLineType: ActionLineType.Loop } ? line.LoopInfo : null;
		if (info == null) {
			_loopLine = null;
			_loopRow.Visible = false;
			return;
		}

		if (!ReferenceEquals(line, _loopLine)) {
			_loopLine = line;
			BuildLoopRow(line!, info);
		}
		_loopRow.Visible = true;
	}

	private void BuildLoopRow(ActionLine line, ActionLoopInfo info) {
		var scope = ActionScope.For(line, _vm);

		// A loop walks a list, so the slot is typed as one: that is what puts global lists on offer
		// and keeps everything that is not a list off it. It costs nothing in reach — of the two
		// corpora's 203 loops, 45 name a global list and 155 name a parameter, and every one of
		// those 118 distinct parameters is declared CommonList.
		_loopList = NewSource(scope, new GameData.VmTypeInfo(GameData.Enums.VmType.List), false, info.Name);
		// The bounds are the only place the engine reads a const_ value, which is what 195 of the
		// Sandbox's 196 loops start at and 187 of them end at.
		_loopStart = NewSource(scope, GameData.VmTypeInfo.Int32, true, info.Start);
		_loopEnd = NewSource(scope, GameData.VmTypeInfo.Int32, true, info.End);
		_loopRandom = new CheckBox {
			Text = "Random order", AutoSize = true, Checked = info.Random, Margin = new Padding(0, 4, 0, 2)
		};
		_loopRandom.CheckedChanged += (_, _) => StoreLoop();

		_loopRow.SuspendLayout();
		var previous = _loopRow.Controls.Cast<Control>().ToList();
		_loopRow.Controls.Clear();
		foreach (var control in previous) control.Dispose();

		_loopRow.RowStyles.Clear();
		_loopRow.RowCount = 0;
		LoopRow("over the list", _loopList);
		LoopRow("from index", _loopStart);
		LoopRow("to index", _loopEnd);
		LoopRow("", _loopRandom, false);
		_loopRow.ResumeLayout();
	}

	private void LoopRow(string caption, Control editor, bool stretch = true) {
		var row = _loopRow.RowCount++;
		_loopRow.RowStyles.Add(new RowStyle(SizeType.AutoSize));
		_loopRow.Controls.Add(Caption(caption), 0, row);
		if (stretch) editor.Anchor = AnchorStyles.Left | AnchorStyles.Right;
		_loopRow.Controls.Add(editor, 1, row);
	}

	private ParameterSourceEditor NewSource(ActionScope scope, GameData.VmTypeInfo type, bool allowConstant,
		ParameterSource value) {
		var editor = new ParameterSourceEditor(_vm, scope, type, null, allowConstant) { Width = 460 };
		editor.Load(value);
		editor.ValueChanged += (_, _) => StoreLoop();
		return editor;
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
		if (_loading || CurrentLine() is not { } line) return;
		line.ActionLineType = loop ? ActionLineType.Loop : ActionLineType.Common;
		EnsureLoopInfo(line);
		// A nested line says its type in the tree, and the bounds appear or go, so this is a
		// reload rather than a repaint.
		Reload();
		Changed?.Invoke(this, EventArgs.Empty);
	}

	/// <summary>
	/// Writes the bounds back onto the line. <see cref="ActionLoopInfo"/> holds its three sources
	/// init-only, so a change means a new one and all four values are written together.
	///
	/// Changing the list does not strand the actions inside the loop: a loop element is written as
	/// local_&lt;lineId&gt;_Loop_List_&lt;list&gt;_Element and <see cref="ParameterSource.Write"/> re-derives
	/// that middle part from the line's current LoopInfo, so the references follow the rename.
	/// </summary>
	private void StoreLoop() {
		if (_loading || _loopLine is not { } line) return;
		if (_loopList == null || _loopStart == null || _loopEnd == null || _loopRandom == null) return;

		line.LoopInfo = new ActionLoopInfo(_loopList.Value, _loopStart.Value, _loopEnd.Value, _loopRandom.Checked);
		Changed?.Invoke(this, EventArgs.Empty);
	}

	/// <summary>
	/// A loop line needs its bounds, or nothing decides how many times it runs.
	///
	/// The defaults are the whole list — const_0 to const_2147483647 — which is what 187 of the
	/// Sandbox's 196 loops say, rather than the 0-to-10 the other editors seed, which is a range
	/// that silently stops after ten elements and is not a shape the shipped data uses anywhere.
	/// </summary>
	private void EnsureLoopInfo(ActionLine line) {
		if (line.ActionLineType != ActionLineType.Loop || line.LoopInfo != null) return;
		line.LoopInfo = new ActionLoopInfo(
			ParameterSource.Create("", _vm),
			ParameterSource.Create("const_0", _vm, null, GameData.VmTypeInfo.Int32),
			ParameterSource.Create("const_2147483647", _vm, null, GameData.VmTypeInfo.Int32),
			false);
	}

	/// <summary>
	/// Adds an action to whichever line the selection sits in, so a nested line fills too, making
	/// whatever has to exist for it to sit in. An entry point and its line
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
