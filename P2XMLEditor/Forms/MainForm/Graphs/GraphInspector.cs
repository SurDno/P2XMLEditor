using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using P2XMLEditor.Core;
using P2XMLEditor.Forms.Editors;
using P2XMLEditor.Forms.Editors.Graphs;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.GameData.VirtualMachineElements.Enums;
using P2XMLEditor.GameData.VirtualMachineElements.Helper;
using P2XMLEditor.GameData.VirtualMachineElements.Interfaces;
using P2XMLEditor.Helper;

namespace P2XMLEditor.Forms.MainForm.Graphs;

/// <summary>
/// Everything about the selected node that is not its position on screen.
///
/// Split into tabs because a node's parts belong to different questions and cramming them into
/// one scrolling column made none of them findable: what the node is, what runs when a link
/// arrives, what decides which way it leaves, and what it takes from whoever calls it.
/// </summary>
public sealed class GraphInspector : Panel {
	private readonly VirtualMachine _vm;

	private readonly TabControl _tabs;
	private readonly TabPage _generalTab;
	private readonly TabPage _entriesTab;
	private readonly TabPage _conditionsTab;
	private readonly TabPage _inputsTab;

	private readonly Label _headline;
	private readonly TextBox _name;
	private readonly CheckBox _initial;
	private readonly CheckBox _ignoreBlock;
	private readonly ComboBox _branchType;
	private readonly Label _details;
	private readonly Button _open;

	private readonly ListView _conditions;
	private readonly EntryPointsEditor _entries;
	private readonly InputParamsEditor _inputs;

	private VmElement? _node;
	private bool _loading;

	public event EventHandler? Changed;
	public event EventHandler<VmElement>? OpenRequested;

