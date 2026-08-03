using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using P2XMLEditor.Core;
using P2XMLEditor.Forms.Editors.Actions;
using P2XMLEditor.GameData;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.GameData.VirtualMachineElements.Helper;
using P2XMLEditor.GameData.VirtualMachineElements.Interfaces;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes;
using P2XMLEditor.Helper;

namespace P2XMLEditor.Forms.Editors.Graphs;

/// <summary>
/// Edits one <see cref="GraphLink"/> — a transition between two nodes of a graph.
///
/// A link stores four things the old editor showed as raw integers and free text, and all four
/// mean something the editor can work out. <see cref="GraphTopology"/> holds the rules; this
/// form is what they buy:
///
/// * the exit is chosen from the source's real outcomes — a branch's conditions and its else
///   exit, a speech's replies, or the single unconditional exit a state has;
/// * the entry is chosen from the destination's own entry points, each shown with the action
///   line it runs on arrival;
/// * the event is chosen from those the named owner can actually raise, and the owner from the
///   four forms that subscribe at all;
/// * the arguments are one typed slot per input parameter of the graph being entered, edited
///   by the same <see cref="ParameterSourceEditor"/> as everywhere else — which means the
///   event's own messages are on offer inside them, since a link fires on its event and that
///   payload is exactly what it has to pass on.
/// </summary>
public sealed class GraphLinkEditorForm : Form {
	private const int RowHeight = 34;
	private const int LabelWidth = 190;

	private readonly VirtualMachine _vm;
	private readonly GraphLink _link;
	private readonly VmElement _container;

	private readonly TextBox _name;
	private readonly CheckBox _enabled;
	private readonly EventOwnerEditor _owner;
	private readonly ComboBox _event;
	private readonly Label _source;
	private readonly ComboBox _exit;
	private readonly ComboBox _destination;
	private readonly ComboBox _entry;
	private readonly TableLayoutPanel _rows;
	private readonly TableLayoutPanel _arguments;
	private readonly GroupBox _argumentsGroup;
	private readonly TextBox _preview;

	private readonly List<ParameterSourceEditor> _argumentEditors = [];

	private Panel _entryRow = null!;

	private bool _loading = true;

