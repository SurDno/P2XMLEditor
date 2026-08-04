using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using P2XMLEditor.Core;
using P2XMLEditor.Forms.Editors;
using P2XMLEditor.Forms.Editors.Actions;
using P2XMLEditor.Forms.Editors.Graphs;
using P2XMLEditor.GameData;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.GameData.VirtualMachineElements.Enums;
using P2XMLEditor.GameData.VirtualMachineElements.Helper;
using P2XMLEditor.GameData.VirtualMachineElements.Interfaces;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes;
using P2XMLEditor.Helper;

namespace P2XMLEditor.Forms.MainForm.Graphs;

/// <summary>
/// Everything about the selection that is not its position on screen — a node or a link.
///
/// Both live here rather than a link getting a dialog of its own. A link is a thing on the
/// canvas exactly as a node is, and putting a modal window over the graph to change which exit
/// it leaves by hides the very picture that makes the answer obvious. Editing is immediate for
/// the same reason: there is no OK to press because there is nothing to cancel back to.
///
/// No tabs either. The sections are short — a name and two checkboxes, a list of entry points,
/// a list of conditions — and a tab strip over four things that would all fit on screen
/// together costs a click to see any one of them and hides the rest for no gain. They are
/// stacked instead, and only the ones that apply to the selection are built.
/// </summary>
public sealed class GraphInspector : Panel {
	private const int LabelWidth = 128;
	private const int RowHeight = 30;

	private readonly VirtualMachine _vm;
	private readonly Label _headline;
	private readonly Panel _scroll;
	private readonly TableLayoutPanel _stack;

	private VmElement? _node;
	private GraphLink? _link;
	private bool _loading;

	public event EventHandler? Changed;
	public event EventHandler<VmElement>? OpenRequested;

	public GraphInspector(VirtualMachine vm) {
		_vm = vm;
		Dock = DockStyle.Right;
		Width = 460;
		Padding = new Padding(6);

		_headline = new Label {
			Dock = DockStyle.Top, Height = 34, TextAlign = ContentAlignment.MiddleLeft,
			Font = new Font(FontFamily.GenericSansSerif, 10f, FontStyle.Bold), AutoEllipsis = true
		};

		_stack = new TableLayoutPanel {
			Dock = DockStyle.Top, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink,
			ColumnCount = 1, RowCount = 0
		};
		_stack.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

		_scroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true };
		_scroll.Controls.Add(_stack);

		Controls.Add(_scroll);
		Controls.Add(_headline);