	public GraphInspector(VirtualMachine vm) {
		_vm = vm;
		Dock = DockStyle.Right;
		Width = 420;
		Padding = new Padding(6);

		_headline = new Label {
			Dock = DockStyle.Top, Height = 34, TextAlign = ContentAlignment.MiddleLeft,
			Font = new Font(FontFamily.GenericSansSerif, 10f, FontStyle.Bold)
		};

		_name = new TextBox { Dock = DockStyle.Fill };
		_name.TextChanged += (_, _) => Apply(node => {
			switch (node) {
				case IGraphElement element: element.Name = _name.Text; break;
				case Talking talking: talking.Name = _name.Text; break;
				case Speech speech: speech.Name = _name.Text; break;
			}
		});

		_initial = new CheckBox { Text = "Initial — the FSM starts here", AutoSize = true, Dock = DockStyle.Top };
		_initial.CheckedChanged += (_, _) => Apply(SetInitial);

		_ignoreBlock = new CheckBox { Text = "Ignore block", AutoSize = true, Dock = DockStyle.Top };
		_ignoreBlock.CheckedChanged += (_, _) => Apply(SetIgnoreBlock);

		_branchType = new ComboBox {
			Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList, IntegralHeight = false
		};
		foreach (var type in Enum.GetValues<BranchType>()) _branchType.Items.Add(type);
		_branchType.SelectedIndexChanged += (_, _) => Apply(node => {
			if (node is Branch branch && _branchType.SelectedItem is BranchType type) branch.BranchType = type;
		});

		_details = new Label {
			Dock = DockStyle.Top, AutoSize = false, Height = 96, ForeColor = SystemColors.GrayText,
			TextAlign = ContentAlignment.TopLeft
		};

		_open = new Button { Text = "Open this graph", Dock = DockStyle.Top, Height = 30, Visible = false };
		_open.Click += (_, _) => {
			if (_node != null) OpenRequested?.Invoke(this, _node);
		};

		_generalTab = new TabPage("Node") { Padding = new Padding(8) };
		var general = new TableLayoutPanel {
			Dock = DockStyle.Top, AutoSize = true, ColumnCount = 2, RowCount = 0
		};
		general.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90));
		general.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
		AddRow(general, "Name", _name);
		AddRow(general, "Branch type", _branchType);
		_generalTab.Controls.Add(_details);
		_generalTab.Controls.Add(_open);
		_generalTab.Controls.Add(_ignoreBlock);
		_generalTab.Controls.Add(_initial);
		_generalTab.Controls.Add(general);

		_entries = new EntryPointsEditor(vm) { Dock = DockStyle.Fill };
		_entries.Changed += (_, _) => Changed?.Invoke(this, EventArgs.Empty);
		_entriesTab = new TabPage("Entry points") { Padding = new Padding(8) };
		_entriesTab.Controls.Add(_entries);

		_conditions = new ListView {
			Dock = DockStyle.Fill, View = View.Details, FullRowSelect = true, HideSelection = false
		};
		_conditions.Columns.Add("Exit", 44);
		_conditions.Columns.Add("Taken when", 320);
		_conditions.DoubleClick += (_, _) => EditCondition();

		var conditionButtons = new FlowLayoutPanel {
			Dock = DockStyle.Bottom, AutoSize = true, Padding = new Padding(0, 4, 0, 0)
		};
		conditionButtons.Controls.AddRange([
			NewButton("Add condition", AddCondition),
			NewButton("Edit…", EditCondition),
			NewButton("Remove", RemoveCondition)
		]);

		_conditionsTab = new TabPage("Conditions") { Padding = new Padding(8) };
		_conditionsTab.Controls.Add(_conditions);
		_conditionsTab.Controls.Add(conditionButtons);

		_inputs = new InputParamsEditor(vm) { Dock = DockStyle.Fill };
		_inputs.Changed += (_, _) => Changed?.Invoke(this, EventArgs.Empty);
		_inputsTab = new TabPage("Input params") { Padding = new Padding(8) };
		_inputsTab.Controls.Add(_inputs);

		_tabs = new TabControl { Dock = DockStyle.Fill };
		Controls.Add(_tabs);
		Controls.Add(_headline);

		SetNode(null);
	}

	private static Button NewButton(string text, System.Action onClick) {
		var button = new Button { Text = text, AutoSize = true, Margin = new Padding(0, 0, 6, 0) };
		button.Click += (_, _) => onClick();
		return button;
	}

	private static void AddRow(TableLayoutPanel table, string label, Control control) {
		var row = table.RowCount;
		table.RowCount = row + 1;
		table.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
		table.Controls.Add(new Label {
			Text = label, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft
		}, 0, row);
		table.Controls.Add(control, 1, row);
	}

	// ---------------------------------------------------------------- binding

	public void SetNode(VmElement? node) {
		_node = node;
		_loading = true;
		try {
			_headline.Text = node == null
				? "Nothing selected"
				: $"{node.GetType().Name}   {GraphTopology.NameOf(node)}   [{node.Id}]";

			_tabs.TabPages.Clear();
			if (node == null) {
				_tabs.Enabled = false;
				return;
			}
			_tabs.Enabled = true;

			_name.Text = GraphTopology.NameOf(node);
			_initial.Checked = GraphTopology.IsInitial(node);
			_ignoreBlock.Checked = node switch {
				IGraphElement element => element.IgnoreBlock,
				Talking talking => talking.IgnoreBlock,
				Speech speech => speech.IgnoreBlock,
				_ => false
			};

			var isBranch = node is Branch;
			_branchType.Visible = isBranch;
			if (node is Branch branch) _branchType.SelectedItem = branch.BranchType;

			_open.Visible = GraphTopology.IsContainer(node);
			_details.Text = Describe(node);

			_tabs.TabPages.Add(_generalTab);
			_tabs.TabPages.Add(_entriesTab);
			if (isBranch) _tabs.TabPages.Add(_conditionsTab);
			if (node is Graph) _tabs.TabPages.Add(_inputsTab);

			_entries.SetNode(node);
			_inputs.SetGraph(node as Graph);
			ReloadConditions();
		} finally {
			_loading = false;
		}
	}

	/// <summary>
	/// The node in one paragraph, leading with the thing that is easiest to get wrong: a branch
	/// always has one more exit than it has conditions, and a graph's parameters have to be
	/// supplied by every link that enters it.
	/// </summary>
	private string Describe(VmElement node) {
		var exits = GraphTopology.ExitsOf(node).Count;
		var entries = GraphTopology.EntryPointsOf(node).Count;

		return node switch {
			Branch branch =>
				$"{branch.BranchConditions.Count} condition(s), so {exits} exits — one per condition "
				+ $"and one for when none matched.\r\n{entries} entry point(s).",
			Graph graph =>
				$"{graph.GraphType.Serialize()} with {graph.States.Count} node(s).\r\n"
				+ $"{graph.InputParams?.Count ?? 0} input parameter(s), which every link entering it must supply.\r\n"
				+ $"{entries} entry point(s)."
				+ (graph.SubstituteGraph == null ? "" : $"\r\nSubstitutes {GraphTopology.NameOf(graph.SubstituteGraph.Value.Element)}."),
			Speech speech =>
				$"{speech.Replies.Count} reply(ies), so {exits} exits — one per reply.\r\n{entries} entry point(s).",
			Talking talking =>
				$"{talking.States.Count} node(s).\r\n{entries} entry point(s).",
			_ => $"One exit, taken when it finishes.\r\n{entries} entry point(s)."
		};
	}

	private void Apply(System.Action<VmElement> change) {
		if (_loading || _node == null) return;
		change(_node);
		Changed?.Invoke(this, EventArgs.Empty);
	}

	private void SetInitial(VmElement node) {
		switch (node) {
			case IGraphElement element: element.Initial = _initial.Checked; break;
			case Talking talking: talking.Initial = _initial.Checked; break;
			case Speech speech: speech.Initial = _initial.Checked; break;
		}
	}

	private void SetIgnoreBlock(VmElement node) {
		switch (node) {
			case IGraphElement element: element.IgnoreBlock = _ignoreBlock.Checked; break;
			case Talking talking: talking.IgnoreBlock = _ignoreBlock.Checked; break;
			case Speech speech: speech.IgnoreBlock = _ignoreBlock.Checked; break;
		}
	}

	// ---------------------------------------------------------------- conditions

	/// <summary>
	/// A branch's conditions, listed against the exit each one takes. The pairing is the reason
	/// this is a list and not a set: exit <c>i</c> is condition <c>i</c>, so reordering them
	/// re-points every link that leaves the branch.
	/// </summary>
	private void ReloadConditions() {
		_conditions.BeginUpdate();
		_conditions.Items.Clear();

		if (_node is Branch branch) {
			foreach (var exit in GraphTopology.ExitsOf(branch)) {
				var item = new ListViewItem(exit.Index.ToString()) { Tag = exit.Condition };
				item.SubItems.Add(exit.Label);
				if (exit.Condition == null) item.ForeColor = SystemColors.GrayText;
				_conditions.Items.Add(item);
			}
		}

		_conditions.EndUpdate();
	}

	private void AddCondition() {
		if (_node is not Branch branch) return;

		var condition = VmElement.CreateDefault<Condition>(_vm, branch);
		using var editor = new ConditionEditorForm(_vm, condition, new(branch));
		if (editor.ShowDialog(FindForm()) == DialogResult.OK) {
			branch.BranchConditions.Add(new(condition));
			ReloadConditions();
			Changed?.Invoke(this, EventArgs.Empty);
		} else {
			_vm.RemoveElement(condition);
		}
	}

	private void EditCondition() {
		if (_node is not Branch branch) return;
		if (_conditions.SelectedItems.Count == 0) return;
		if (_conditions.SelectedItems[0].Tag is not Condition condition) return;

		using var editor = new ConditionEditorForm(_vm, condition, new(branch));
		if (editor.ShowDialog(FindForm()) != DialogResult.OK) return;
		ReloadConditions();
		Changed?.Invoke(this, EventArgs.Empty);
	}

	/// <summary>
	/// Removing a condition removes an exit with it, so every link leaving by a later one moves
	/// down. Said plainly before it happens, because nothing on screen would show it afterwards.
	/// </summary>
	private void RemoveCondition() {
		if (_node is not Branch branch) return;
		if (_conditions.SelectedItems.Count == 0) return;
		if (_conditions.SelectedItems[0].Tag is not { } condition) return;

		var index = branch.BranchConditions.FindIndex(c => c.Element == condition);
		if (index < 0) return;

		var affected = _vm.GetElementsByType<GraphLink>()
			.Count(l => l.Source?.Element == branch && l.SourceExitPointIndex >= index);

		var message = $"Remove exit {index}?";
		if (affected > 0) message += $"\n\n{affected} link(s) leave by it or by a later exit and will shift down.";

		if (MessageBox.Show(this, message, "Remove condition", MessageBoxButtons.YesNo,
				MessageBoxIcon.Warning) != DialogResult.Yes)
			return;

		branch.BranchConditions.RemoveAt(index);
		_vm.RemoveElement(condition);
		ReloadConditions();
		Changed?.Invoke(this, EventArgs.Empty);
	}
}