	public GraphLinkEditorForm(VirtualMachine vm, GraphLink link) {
		_vm = vm;
		_link = link;
		_container = link.Parent.Element;

		Text = $"Link {link.Id}   —   in {GraphTopology.NameOf(_container)}";
		Size = new Size(1080, 720);
		MinimumSize = new Size(820, 560);
		StartPosition = FormStartPosition.CenterParent;
		MinimizeBox = false;
		ShowInTaskbar = false;

		_name = new TextBox();

		_enabled = new CheckBox { Text = "Enabled", AutoSize = true, Dock = DockStyle.Left };

		_owner = new EventOwnerEditor(vm);
		_owner.ValueChanged += (_, _) => {
			PopulateEvents();
			RefreshPreview();
		};

		_event = NewCombo();
		_event.SelectedIndexChanged += (_, _) => {
			// The event decides which messages the arguments may be written in terms of, so the
			// argument editors are rebuilt against the new scope rather than left on the old one.
			RebuildArguments();
			RefreshPreview();
		};

		_source = new Label { Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft };

		_exit = NewCombo();
		_exit.SelectedIndexChanged += (_, _) => RefreshPreview();

		_destination = NewCombo();
		_destination.SelectedIndexChanged += (_, _) => {
			PopulateEntries();
			RebuildArguments();
			RefreshPreview();
		};

		_entry = NewCombo();
		_entry.SelectedIndexChanged += (_, _) => RefreshPreview();

		_rows = new TableLayoutPanel {
			Dock = DockStyle.Top, AutoSize = true, ColumnCount = 2, RowCount = 0,
			Padding = new Padding(12, 12, 12, 0)
		};
		_rows.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, LabelWidth));
		_rows.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

		AddRow("Name", _name);
		AddRow("", _enabled);
		AddRow("Fires on the event of", _owner);
		AddRow("Event", _event);
		AddRow("Leaves", _source);
		AddRow("by", _exit);
		AddRow("Goes to", _destination);
		_entryRow = AddRow("entering at", _entry);

		_arguments = new TableLayoutPanel {
			Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 0, AutoScroll = true,
			Padding = new Padding(6, 4, 6, 4)
		};
		_arguments.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, LabelWidth - 12));
		_arguments.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

		_argumentsGroup = new GroupBox {
			Text = "Arguments", Dock = DockStyle.Fill, Margin = new Padding(12, 6, 12, 6)
		};
		_argumentsGroup.Controls.Add(_arguments);

		_preview = new TextBox {
			Dock = DockStyle.Bottom, Height = 120, ReadOnly = true, Multiline = true,
			ScrollBars = ScrollBars.Vertical, Font = new Font(FontFamily.GenericMonospace, 9f)
		};

		var buttons = new FlowLayoutPanel {
			Dock = DockStyle.Bottom, FlowDirection = FlowDirection.RightToLeft, Height = 52,
			Padding = new Padding(12, 10, 12, 10)
		};
		var cancel = new Button { Text = "Cancel", Size = new Size(100, 32), DialogResult = DialogResult.Cancel };
		var save = new Button { Text = "Save", Size = new Size(100, 32), Margin = new Padding(8, 0, 0, 0) };
		save.Click += (_, _) => Save();
		buttons.Controls.AddRange([cancel, save]);
		AcceptButton = save;
		CancelButton = cancel;

		var body = new Panel { Dock = DockStyle.Fill };
		body.Controls.Add(_argumentsGroup);
		body.Controls.Add(_preview);
		body.Controls.Add(_rows);

		Controls.Add(body);
		Controls.Add(buttons);

		LoadLink(link);
		_loading = false;
		RefreshPreview();
	}

	private static ComboBox NewCombo() =>
		new() { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList, IntegralHeight = false };

	private Panel AddRow(string label, Control control) {
		var row = _rows.RowCount;
		_rows.RowCount = row + 1;
		_rows.RowStyles.Add(new RowStyle(SizeType.Absolute, RowHeight));

		var host = new Panel { Dock = DockStyle.Fill, Margin = new Padding(0, 2, 0, 2) };
		control.Dock = DockStyle.Fill;
		host.Controls.Add(control);

		var caption = new Label {
			Text = label, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft
		};
		_rows.Controls.Add(caption, 0, row);
		_rows.Controls.Add(host, 1, row);
		host.Tag = caption;
		return host;
	}

	// ---------------------------------------------------------------- loading

	private void LoadLink(GraphLink link) {
		_name.Text = link.Name ?? "";
		_enabled.Checked = link.Enabled;

		_owner.Load(link.EventObject);
		PopulateEvents();
		SelectById(_event, link.Event?.Id.ToString());

		_source.Text = link.Source?.Element == null
			? "on the event (the link has no source node)"
			: $"{GraphTopology.NameOf(link.Source.Value.Element)}   [{TypeName(link.Source.Value.Element)}]";

		PopulateExits();
		SelectByIndex(_exit, link.SourceExitPointIndex);

		PopulateDestinations();
		SelectById(_destination, link.Destination?.Element.Id.ToString() ?? "");

		PopulateEntries();
		SelectByIndex(_entry, link.DestEntryPointIndex);

		RebuildArguments(link.SourceParams);
	}

	private static string TypeName(VmElement element) => element.GetType().Name;

	// ---------------------------------------------------------------- events

	/// <summary>
	/// Events on offer. With an owner named, only what that object can raise; with none, the
	/// link fires on its own FSM's event, and the FSM's owner is the object to ask. Where
	/// neither pins an object — a parameter that does not name a blueprint — every event is
	/// offered, because nothing here knows better.
	/// </summary>
	private void PopulateEvents() {
		var selected = SelectedEventId;
		_event.Items.Clear();
		_event.Items.Add(new ChoiceItem("", "(none — the link follows on from its source)"));

		var holder = _owner.ResolvedHolder ?? OwnerOfContainer();
		var events = holder == null
			? _vm.GetElementsByType<Event>()
			: ActionScope.RaisableEvents(holder, _vm);

		foreach (var raisable in events.OrderBy(e => e.Name, StringComparer.Ordinal))
			_event.Items.Add(new ChoiceItem(raisable.Id.ToString(),
				$"{raisable.Name}   ({raisable.Messages.Count} msg)   ← {OwnerName(raisable)}"));

		// An event the owner cannot raise stays selectable when the data already names it:
		// existing content outranks the editor's reading of who owns what.
		if (!string.IsNullOrEmpty(selected) && !Contains(_event, selected) && _link.Event != null)
			_event.Items.Insert(1, new ChoiceItem(selected!, $"{_link.Event.Name}   (not raised by this owner)"));

		SelectById(_event, selected ?? "");
	}

	private ParameterHolder? OwnerOfContainer() => _container switch {
		Graph graph => graph.Owner,
		Talking talking => talking.Owner.Element as ParameterHolder,
		_ => null
	};

	private static string OwnerName(Event raisable) =>
		raisable.Parent.Element is INamedElement named ? named.Name : raisable.Parent.Element.GetType().Name;

	private string? SelectedEventId => (_event.SelectedItem as ChoiceItem)?.Id;

	private Event? SelectedEvent {
		get {
			var id = SelectedEventId;
			return string.IsNullOrEmpty(id) || !ulong.TryParse(id, out var parsed)
				? null
				: _vm.GetNullableElement<Event>(parsed);
		}
	}

	// ---------------------------------------------------------------- endpoints

	private void PopulateExits() {
		_exit.Items.Clear();
		foreach (var exit in GraphTopology.ExitsOf(_link.Source?.Element))
			_exit.Items.Add(new IndexItem(exit.Index, exit.Label));
	}

	private void PopulateDestinations() {
		_destination.Items.Clear();
		// Going nowhere is a real answer: a link with no destination returns rather than moving
		// on, and a fifth of the links in the shipped data are exactly that.
		_destination.Items.Add(new ChoiceItem("", "(nowhere — the link returns instead)"));
		foreach (var node in GraphTopology.NodesOf(_container))
			_destination.Items.Add(new ChoiceItem(node.Id.ToString(),
				$"{GraphTopology.NameOf(node)}   [{TypeName(node)}]"));
	}

	private VmElement? SelectedDestination {
		get {
			var id = (_destination.SelectedItem as ChoiceItem)?.Id;
			return string.IsNullOrEmpty(id) || !ulong.TryParse(id, out var parsed)
				? null
				: _vm.GetNullableElement(parsed);
		}
	}

	/// <summary>
	/// The second half of "goes to". With a destination it is which entry point the link arrives
	/// through; without one the same field is a LinkExit saying how the flow returns, so the row
	/// changes what it offers and what it is called rather than pretending the number means the
	/// same thing both ways.
	/// </summary>
	private void PopulateEntries() {
		var selected = (_entry.SelectedItem as IndexItem)?.Index;
		_entry.Items.Clear();

		var destination = SelectedDestination;
		if (destination == null) {
			foreach (var (exit, label) in GraphTopology.ExitTypes)
				_entry.Items.Add(new IndexItem((int)exit, label));
		} else {
			foreach (var entry in GraphTopology.EntriesOf(destination))
				_entry.Items.Add(new IndexItem(entry.Index, entry.Label));
		}

		if (_entryRow.Tag is Label caption)
			caption.Text = destination == null ? "returning" : "entering at";

		if (selected != null) SelectByIndex(_entry, selected.Value);
		if (_entry.SelectedIndex < 0 && _entry.Items.Count > 0) _entry.SelectedIndex = 0;
	}

	private int SelectedExitIndex => (_exit.SelectedItem as IndexItem)?.Index ?? -1;
	private int SelectedEntryIndex => (_entry.SelectedItem as IndexItem)?.Index ?? 0;

	// ---------------------------------------------------------------- arguments

	/// <summary>
	/// One editor per input parameter of the graph being entered, typed by its declaration.
	///
	/// The scope handed to them is the link's, not an action's: the event's messages and the
	/// owning graph's input parameters, and no loop variables, because a link runs outside any
	/// action line. Changing the event or the destination rebuilds them, since both change what
	/// may be written here.
	/// </summary>
	private void RebuildArguments(IReadOnlyList<string>? values = null) {
		var existing = values ?? _argumentEditors.Select(e => e.SerializedValue).ToList();

		_arguments.SuspendLayout();
		_arguments.Controls.Clear();
		_arguments.RowStyles.Clear();
		_arguments.RowCount = 0;
		_argumentEditors.Clear();

		var scope = ScopeForArguments();
		var parameters = GraphTopology.ParameterisedGraph(SelectedDestination)?.InputParams ?? [];
		var count = Math.Max(parameters.Count, existing.Count);

		for (var i = 0; i < count; i++) {
			var parameter = i < parameters.Count ? parameters[i] : null;
			AddArgument(i, parameter, scope, i < existing.Count ? existing[i] ?? "" : "");
		}

		_argumentsGroup.Text = parameters.Count == 0
			? count == 0
				? "Arguments — the destination takes none"
				: $"Arguments — the destination takes none, but {count} are passed"
			: $"Arguments — {parameters.Count} expected by {GraphTopology.NameOf(SelectedDestination)}";

		_arguments.ResumeLayout();
	}

	private ActionScope ScopeForArguments() {
		try {
			// Built off a copy carrying the event now selected rather than the one stored, so the
			// messages on offer follow the dropdown instead of lagging a save behind.
			var selected = SelectedEvent;
			if (selected == _link.Event) return ActionScope.ForLink(_link, _vm);

			var previous = _link.Event;
			_link.Event = selected;
			try {
				return ActionScope.ForLink(_link, _vm);
			} finally {
				_link.Event = previous;
			}
		} catch {
			return ActionScope.Empty;
		}
	}

	private void AddArgument(int index, InputParameter? parameter, ActionScope scope, string value) {
		var type = SafeTypeInfo(parameter?.Type);
		var editor = new ParameterSourceEditor(_vm, scope, type) { Dock = DockStyle.Fill };
		editor.ValueChanged += (_, _) => RefreshPreview();

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
			Text = parameter == null ? $"arg {index + 1}   (undeclared)" : $"{parameter.ParamName}   [{parameter.Type}]",
			Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, AutoEllipsis = true
		}, 0, row);
		_arguments.Controls.Add(editor, 1, row);
	}

	private VmTypeInfo? SafeTypeInfo(string? xmlType) {
		if (string.IsNullOrEmpty(xmlType)) return null;
		try {
			return VmTypeHelper.GetVmTypeInfo(xmlType, _vm);
		} catch {
			return null;
		}
	}

	// ---------------------------------------------------------------- save

	private string? ValidationError() {
		var exits = GraphTopology.ExitsOf(_link.Source?.Element);
		if (exits.Count > 0 && exits.All(e => e.Index != SelectedExitIndex))
			return "Choose which way out of the source node this link is taken.";

		return null;
	}

	private void Save() {
		if (ValidationError() is { } error) {
			MessageBox.Show(this, error, "Cannot save", MessageBoxButtons.OK, MessageBoxIcon.Warning);
			return;
		}

		_link.Name = _name.Text;
		_link.Enabled = _enabled.Checked;
		_link.EventObject = _owner.Value;
		_link.Event = SelectedEvent;
		_link.SourceExitPointIndex = SelectedExitIndex;
		_link.DestEntryPointIndex = SelectedEntryIndex;

		var destination = SelectedDestination;
		if (destination?.Id != _link.Destination?.Element.Id) Retarget(destination);

		// An empty list and a missing one are different on the wire: a link that passes nothing
		// writes no SourceParams element at all, which is what 34 979 of them do.
		var arguments = _argumentEditors.Select(e => e.SerializedValue).ToList();
		_link.SourceParams = arguments.Count == 0 ? null : arguments;

		DialogResult = DialogResult.OK;
		Close();
	}

	/// <summary>
	/// Points the link at another node, keeping the two nodes' own link lists in step. They are
	/// the same objects the graph draws from, so leaving a stale entry behind would show an edge
	/// that no longer exists.
	/// </summary>
	private void Retarget(VmElement? destination) {
		if (_link.Destination?.Element is { } previous && InputLinksOf(previous) is { } oldList)
			oldList.Remove(_link);

		_link.Destination = destination == null ? null : new(destination);

		if (destination != null && InputLinksOf(destination) is { } newList && !newList.Contains(_link))
			newList.Add(_link);
	}

	private static List<GraphLink>? InputLinksOf(VmElement node) => node switch {
		IGraphElement graphElement => graphElement.InputLinks,
		Talking talking => talking.InputLinks,
		Speech speech => speech.InputLinks,
		_ => null
	};

	// ---------------------------------------------------------------- preview

	private void RefreshPreview() {
		if (_loading) return;

		var lines = new List<string> {
			$"name        {_name.Text}",
			$"enabled     {_enabled.Checked}",
			$"owner       {_owner.Value?.Write() ?? "(this FSM)"}",
			$"event       {SelectedEvent?.Name ?? "(none)"}",
			$"from        {GraphTopology.NameOf(_link.Source?.Element)}  exit {SelectedExitIndex}",
			$"to          {(SelectedDestination == null ? "returns: " + ((GraphTopology.LinkExit)SelectedEntryIndex) : GraphTopology.NameOf(SelectedDestination) + "  entry " + SelectedEntryIndex)}"
		};

		for (var i = 0; i < _argumentEditors.Count; i++)
			lines.Add($"  arg[{i}]   {_argumentEditors[i].SerializedValue}");

		if (ValidationError() is { } error) lines.Add($"\r\n! {error}");

		_preview.Text = string.Join("\r\n", lines);
	}

	// ---------------------------------------------------------------- helpers

	private static bool Contains(ComboBox box, string id) =>
		box.Items.Cast<object>().Any(item => item is ChoiceItem choice && choice.Id == id);

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
