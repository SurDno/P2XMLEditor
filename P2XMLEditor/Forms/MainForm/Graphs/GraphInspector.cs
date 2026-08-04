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
	///
	/// The conditions themselves are not repeated here. They are written on the links they gate,
	/// which is where they are read and where they are edited; saying them twice only raises the
	/// question of which of the two is the real one.
	/// </summary>
	private string Summarise(VmElement node) {
		var exits = GraphTopology.ExitsOf(node).Count;
		var entries = GraphTopology.EntryPointsOf(node).Count;

		return node switch {
			Branch branch =>
				$"{branch.BranchConditions?.Count ?? 0} condition(s), so {exits} exits — one per condition and "
				+ $"one for when none matched.  Each is written on the link it gates.{Dangling(branch)}"
				+ $"  {entries} entry point(s).",
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

	/// <summary>
	/// Conditions with nothing leaving by them. The engine evaluates such an exit and then
	/// returns from it, and the game never ships one — all 4713 branch conditions across the two
	/// corpora have a link on them. With the conditions living on the links there is nothing on
	/// screen that would show one either, so the count says it.
	/// </summary>
	private static string Dangling(Branch branch) {
		var links = GraphTopology.LinksFrom(branch);
		var count = GraphTopology.ExitsOf(branch)
			.Count(exit => exit.Condition != null && links.All(l => l.SourceExitPointIndex != exit.Index));
		return count == 0 ? "" : $"  {count} condition(s) have no link and are evaluated for nothing.";
	}


	// ---------------------------------------------------------------- link

	private ComboBox _event = null!;
	private ComboBox _entry = null!;
	private Label _entryCaption = null!;
	private TableLayoutPanel _arguments = null!;
	private GroupBox _argumentsGroup = null!;
	private readonly List<ParameterSourceEditor> _argumentEditors = [];

	private void BuildLink(GraphLink link) {
		// A link leaving a branch is taken by evaluating the branch's conditions, not by waiting
		// for anything: ProcessBranch picks the exit and calls ProcessLink straight away, and
		// SubscribeToEvents only ever runs for a state the FSM comes to rest in. The data agrees
		// without exception — of the 8015 links leaving a branch in the two corpora, not one
		// carries an Event or an EventObject. So those two rows are not shown here; what does
		// belong is the condition the exit is taken on, which is above.
		var branch = link.Source?.Element as Branch;
		if (branch != null) BuildExitCondition(link, branch);

		var rows = Rows();

		var name = new TextBox { Text = link.Name ?? "" };
		name.TextChanged += (_, _) => { link.Name = name.Text; Touch(); };
		Row(rows, "Name", name);

		var enabled = new CheckBox { Text = "Enabled", AutoSize = false, Checked = link.Enabled };
		enabled.CheckedChanged += (_, _) => { link.Enabled = enabled.Checked; Touch(); };
		Row(rows, "", enabled);

		var owner = new EventOwnerEditor(_vm) { GraphOwner = OwnerOf(link.Parent.Element) };
		_event = NewCombo();

		if (branch == null) {
			owner.Load(link.EventObject);
			owner.ValueChanged += (_, _) => {
				link.EventObject = owner.Value;
				PopulateEvents(link, owner);
				Touch();
			};
			Row(rows, "Fires on event of", owner);

			_event.SelectedIndexChanged += (_, _) => {
				link.Event = SelectedEvent();
				// The event decides which messages the arguments may be written in terms of.
				RebuildArguments(link);
				Touch();
			};
			Row(rows, "Event", _event);
		}

		Row(rows, "Leaves", new Label {
			Text = link.Source?.Element == null
				? "on the event (no source node)"
				: $"{GraphTopology.NameOf(link.Source.Value.Element)}",
			TextAlign = ContentAlignment.MiddleLeft, AutoEllipsis = true
		});

		// No exit picker for a branch: the link owns the condition it is taken on, and choosing
		// a different index would silently hand it somebody else's. A speech keeps one, because
		// there the exit is which reply was given and picking it is the whole point. A state or
		// a subgraph has a single unconditional exit, so there is nothing to pick.
		var exits = GraphTopology.ExitsOf(link.Source?.Element);
		if (branch == null && exits.Count > 1) {
			var exit = NewCombo();
			foreach (var value in exits) exit.Items.Add(new IndexItem(value.Index, value.Label));
			SelectByIndex(exit, link.SourceExitPointIndex);
			exit.SelectedIndexChanged += (_, _) => {
				if (exit.SelectedItem is IndexItem item) link.SourceExitPointIndex = item.Index;
				Touch();
			};
			Row(rows, "by", exit);
		}

		var destination = NewCombo();
		destination.Items.Add(new ChoiceItem("", "(nowhere — the link returns instead)"));
		foreach (var node in GraphTopology.NodesOf(link.Parent.Element))
			destination.Items.Add(new ChoiceItem(node.Id.ToString(),
				$"{GraphTopology.NameOf(node)}   [{node.GetType().Name}]"));

		// A destination outside this graph — a placeholder for an id the data never defines, or
		// a node belonging elsewhere — is kept selectable. Dropping it would show the link as
		// going nowhere, which is a different thing entirely and one save away from being true.
		var current = link.Destination?.Element;
		if (current != null && GraphTopology.NodesOf(link.Parent.Element).All(n => n.Id != current.Id))
			destination.Items.Add(new ChoiceItem(current.Id.ToString(),
				$"{GraphTopology.NameOf(current)}   (not in this graph)"));

		SelectById(destination, current?.Id.ToString() ?? "");
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

		if (branch == null) {
			PopulateEvents(link, owner);
			SelectById(_event, link.Event?.Id.ToString() ?? "");
		}
		PopulateEntries(link);
		RebuildArguments(link, link.SourceParams);
	}

	/// <summary>
	/// The condition this link is taken on, shown above the link itself and edited there.
	///
	/// It is stored on the branch — BranchConditions[i] gates exit i — but that is not where it
	/// is understood. Reading a graph, the question is "when is this arrow taken", and answering
	/// it meant selecting the branch, counting exits and matching an index. Here it is one line
	/// above the arrow it belongs to.
	///
	/// The last exit has no condition: it is the one taken when none matched. Giving it one
	/// appends to the branch, which turns this exit into a real condition and pushes the
	/// "otherwise" out to the next index — so the link's own exit number does not even change.
	/// </summary>
	private void BuildExitCondition(GraphLink link, Branch branch) {
		var index = link.SourceExitPointIndex;
		var conditions = branch.BranchConditions;
		var condition = index >= 0 && index < conditions.Count ? conditions[index].Element : null;

		var text = new TextBox {
			Dock = DockStyle.Fill, ReadOnly = true, Multiline = true, BorderStyle = BorderStyle.None,
			BackColor = SystemColors.Control, ScrollBars = ScrollBars.Vertical, WordWrap = true,
			Text = DescribeCondition(branch, index, condition)
		};

		var buttons = new FlowLayoutPanel { Dock = DockStyle.Bottom, AutoSize = true, Padding = new Padding(0, 4, 0, 0) };
		if (condition is Condition editable) {
			buttons.Controls.Add(NewButton("Edit…", () => {
				using var editor = new ConditionEditorForm(_vm, editable, new(branch));
				if (editor.ShowDialog(FindForm()) != DialogResult.OK) return;
				text.Text = DescribeCondition(branch, index, editable);
				Touch();
			}));
			buttons.Controls.Add(NewButton("Remove", () => RemoveExitCondition(link, branch, editable)));
		} else if (index == conditions.Count) {
			buttons.Controls.Add(NewButton("Give this link a condition", () => AddExitCondition(link, branch)));
		}

		var host = new Panel { Dock = DockStyle.Fill };
		host.Controls.Add(text);
		host.Controls.Add(buttons);

		// This is the one thing on a branch link worth reading, and a condition is a nested
		// expression written out in full — three lines was showing a tenth of a real one and
		// asking the user to scroll a read-only box a line at a time to see the rest.
		Section("Taken when", host, ConditionHeight(text.Text));
	}

	/// <summary>
	/// Enough room for the condition as written, within reason: short ones do not leave a hole
	/// under themselves and long ones stop being a peephole.
	/// </summary>
	private static int ConditionHeight(string text) {
		var lines = text.Length / 46 + text.Count(c => c == '\n') + 1;
		return Math.Clamp(48 + lines * 15, 108, 320);
	}

	private string DescribeCondition(Branch branch, int index, VmElement? condition) {
		if (condition != null) {
			try {
				return PreviewHelper.Preview(condition);
			} catch {
				return $"condition {condition.Id}";
			}
		}

		if (index == branch.BranchConditions.Count)
			return "No condition — this is the exit taken when none of the others matched.";
		return $"Exit {index} has no condition; the branch declares {branch.BranchConditions.Count}.";
	}

	/// <summary>
	/// Turns the "otherwise" exit into a conditional one. Appending is all it takes: the new
	/// condition lands at the index this link already leaves by, and the otherwise exit moves to
	/// the next number — taking anything else that was leaving by it along, so those links stay
	/// on "otherwise" rather than inheriting a condition written for this one.
	/// </summary>
	private void AddExitCondition(GraphLink link, Branch branch) {
		var index = link.SourceExitPointIndex;
		var condition = GraphTopology.AddCondition(branch, _vm);

		// AddCondition moved every link on the otherwise exit up, including this one; this is the
		// link the condition was written for, so it stays where it is and takes it.
		link.SourceExitPointIndex = index;

		using var editor = new ConditionEditorForm(_vm, condition, new(branch));
		if (editor.ShowDialog(FindForm()) != DialogResult.OK) {
			GraphTopology.RemoveConditionAt(branch, index, _vm);
			SetSelection(null, link);
			return;
		}

		SetSelection(null, link);
		Touch();
	}

	/// <summary>
	/// Drops the condition, which drops the exit with it. Every link leaving by a later exit
	/// moves down a number so it keeps the condition it was on, so this is the one edit here
	/// that reaches past the link in front of you — it says how many before doing anything.
	/// </summary>
	private void RemoveExitCondition(GraphLink link, Branch branch, Condition condition) {
		var index = branch.BranchConditions.FindIndex(c => c.Element == condition);
		if (index < 0) return;

		var affected = GraphTopology.LinksFrom(branch).Count(l => l.SourceExitPointIndex > index);

		var message = "Remove this condition?\n\nThis link will fall through to the otherwise exit.";
		if (affected > 0) message += $"\n\n{affected} other link(s) leave by a later exit and will move down a number.";

		if (MessageBox.Show(this, message, "Remove condition", MessageBoxButtons.YesNo,
				MessageBoxIcon.Warning) != DialogResult.Yes)
			return;

		GraphTopology.RemoveConditionAt(branch, index, _vm);
		SetSelection(null, link);
		Touch();
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