		SetSelection(null, null);
	}

	// ---------------------------------------------------------------- stacking

	/// <summary>
	/// Adds one section to the column. A TableLayoutPanel rather than a pile of Top-docked
	/// controls: docking gives the order by z-index, which is the reverse of the order things
	/// are added in and one refactor away from being wrong.
	/// </summary>
	private void Section(string title, Control content, int height) {
		var group = new GroupBox {
			Text = title, Dock = DockStyle.Fill, Height = height,
			Margin = new Padding(0, 0, 0, 8), Padding = new Padding(8, 6, 8, 8)
		};
		content.Dock = DockStyle.Fill;
		group.Controls.Add(content);

		var row = _stack.RowCount;
		_stack.RowCount = row + 1;
		_stack.RowStyles.Add(new RowStyle(SizeType.Absolute, height + 8));
		_stack.Controls.Add(group, 0, row);
	}

	private static TableLayoutPanel Rows() {
		var table = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 0 };
		table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, LabelWidth));
		table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
		return table;
	}

	private static void Row(TableLayoutPanel table, string label, Control control) {
		var row = table.RowCount;
		table.RowCount = row + 1;
		table.RowStyles.Add(new RowStyle(SizeType.Absolute, RowHeight));
		table.Controls.Add(new Label {
			Text = label, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, AutoEllipsis = true
		}, 0, row);
		control.Dock = DockStyle.Fill;
		table.Controls.Add(control, 1, row);
	}

	private static ComboBox NewCombo() =>
		new() { DropDownStyle = ComboBoxStyle.DropDownList, IntegralHeight = false };

	private static Button NewButton(string text, System.Action onClick) {
		var button = new Button { Text = text, AutoSize = true, Margin = new Padding(0, 0, 6, 0) };
		button.Click += (_, _) => onClick();
		return button;
	}

	// ---------------------------------------------------------------- selection

	public void SetSelection(VmElement? node, GraphLink? link) {
		_node = node;
		_link = link;
		_loading = true;
		try {
			_stack.SuspendLayout();
			_stack.Controls.Clear();
			_stack.RowStyles.Clear();
			_stack.RowCount = 0;

			if (link != null) {
				_headline.Text = $"Link   {Describe(link)}";
				BuildLink(link);
			} else if (node != null) {
				_headline.Text = $"{node.GetType().Name}   {GraphTopology.NameOf(node)}   [{node.Id}]";
				BuildNode(node);
			} else {
				_headline.Text = "Nothing selected";
			}

			_stack.ResumeLayout();
		} finally {
			_loading = false;
		}
	}

	private static string Describe(GraphLink link) =>
		string.IsNullOrWhiteSpace(link.Name) ? link.Id.ToString() : link.Name;

	private void Touch() {
		if (_loading) return;
		Changed?.Invoke(this, EventArgs.Empty);
	}

	// ---------------------------------------------------------------- node

	private void BuildNode(VmElement node) {
		var general = Rows();

		var name = new TextBox { Text = GraphTopology.NameOf(node) };
		name.TextChanged += (_, _) => {
			switch (node) {
				case IGraphElement element: element.Name = name.Text; break;
				case Talking talking: talking.Name = name.Text; break;
				case Speech speech: speech.Name = name.Text; break;
			}
			Touch();
		};
		Row(general, "Name", name);

		if (node is Branch branch) {
			var type = NewCombo();
			foreach (var value in Enum.GetValues<BranchType>()) type.Items.Add(value);
			type.SelectedItem = branch.BranchType;
			type.SelectedIndexChanged += (_, _) => {
				if (type.SelectedItem is BranchType chosen) branch.BranchType = chosen;
				Touch();
			};
			Row(general, "Branch type", type);
		}

		var initial = new CheckBox {
			Text = "Initial — the FSM starts here", AutoSize = false, Checked = GraphTopology.IsInitial(node)
		};
		initial.CheckedChanged += (_, _) => {
			// Exclusive: the engine takes the first initial node it finds and ignores any others,
			// so ticking one unticks the rest rather than leaving a flag that does nothing.
			if (initial.Checked) GraphTopology.MakeInitial(node);
			else GraphTopology.SetInitial(node, false);
			Touch();
		};
		Row(general, "", initial);

		var ignore = new CheckBox { Text = "Ignore block", AutoSize = false, Checked = IgnoreBlockOf(node) };
		ignore.CheckedChanged += (_, _) => {
			switch (node) {
				case IGraphElement element: element.IgnoreBlock = ignore.Checked; break;
				case Talking talking: talking.IgnoreBlock = ignore.Checked; break;
				case Speech speech: speech.IgnoreBlock = ignore.Checked; break;
			}
			Touch();
		};
		Row(general, "", ignore);

		if (GraphTopology.IsContainer(node)) {
			var open = new Button { Text = "Open this graph" };
			open.Click += (_, _) => OpenRequested?.Invoke(this, node);
			Row(general, "", open);
		}

		var details = new Label {
			Dock = DockStyle.Fill, ForeColor = SystemColors.GrayText, Text = Summarise(node),
			TextAlign = ContentAlignment.TopLeft
		};
		var generalHost = new Panel { Dock = DockStyle.Fill };
		generalHost.Controls.Add(details);
		generalHost.Controls.Add(general);
		general.Dock = DockStyle.Top;
		general.Height = general.RowCount * RowHeight;

		Section(node.GetType().Name, generalHost, general.Height + 74);

		var entries = new EntryPointsEditor(_vm);
		entries.SetNode(node);
		entries.Changed += (_, _) => Touch();
		Section("Entry points — what runs when a link arrives", entries, 210);

		if (node is Branch conditions) BuildConditions(conditions);
		if (node is Graph graph) {
			var inputs = new InputParamsEditor(_vm);
			inputs.SetGraph(graph);
			inputs.Changed += (_, _) => Touch();
			Section("Input params — what every link entering must supply", inputs, 180);
		}
	}

	private static bool IgnoreBlockOf(VmElement node) => node switch {
		IGraphElement element => element.IgnoreBlock,
		Talking talking => talking.IgnoreBlock,
		Speech speech => speech.IgnoreBlock,
		_ => false
	};

	/// <summary>
	/// The node in one paragraph, leading with what is easiest to get wrong: a branch always has
	/// one more exit than it has conditions, and a graph's parameters have to be supplied by
	/// every link entering it.
	/// </summary>
	private string Summarise(VmElement node) {
		var exits = GraphTopology.ExitsOf(node).Count;
		var entries = GraphTopology.EntryPointsOf(node).Count;

		return node switch {
			Branch branch =>
				$"{branch.BranchConditions.Count} condition(s), so {exits} exits — one per condition and "
				+ $"one for when none matched.  {entries} entry point(s).",
			Graph graph =>
				$"{graph.GraphType.Serialize()}, {graph.States.Count} node(s), "
				+ $"{graph.InputParams?.Count ?? 0} input parameter(s), {entries} entry point(s)."
				+ (graph.SubstituteGraph == null
					? ""
					: $"  Substitutes {GraphTopology.NameOf(graph.SubstituteGraph.Value.Element)}."),
			Speech speech => $"{speech.Replies.Count} reply(ies), so {exits} exits.  {entries} entry point(s).",
			Talking talking => $"{talking.States.Count} node(s).  {entries} entry point(s).",
			_ => $"One exit, taken when it finishes.  {entries} entry point(s)."
		};
	}

	// ---------------------------------------------------------------- conditions

	private ListView _conditions = null!;

	private void BuildConditions(Branch branch) {
		_conditions = new ListView {
			Dock = DockStyle.Fill, View = View.Details, FullRowSelect = true, HideSelection = false
		};
		_conditions.Columns.Add("Exit", 44);
		_conditions.Columns.Add("Taken when", 300);
		_conditions.DoubleClick += (_, _) => EditCondition(branch);

		var buttons = new FlowLayoutPanel { Dock = DockStyle.Bottom, AutoSize = true, Padding = new Padding(0, 4, 0, 0) };
		buttons.Controls.AddRange([
			NewButton("Add", () => AddCondition(branch)),
			NewButton("Edit…", () => EditCondition(branch)),
			NewButton("Remove", () => RemoveCondition(branch))
		]);

		var host = new Panel { Dock = DockStyle.Fill };
		host.Controls.Add(_conditions);
		host.Controls.Add(buttons);

		ReloadConditions(branch);
		Section("Conditions — one exit each, plus one for none", host, 190);
	}

	private void ReloadConditions(Branch branch) {
		_conditions.BeginUpdate();
		_conditions.Items.Clear();
		foreach (var exit in GraphTopology.ExitsOf(branch)) {
			var item = new ListViewItem(exit.Index.ToString()) { Tag = exit.Condition };
			item.SubItems.Add(exit.Label);
			if (exit.Condition == null) item.ForeColor = SystemColors.GrayText;
			_conditions.Items.Add(item);
		}
		_conditions.EndUpdate();
	}

	private void AddCondition(Branch branch) {
		var condition = VmElement.CreateDefault<Condition>(_vm, branch);
		using var editor = new ConditionEditorForm(_vm, condition, new(branch));
		if (editor.ShowDialog(FindForm()) == DialogResult.OK) {
			branch.BranchConditions.Add(new(condition));
			ReloadConditions(branch);
			Touch();
		} else {
			_vm.RemoveElement(condition);
		}
	}

	private void EditCondition(Branch branch) {
		if (_conditions.SelectedItems.Count == 0) return;
		if (_conditions.SelectedItems[0].Tag is not Condition condition) return;

		using var editor = new ConditionEditorForm(_vm, condition, new(branch));
		if (editor.ShowDialog(FindForm()) != DialogResult.OK) return;
		ReloadConditions(branch);
		Touch();
	}

	/// <summary>
	/// Removing a condition removes an exit with it, so every link leaving by a later one moves
	/// down. Said plainly first, because nothing on screen would show it afterwards.
	/// </summary>
	private void RemoveCondition(Branch branch) {
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
		ReloadConditions(branch);
		Touch();
	}

	// ---------------------------------------------------------------- link

	private ComboBox _event = null!;
	private ComboBox _entry = null!;
	private Label _entryCaption = null!;
	private TableLayoutPanel _arguments = null!;
	private GroupBox _argumentsGroup = null!;
	private readonly List<ParameterSourceEditor> _argumentEditors = [];

	private void BuildLink(GraphLink link) {
		var rows = Rows();

		var name = new TextBox { Text = link.Name ?? "" };
		name.TextChanged += (_, _) => { link.Name = name.Text; Touch(); };
		Row(rows, "Name", name);

		var enabled = new CheckBox { Text = "Enabled", AutoSize = false, Checked = link.Enabled };
		enabled.CheckedChanged += (_, _) => { link.Enabled = enabled.Checked; Touch(); };
		Row(rows, "", enabled);

		var owner = new EventOwnerEditor(_vm) { GraphOwner = OwnerOf(link.Parent.Element) };
		owner.Load(link.EventObject);
		owner.ValueChanged += (_, _) => {
			link.EventObject = owner.Value;
			PopulateEvents(link, owner);
			Touch();
		};
		Row(rows, "Fires on event of", owner);

		_event = NewCombo();
		_event.SelectedIndexChanged += (_, _) => {
			link.Event = SelectedEvent();
			// The event decides which messages the arguments may be written in terms of.
			RebuildArguments(link);
			Touch();
		};
		Row(rows, "Event", _event);

		Row(rows, "Leaves", new Label {
			Text = link.Source?.Element == null
				? "on the event (no source node)"
				: $"{GraphTopology.NameOf(link.Source.Value.Element)}",
			TextAlign = ContentAlignment.MiddleLeft, AutoEllipsis = true
		});

		var exit = NewCombo();
		foreach (var value in GraphTopology.ExitsOf(link.Source?.Element))
			exit.Items.Add(new IndexItem(value.Index, value.Label));
		SelectByIndex(exit, link.SourceExitPointIndex);
		exit.SelectedIndexChanged += (_, _) => {
			if (exit.SelectedItem is IndexItem item) link.SourceExitPointIndex = item.Index;
			Touch();
		};
		Row(rows, "by", exit);

		var destination = NewCombo();
		destination.Items.Add(new ChoiceItem("", "(nowhere — the link returns instead)"));
		foreach (var node in GraphTopology.NodesOf(link.Parent.Element))
			destination.Items.Add(new ChoiceItem(node.Id.ToString(),
				$"{GraphTopology.NameOf(node)}   [{node.GetType().Name}]"));
		SelectById(destination, link.Destination?.Element.Id.ToString() ?? "");
		destination.SelectedIndexChanged += (_, _) => {
			Retarget(link, Resolve((destination.SelectedItem as ChoiceItem)?.Id));
			PopulateEntries(link);
			RebuildArguments(link);
			Touch();
		};
		Row(rows, "Goes to", destination);

		_entry = NewCombo();
		_entry.SelectedIndexChanged += (_, _) => {
			if (_entry.SelectedItem is IndexItem item) link.DestEntryPointIndex = item.Index;
			Touch();
		};
		Row(rows, "entering at", _entry);
		_entryCaption = (Label)rows.GetControlFromPosition(0, rows.RowCount - 1)!;

		rows.Height = rows.RowCount * RowHeight;
		Section("Link", rows, rows.Height + 12);

		_arguments = new TableLayoutPanel {
			Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 0, AutoScroll = true
		};
		_arguments.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, LabelWidth));
		_arguments.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
		_argumentsGroup = new GroupBox {
			Text = "Arguments", Dock = DockStyle.Fill, Height = 170,
			Margin = new Padding(0, 0, 0, 8), Padding = new Padding(8, 6, 8, 8)
		};
		_argumentsGroup.Controls.Add(_arguments);

		var argumentsRow = _stack.RowCount;
		_stack.RowCount = argumentsRow + 1;
		_stack.RowStyles.Add(new RowStyle(SizeType.Absolute, 178));
		_stack.Controls.Add(_argumentsGroup, 0, argumentsRow);

		PopulateEvents(link, owner);
		SelectById(_event, link.Event?.Id.ToString() ?? "");
		PopulateEntries(link);
		RebuildArguments(link, link.SourceParams);
	}

	private VmElement? Resolve(string? id) =>
		string.IsNullOrEmpty(id) || !ulong.TryParse(id, out var parsed) ? null : _vm.GetNullableElement(parsed);

	/// <summary>
	/// Points the link at another node, keeping both nodes' own link lists in step — they are
	/// the same objects the canvas draws from.
	/// </summary>
	private void Retarget(GraphLink link, VmElement? destination) {
		if (_loading) return;
		if (destination?.Id == link.Destination?.Element.Id) return;

		if (link.Destination?.Element is { } previous) InputLinksOf(previous)?.Remove(link);
		link.Destination = destination == null ? null : new(destination);
		if (destination != null && InputLinksOf(destination) is { } list && !list.Contains(link)) list.Add(link);
	}

	private static List<GraphLink>? InputLinksOf(VmElement node) => node switch {
		IGraphElement element => element.InputLinks,
		Talking talking => talking.InputLinks,
		Speech speech => speech.InputLinks,
		_ => null
	};

	/// <summary>
	/// Events on offer: with an owner named, what that object can raise; with none, the link
	/// fires on its own FSM's event and the FSM's owner is the object to ask.
	/// </summary>
	private void PopulateEvents(GraphLink link, EventOwnerEditor owner) {
		var selected = (_event.SelectedItem as ChoiceItem)?.Id;
		_event.Items.Clear();
		_event.Items.Add(new ChoiceItem("", "(none — follows on from its source)"));

		var holder = owner.ResolvedHolder ?? OwnerOf(link.Parent.Element);
		var events = holder == null
			? _vm.GetElementsByType<Event>()
			: ActionScope.RaisableEvents(holder, _vm);

		foreach (var raisable in events.OrderBy(e => e.Name, StringComparer.Ordinal))
			_event.Items.Add(new ChoiceItem(raisable.Id.ToString(),
				$"{raisable.Name}   ({raisable.Messages.Count} msg)"));

		// An event the owner cannot raise stays selectable when the data already names it.
		if (link.Event != null && !_event.Items.Cast<object>()
				.Any(i => i is ChoiceItem c && c.Id == link.Event.Id.ToString()))
			_event.Items.Insert(1, new ChoiceItem(link.Event.Id.ToString(),
				$"{link.Event.Name}   (not raised by this owner)"));

		SelectById(_event, selected ?? link.Event?.Id.ToString() ?? "");
	}

	private static ParameterHolder? OwnerOf(VmElement container) => container switch {
		Graph graph => graph.Owner,
		Talking talking => talking.Owner.Element as ParameterHolder,
		_ => null
	};

	private Event? SelectedEvent() => Resolve((_event.SelectedItem as ChoiceItem)?.Id) as Event;

	/// <summary>
	/// With a destination this is which entry point the link arrives through; without one the
	/// same field is a LinkExit saying how the flow returns, so the row changes what it offers
	/// and what it is called rather than pretending the number means the same thing both ways.
	/// </summary>
	private void PopulateEntries(GraphLink link) {
		var selected = (_entry.SelectedItem as IndexItem)?.Index ?? link.DestEntryPointIndex;
		_entry.Items.Clear();

		var destination = link.Destination?.Element;
		if (destination == null)
			foreach (var (value, label) in GraphTopology.ExitTypes)
				_entry.Items.Add(new IndexItem((int)value, label));
		else
			foreach (var entry in GraphTopology.EntriesOf(destination))
				_entry.Items.Add(new IndexItem(entry.Index, entry.Label));

		_entryCaption.Text = destination == null ? "returning" : "entering at";
		SelectByIndex(_entry, selected);
		if (_entry.SelectedIndex < 0 && _entry.Items.Count > 0) _entry.SelectedIndex = 0;
	}

	/// <summary>
	/// One editor per input parameter of the graph being entered, typed by its declaration and
	/// scoped to the link — the event's messages and the owning graph's input params, which is
	/// what VMEventLink.GetLocalContextVariables returns.
	/// </summary>
	private void RebuildArguments(GraphLink link, IReadOnlyList<string>? values = null) {
		var existing = values ?? _argumentEditors.Select(e => e.SerializedValue).ToList();

		_arguments.SuspendLayout();
		_arguments.Controls.Clear();
		_arguments.RowStyles.Clear();
		_arguments.RowCount = 0;
		_argumentEditors.Clear();

		var scope = SafeScope(link);
		var parameters = GraphTopology.ParameterisedGraph(link.Destination?.Element)?.InputParams ?? [];
		var count = Math.Max(parameters.Count, existing.Count);

		for (var i = 0; i < count; i++)
			AddArgument(link, i, i < parameters.Count ? parameters[i] : null, scope,
				i < existing.Count ? existing[i] ?? "" : "");

		_argumentsGroup.Text = parameters.Count == 0
			? count == 0 ? "Arguments — the destination takes none"
						 : $"Arguments — the destination takes none, but {count} are passed"
			: $"Arguments — {parameters.Count} expected";

		_arguments.ResumeLayout();
		StoreArguments(link);
	}

	private ActionScope SafeScope(GraphLink link) {
		try {
			return ActionScope.ForLink(link, _vm);
		} catch {
			return ActionScope.Empty;
		}
	}

	private void AddArgument(GraphLink link, int index, InputParameter? parameter, ActionScope scope, string value) {
		var type = SafeTypeInfo(parameter?.Type);
		var editor = new ParameterSourceEditor(_vm, scope, type) { Dock = DockStyle.Fill };
		editor.ValueChanged += (_, _) => {
			StoreArguments(link);
			Touch();
		};

		if (!string.IsNullOrEmpty(value)) {
			try {
				using var fillScope = VirtualMachine.EnterFillScope(scope.LocalContext);
				editor.Load(ParameterSource.Create(value, _vm, null, type), value);
			} catch {
				editor.LoadRaw(value);
			}
		}
		_argumentEditors.Add(editor);

		var row = _arguments.RowCount;
		_arguments.RowCount = row + 1;
		_arguments.RowStyles.Add(new RowStyle(SizeType.Absolute, RowHeight));
		_arguments.Controls.Add(new Label {
			Text = parameter == null ? $"arg {index + 1}" : parameter.ParamName,
			Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, AutoEllipsis = true
		}, 0, row);
		_arguments.Controls.Add(editor, 1, row);
	}

	/// <summary>
	/// An empty list and a missing one differ on the wire: a link that passes nothing writes no
	/// SourceParams element at all, which is what 34979 of them do.
	/// </summary>
	private void StoreArguments(GraphLink link) {
		if (_loading) return;
		var arguments = _argumentEditors.Select(e => e.SerializedValue).ToList();
		link.SourceParams = arguments.Count == 0 ? null : arguments;
	}

	private VmTypeInfo? SafeTypeInfo(string? xmlType) {
		if (string.IsNullOrEmpty(xmlType)) return null;
		try {
			return VmTypeHelper.GetVmTypeInfo(xmlType, _vm);
		} catch {
			return null;
		}
	}

	// ---------------------------------------------------------------- helpers

	private static void SelectById(ComboBox box, string? id) {
		if (id == null) return;
		for (var i = 0; i < box.Items.Count; i++) {
			if (box.Items[i] is ChoiceItem item && item.Id == id) {
				box.SelectedIndex = i;
				return;
			}
		}
		if (box.SelectedIndex < 0 && box.Items.Count > 0) box.SelectedIndex = 0;
	}

	private static void SelectByIndex(ComboBox box, int index) {
		for (var i = 0; i < box.Items.Count; i++) {
			if (box.Items[i] is IndexItem item && item.Index == index) {
				box.SelectedIndex = i;
				return;
			}
		}
		if (box.SelectedIndex < 0 && box.Items.Count > 0) box.SelectedIndex = 0;
	}

	private sealed class ChoiceItem(string id, string label) {
		public string Id { get; } = id;
		public override string ToString() => label;
	}

	private sealed class IndexItem(int index, string label) {
		public int Index { get; } = index;
		public override string ToString() => label;
	}
}
