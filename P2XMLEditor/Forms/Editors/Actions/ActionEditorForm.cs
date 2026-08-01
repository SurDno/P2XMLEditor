using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using P2XMLEditor.Core;
using P2XMLEditor.GameData;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.GameData.VirtualMachineElements.Enums;
using P2XMLEditor.GameData.VirtualMachineElements.Helper;
using P2XMLEditor.GameData.VirtualMachineElements.Interfaces;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes;
using P2XMLEditor.Helper;
using VmAction = P2XMLEditor.GameData.VirtualMachineElements.Action;

namespace P2XMLEditor.Forms.Editors.Actions;

/// <summary>
/// Edits a single <see cref="VmAction"/>.
///
/// The form is driven by three things it works out from the action itself: its
/// <see cref="ActionScope"/>, which decides which messages, input parameters and loop
/// variables can be referenced here at all; the target object's functional components, which
/// decide which functions and events can be called on it; and the declared type of whatever
/// is being written to or called, which decides what each value slot will accept. All three
/// flow into the child controls, so retargeting the action reshapes the whole form beneath it.
/// </summary>
public sealed class ActionEditorForm : Form {
	private const int RowHeight = 38;
	private const int ContentHeight = 30;
	private const int LabelColumn = 200;
	private const int TypeColumn = 300;

	/// <summary>ACTION_TYPE_NONE never occurs in the data and is not something to author.</summary>
	private static readonly ActionType[] EditableTypes = [
		ActionType.SetParam, ActionType.SetExpression, ActionType.Math, ActionType.DoFunction, ActionType.RaiseEvent
	];

	private readonly VirtualMachine _vm;
	private readonly VmAction _action;
	private readonly ActionScope _scope;
	private readonly Expression? _originalExpression;

	private readonly TableLayoutPanel _root;
	private readonly Dictionary<ActionType, RadioButton> _typeButtons = [];
	private readonly TextBox _name;
	private readonly TargetObjectEditor _targetObject;
	private readonly ParamTargetEditor _targetParam;
	private readonly Label _calleeLabel;
	private readonly ComboBox _callee;
	private readonly Label _returnType;
	private readonly GroupBox _slotsGroup;
	private readonly TableLayoutPanel _slots;
	private readonly CheckBox _resultToggle;
	private readonly ResultTargetEditor _result;
	private readonly TextBox _preview;
	private readonly ToolTip _toolTip = new();

	private readonly List<ParameterSourceEditor> _slotEditors = [];
	private ActionType _selectedType = ActionType.SetParam;

	// The operation and the expression preview live inside the source-value group, which is
	// rebuilt from scratch whenever the shape of the action changes. So the operation is held
	// as a value rather than read off a control, and the preview box is whichever one the
	// current rebuild produced — null while no expression row is on screen.
	private MathOperationType _mathOperation = MathOperationType.None;
	private TextBox? _expressionPreview;

	private bool _loading = true;
	private bool _suppressCalleeEvents;

	private const int RowName = 0;
	private const int RowTargetObject = 1;
	private const int RowTargetParam = 2;
	private const int RowCallee = 3;
	private const int RowSlots = 4;
	private const int RowResult = 5;
	private const int RowPreview = 6;
	private const int TotalRows = 7;

	public ActionEditorForm(VirtualMachine vm, VmAction action) {
		_vm = vm;
		_action = action;
		_scope = ActionScope.For(action, vm);
		_originalExpression = action.SourceExpression;

		Text = $"Action {action.Id}   —   {ContextDescription()}";
		Size = new Size(1140, 800);
		MinimumSize = new Size(900, 600);
		StartPosition = FormStartPosition.CenterParent;

		_root = new TableLayoutPanel {
			Dock = DockStyle.Fill, ColumnCount = 2, RowCount = TotalRows, Padding = new Padding(12, 12, 12, 6)
		};
		_root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, LabelColumn));
		_root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
		for (var i = 0; i < TotalRows; i++)
			_root.RowStyles.Add(new RowStyle(SizeType.Absolute, RowHeight));
		_root.RowStyles[RowSlots] = new RowStyle(SizeType.Percent, 100);
		_root.RowStyles[RowPreview] = new RowStyle(SizeType.Absolute, 88);

		_name = Row(new TextBox());

		_targetObject = Row(new TargetObjectEditor(_vm, _scope));
		_targetParam = Row(new ParamTargetEditor(_vm,
			() => new TargetObjectBinding(_targetObject.EffectiveHolder, _targetObject.IsConcreteTarget)));
		_result = Row(new ResultTargetEditor(_vm, _scope));

		_targetObject.ValueChanged += (_, _) => OnTargetObjectChanged();
		_targetParam.ValueChanged += (_, _) => {
			PushExpectedTypeToSource();
			RefreshPreview();
		};
		_result.ValueChanged += (_, _) => RefreshPreview();

		_calleeLabel = NewLabel("Function");
		_callee = Row(NewCombo(ComboBoxStyle.DropDown));
		_callee.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
		_callee.AutoCompleteSource = AutoCompleteSource.ListItems;
		_callee.SelectedIndexChanged += (_, _) => OnCalleeChanged();

		// The return type is what decides where the result may be stored, so it is stated next
		// to the function rather than left for the user to infer from the parameter list.
		_returnType = NewLabel("");
		_returnType.ForeColor = SystemColors.GrayText;
		_returnType.Margin = new Padding(8, 4, 0, 4);

		var calleeRow = NewRowPanel(2);
		calleeRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
		calleeRow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 230));
		calleeRow.Controls.Add(_callee, 0, 0);
		calleeRow.Controls.Add(_returnType, 1, 0);

		_slots = new TableLayoutPanel {
			Dock = DockStyle.Fill, ColumnCount = 2, AutoScroll = true, Margin = Padding.Empty,
			Padding = new Padding(6, 4, 6, 4)
		};
		_slots.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, LabelColumn - 12));
		_slots.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
		_slotsGroup = new GroupBox {
			Text = "Parameters", Dock = DockStyle.Fill, Margin = new Padding(0, 6, 0, 6)
		};
		_slotsGroup.Controls.Add(_slots);

		// The toggle is the one place that says whether a result is stored: it pushes into the
		// editor and is never assigned back from it, so the checkbox the user sees and the value
		// that gets written cannot drift apart.
		_resultToggle = Row(new CheckBox { Text = "Store result in", AutoSize = false });
		_resultToggle.CheckedChanged += (_, _) => {
			_result.Storing = _resultToggle.Checked;
			RefreshPreview();
		};

		_preview = new TextBox {
			Dock = DockStyle.Fill, ReadOnly = true, Multiline = true, ScrollBars = ScrollBars.Vertical,
			Font = new Font(FontFamily.GenericMonospace, 8.5f), Margin = new Padding(0, 6, 0, 0)
		};

		AddRow(RowName, "Name", _name);
		AddRow(RowTargetObject, "Target object", _targetObject);
		AddRow(RowTargetParam, "Target param", _targetParam);
		AddRow(RowCallee, _calleeLabel, calleeRow);
		AddSpanningRow(RowSlots, _slotsGroup);
		AddRow(RowResult, _resultToggle, _result);
		AddSpanningRow(RowPreview, _preview);

		var split = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1 };
		split.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, TypeColumn));
		split.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
		split.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
		split.Controls.Add(BuildTypeSelector(), 0, 0);
		split.Controls.Add(_root, 1, 0);

		var buttons = new FlowLayoutPanel {
			Dock = DockStyle.Bottom, FlowDirection = FlowDirection.RightToLeft, Height = 52,
			Padding = new Padding(12, 8, 12, 8)
		};
		var cancel = new Button { Text = "Cancel", Size = new Size(110, 34), DialogResult = DialogResult.Cancel };
		var save = new Button { Text = "Save", Size = new Size(110, 34), Margin = new Padding(8, 0, 0, 0) };
		save.Click += (_, _) => Save();
		buttons.Controls.AddRange([cancel, save]);
		AcceptButton = save;
		CancelButton = cancel;

		Controls.Add(split);
		Controls.Add(buttons);

		LoadAction();
	}

	// ---------------------------------------------------------------- control factory

	/// <summary>
	/// Prepares a control to sit in one form row. It anchors sideways only and keeps a fixed
	/// height, so it stretches across the column but is never stretched down the cell: a row
	/// that shares a table with the tall parameters area would otherwise pull buttons and
	/// checkboxes to full height while the combo boxes stayed at the top, and squeeze combos
	/// short enough to clip in the rows that do fit.
	/// </summary>
	private static T Row<T>(T control) where T : Control {
		control.Anchor = AnchorStyles.Left | AnchorStyles.Right;
		control.Height = ContentHeight;
		control.Margin = new Padding(0, 4, 0, 4);
		return control;
	}

	private static ComboBox NewCombo(ComboBoxStyle style) =>
		new() { DropDownStyle = style, IntegralHeight = false };

	private static TableLayoutPanel NewRowPanel(int columns) {
		var panel = new TableLayoutPanel {
			ColumnCount = columns, RowCount = 1, Margin = Padding.Empty, Padding = Padding.Empty,
			Anchor = AnchorStyles.Left | AnchorStyles.Right, Height = ContentHeight + 8
		};
		panel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
		return panel;
	}

	private Label NewLabel(string text) {
		var label = Row(new Label {
			Text = text, TextAlign = ContentAlignment.MiddleLeft, AutoSize = false, AutoEllipsis = true
		});
		label.Margin = new Padding(0, 4, 8, 4);
		// Slot labels carry the declared type and can outrun the column; the full text stays
		// reachable rather than being silently clipped.
		_toolTip.SetToolTip(label, text);
		return label;
	}

	/// <summary>
	/// The action types, top-docked and sized to their own content: five radio buttons have no
	/// use for the height of the whole form, and stretching them only puts a field of empty box
	/// next to the rows that matter.
	/// </summary>
	private Control BuildTypeSelector() {
		var flow = new FlowLayoutPanel {
			Dock = DockStyle.Top, FlowDirection = FlowDirection.TopDown, WrapContents = false,
			AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Padding = new Padding(8, 6, 6, 6)
		};

		foreach (var type in EditableTypes) {
			var button = new RadioButton {
				Text = DescribeType(type), AutoSize = true, Margin = new Padding(0, 6, 0, 6)
			};
			var captured = type;
			button.CheckedChanged += (_, _) => {
				if (button.Checked) OnActionTypeChanged(captured);
			};
			_typeButtons[type] = button;
			flow.Controls.Add(button);
		}

		var group = new GroupBox {
			Text = "Action type", Dock = DockStyle.Top, AutoSize = true,
			AutoSizeMode = AutoSizeMode.GrowAndShrink, Margin = new Padding(12, 12, 0, 6)
		};
		group.Controls.Add(flow);

		// The group is docked inside a plain host so it keeps its own height instead of being
		// stretched to fill the table cell it sits in.
		var host = new Panel { Dock = DockStyle.Fill, Margin = Padding.Empty };
		host.Controls.Add(group);
		return host;
	}

	/// <summary>
	/// Says what the action does, not what its enum is called: every type both reads and writes
	/// something, and "Math" alone does not say that the thing it changes is the target
	/// parameter.
	/// </summary>
	private static string DescribeType(ActionType type) => type switch {
		ActionType.SetParam => "Set parameter to value",
		ActionType.SetExpression => "Set parameter to result of expression",
		ActionType.Math => "Perform math on parameter",
		ActionType.DoFunction => "Call function on object",
		ActionType.RaiseEvent => "Raise event on object",
		_ => type.ToString()
	};

	// ---------------------------------------------------------------- loading

	private void LoadAction() {
		_loading = true;
		try {
			_name.Text = _action.Name ?? "";
			_selectedType = EditableTypes.Contains(_action.ActionType) ? _action.ActionType : ActionType.SetParam;
			// Before the radio is checked: the operation row is built from this value.
			_mathOperation = _action.MathOperationType;
			_typeButtons[_selectedType].Checked = true;

			_targetObject.Load(_action.TargetObject);
			// A DoFunction action loads its TargetParam into the result editor instead, so the
			// param editor never hears about the target object unless it is told.
			_targetParam.RefreshForTarget();

			// TargetParam is one field with two meanings: for a function call it is where the
			// result goes, resolved against the local context's owner rather than the target.
			if (_selectedType == ActionType.DoFunction) {
				_result.Load(_action.TargetParam);
				// The only place the toggle is set from the data; from here on it drives.
				_resultToggle.Checked = _result.Storing;
			} else {
				_targetParam.Load(_action.TargetParam);
			}

			RefreshCallee(preserveSelection: false);
			BuildSlots(_action.GetParamStrings());
			UpdateRowVisibility();
		} finally {
			_loading = false;
		}
		RefreshPreview();
	}

	private string ContextDescription() {
		var context = _action.LocalContext.Element switch {
			State s => $"State {s.Name}",
			Graph g => $"Graph {g.Name}",
			Branch b => $"Branch {b.Name}",
			Talking t => $"Talking {t.Name}",
			Speech sp => $"Speech {sp.Name}",
			_ => "unknown context"
		};
		var owner = _scope.Owner != null ? $" on {_scope.Owner.Name}" : "";
		return context + owner;
	}

	private ActionType SelectedActionType => _selectedType;

	private void OnTargetObjectChanged() {
		// The target decides which parameters it has and which functions and events can be
		// called on it, so every dependent list is rebuilt whenever it moves.
		_targetParam.RefreshForTarget();

		var before = SelectedCallee?.Id ?? _callee.Text;
		RefreshCallee(preserveSelection: true);
		var after = SelectedCallee?.Id ?? _callee.Text;

		// Retargeting only invalidates the slot values if it changed what is being called;
		// otherwise the arguments are still the same arguments and are left alone.
		if (before != after) BuildSlots(null);
		else PushExpectedTypeToSource();

		UpdateRowVisibility();
		RefreshPreview();
	}

	// ---------------------------------------------------------------- callee (function / event)

	/// <summary>
	/// Rebuilds the function/event list and puts the selection back. Clearing a ComboBox
	/// raises SelectedIndexChanged, so the whole thing runs under suppression.
	/// </summary>
	/// <param name="preserveSelection">
	/// False across an action-type change. A function name and an event id are not the same
	/// namespace, and carrying "Storage.PickUpByTemplate" into the event list only invites the
	/// code that resolves an event id to be handed something that is not one.
	/// </param>
	private void RefreshCallee(bool preserveSelection) {
		var wanted = preserveSelection ? SelectedCallee?.Id ?? StoredCalleeId() : StoredCalleeId();
		_suppressCalleeEvents = true;
		try {
			PopulateCallee();
			_callee.SelectedIndex = -1;
			_callee.Text = "";
			SelectCallee(wanted);
		} finally {
			_suppressCalleeEvents = false;
		}
	}

	/// <summary>The callee stored on the action, but only for the type that actually owns it.</summary>
	private string? StoredCalleeId() => SelectedActionType switch {
		ActionType.DoFunction => _action.Function?.Name,
		ActionType.RaiseEvent => _action.EventToRaise?.Id.ToString(),
		_ => null
	};

	private void PopulateCallee() {
		_callee.Items.Clear();

		switch (SelectedActionType) {
			case ActionType.DoFunction:
				_calleeLabel.Text = "Function";
				foreach (var name in CallableFunctions())
					_callee.Items.Add(new CalleeItem(name, name, null));
				break;
			case ActionType.RaiseEvent:
				_calleeLabel.Text = "Event";
				foreach (var raisable in RaisableEvents())
					_callee.Items.Add(new CalleeItem(raisable.Id.ToString(),
						$"{raisable.Name}   ({raisable.Messages.Count} msg)   ← {OwnerName(raisable)}", raisable));
				break;
		}
	}

	/// <summary>
	/// Functions the target object can actually run. A function belongs to a functional
	/// component, so only the components on the target — its inherited ones included — are
	/// callable. When the target is only known at runtime there is no object to ask, and the
	/// full list is the only honest answer.
	/// </summary>
	private IEnumerable<string> CallableFunctions() {
		var components = _targetObject.ResolvedComponents;
		// Null means the target is only known at runtime and nothing declares what it is; an
		// empty set means it is known and has no components, which is a real answer.
		return components == null
			? FunctionSignature.AvailableNames
			: FunctionSignature.NamesForComponents(components);
	}

	private IEnumerable<Event> RaisableEvents() {
		var holder = _targetObject.ResolvedHolder;
		if (holder == null)
			return _vm.GetElementsByType<Event>().OrderBy(e => e.Name, StringComparer.Ordinal);
		return ActionScope.RaisableEvents(holder, _vm).OrderBy(e => e.Name, StringComparer.Ordinal);
	}

	private static string OwnerName(Event raisable) =>
		raisable.Parent.Element is INamedElement named ? named.Name : raisable.Parent.Element.GetType().Name;

	private void SelectCallee(string? id) {
		if (string.IsNullOrEmpty(id)) return;

		for (var i = 0; i < _callee.Items.Count; i++) {
			if (_callee.Items[i] is CalleeItem item && item.Id == id) {
				_callee.SelectedIndex = i;
				return;
			}
		}

		// The stored callee is outside the offered list — a function whose component the
		// target no longer has, say. Show it rather than silently dropping it on save.
		if (SelectedActionType == ActionType.RaiseEvent) {
			// Resolved without a type constraint: the generic overload throws when an id
			// belongs to some other kind of element, which is exactly the case being handled.
			if (ulong.TryParse(id, out var eventId) && _vm.GetNullableElement(eventId) is Event missing) {
				_callee.Items.Insert(0, new CalleeItem(id, $"{missing.Name}   (not visible from target)", missing));
				_callee.SelectedIndex = 0;
			}
		} else if (SelectedActionType == ActionType.DoFunction) {
			_callee.Items.Insert(0, new CalleeItem(id, $"{id}   (not on target object)", null));
			_callee.SelectedIndex = 0;
		}
	}

	private CalleeItem? SelectedCallee => _callee.SelectedItem as CalleeItem;

	private void OnCalleeChanged() {
		if (_loading || _suppressCalleeEvents) return;
		// A different function or event means different parameters; carrying the old values
		// across positionally would silently produce nonsense, so the slots start empty.
		BuildSlots(null);
		UpdateRowVisibility();
		RefreshPreview();
	}

	private void OnActionTypeChanged(ActionType type) {
		_selectedType = type;
		if (_loading) return;
		// Switching away from a function call reveals the target-param row, whose kinds are
		// derived from the target object; it has to re-derive them before being shown.
		_targetParam.RefreshForTarget();
		RefreshCallee(preserveSelection: false);
		BuildSlots(null);
		UpdateRowVisibility();
		RefreshPreview();
	}

	// ---------------------------------------------------------------- value slots

	/// <summary>
	/// Rebuilds the value rows for the current action type: one per declared function
	/// parameter, one per message the selected event carries, or the single source of a
	/// SetParam or Math action. <paramref name="existing"/> is the stored values when loading
	/// and null whenever the shape changed, since values do not carry across a new signature.
	/// </summary>
	private void BuildSlots(IReadOnlyList<string>? existing) {
		existing ??= [];

		_slots.SuspendLayout();
		foreach (var control in _slots.Controls.Cast<Control>().ToList()) {
			_slots.Controls.Remove(control);
			control.Dispose();
		}
		_slots.RowStyles.Clear();
		_slots.RowCount = 0;
		_slotEditors.Clear();
		_expressionPreview = null;

		switch (SelectedActionType) {
			case ActionType.SetParam:
				_slotsGroup.Text = "Source value";
				AddSlot("Source", _targetParam.ResolvedType, ValueAt(existing, 0), _targetParam.Value);
				break;
			case ActionType.Math:
				// The operation belongs with the value it applies: "+" and the number added are
				// one thought, and reading them a row apart with the target between them is not.
				_slotsGroup.Text = "Source value";
				AddOperationRow();
				AddSlot("Source", _targetParam.ResolvedType, ValueAt(existing, 0), _targetParam.Value);
				break;
			case ActionType.SetExpression:
				_slotsGroup.Text = "Source value";
				AddExpressionRow();
				break;
			case ActionType.DoFunction:
				_slotsGroup.Text = "Function parameters";
				BuildFunctionSlots(existing);
				break;
			case ActionType.RaiseEvent:
				_slotsGroup.Text = "Event messages";
				BuildEventSlots(existing);
				break;
			default:
				_slotsGroup.Text = "Parameters";
				break;
		}

		// Leftover height goes to a spacer rather than inflating the last real row.
		_slots.RowCount += 1;
		_slots.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

		_slots.ResumeLayout();
	}

	private void BuildFunctionSlots(IReadOnlyList<string> existing) {
		var signature = CurrentSignature(existing);
		if (signature == null) {
			var name = SelectedCallee?.Id ?? _callee.Text;
			AddNote(string.IsNullOrEmpty(name) ? "Select a function." : $"Unknown function '{name}'.");
			return;
		}
		if (signature.Slots.Count == 0) {
			AddNote($"{signature.Name} takes no parameters.");
			return;
		}

		foreach (var slot in signature.Slots)
			AddSlot($"{slot.Name}   [{Describe(slot.Type)}]", slot.Type, ValueAt(existing, slot.Index), null,
				slot.Constraint);
	}

	private FunctionSignature? CurrentSignature(IReadOnlyList<string>? existing = null) {
		if (SelectedActionType != ActionType.DoFunction) return null;
		var name = SelectedCallee?.Id ?? _callee.Text;
		return string.IsNullOrEmpty(name) ? null : FunctionSignature.Of(name, _vm, existing);
	}

	private void BuildEventSlots(IReadOnlyList<string> existing) {
		var raisable = SelectedCallee?.Event;
		if (raisable == null) {
			AddNote("Select an event.");
			return;
		}
		if (raisable.Messages.Count == 0) {
			AddNote($"{raisable.Name} carries no messages.");
			return;
		}

		for (var i = 0; i < raisable.Messages.Count; i++) {
			var message = raisable.Messages[i];
			AddSlot($"{message.ParamName}   [{message.Type}]", SafeTypeInfo(message.Type), ValueAt(existing, i), null);
		}
	}

	private void AddSlot(string label, VmTypeInfo? expectedType, string value, ParamTarget? target,
		SlotConstraint? constraint = null) {
		var editor = Row(new ParameterSourceEditor(_vm, _scope, expectedType, target));
		if (constraint != null) editor.Constraint = constraint;
		editor.ValueChanged += (_, _) => RefreshPreview();
		if (!string.IsNullOrEmpty(value)) {
			try {
				// Input-param references resolve against VirtualMachine.FillScope; without it
				// they degrade to literals and the editor shows the wrong kind.
				using var fillScope = VirtualMachine.EnterFillScope(_scope.LocalContext);
				editor.Load(ParameterSource.Create(value, _vm, target, expectedType), value);
			} catch {
				// A value the parser rejects still has to be visible and editable, so it is
				// shown verbatim rather than dropped.
				editor.LoadRaw(value);
			}
		}
		_slotEditors.Add(editor);

		var row = NewSlotRow();
		_slots.Controls.Add(NewLabel(label), 0, row);
		_slots.Controls.Add(editor, 1, row);
	}

	/// <summary>
	/// The math operation, as one radio per operation in equal columns.
	///
	/// ACTION_OPERATION_TYPE_NONE is not offered: it is what an unset field reads as, and no
	/// Math action in either corpus carries it. A Math action that still has it is caught on
	/// save rather than written out as an operation the engine cannot perform.
	/// </summary>
	private void AddOperationRow() {
		var operations = Enum.GetValues<MathOperationType>().Where(o => o != MathOperationType.None).ToList();

		var strip = new TableLayoutPanel {
			ColumnCount = operations.Count, RowCount = 1, Dock = DockStyle.Fill,
			Margin = Padding.Empty, Padding = Padding.Empty
		};
		strip.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
		foreach (var operation in operations) {
			// Equal columns, so the options read as one set of choices rather than as text of
			// varying length that happens to have buttons in front of it.
			strip.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / operations.Count));
			var button = new RadioButton {
				Text = DescribeOperation(operation), Dock = DockStyle.Fill, AutoSize = false,
				TextAlign = ContentAlignment.MiddleLeft, Checked = _mathOperation == operation
			};
			var captured = operation;
			button.CheckedChanged += (_, _) => {
				if (!button.Checked) return;
				_mathOperation = captured;
				RefreshPreview();
			};
			strip.Controls.Add(button, strip.Controls.Count, 0);
		}

		var row = NewSlotRow();
		_slots.Controls.Add(NewLabel("Operation"), 0, row);
		_slots.Controls.Add(strip, 1, row);
	}

	private static string DescribeOperation(MathOperationType operation) => operation switch {
		MathOperationType.Addition => "Add",
		MathOperationType.Subtraction => "Subtract",
		MathOperationType.Multiply => "Multiply",
		MathOperationType.Division => "Divide",
		_ => operation.ToString()
	};

	/// <summary>
	/// The expression a SetExpression action reads from. It is the action's source value, so it
	/// sits where every other source value does rather than in a row of its own.
	/// </summary>
	private void AddExpressionRow() {
		_expressionPreview = new TextBox { Dock = DockStyle.Fill, ReadOnly = true, Margin = new Padding(0, 4, 6, 4) };
		var edit = new Button { Dock = DockStyle.Fill, Text = "Edit expression…", Margin = new Padding(0, 4, 0, 4) };
		edit.Click += (_, _) => EditExpression();

		var panel = new TableLayoutPanel {
			ColumnCount = 2, RowCount = 1, Dock = DockStyle.Fill, Margin = Padding.Empty, Padding = Padding.Empty
		};
		panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
		panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 170));
		panel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
		panel.Controls.Add(_expressionPreview, 0, 0);
		panel.Controls.Add(edit, 1, 0);

		var row = NewSlotRow();
		_slots.Controls.Add(NewLabel("Expression"), 0, row);
		_slots.Controls.Add(panel, 1, row);
		UpdateExpressionPreview();
	}

	private void AddNote(string text) {
		var row = NewSlotRow();
		var note = NewLabel(text);
		note.ForeColor = SystemColors.GrayText;
		_slots.Controls.Add(note, 0, row);
		_slots.SetColumnSpan(note, 2);
	}

	private int NewSlotRow() {
		var row = _slots.RowCount;
		_slots.RowCount = row + 1;
		_slots.RowStyles.Add(new RowStyle(SizeType.Absolute, RowHeight));
		return row;
	}

	private static string ValueAt(IReadOnlyList<string> values, int index) =>
		index >= 0 && index < values.Count ? values[index] ?? "" : "";

	private VmTypeInfo? SafeTypeInfo(string xmlType) {
		try {
			return VmTypeHelper.GetVmTypeInfo(xmlType, _vm);
		} catch {
			return null;
		}
	}

	private static string Describe(VmTypeInfo? type) {
		try {
			return type?.Serialize() ?? "?";
		} catch {
			return "?";
		}
	}

	private void PushExpectedTypeToSource() {
		if (SelectedActionType is not (ActionType.SetParam or ActionType.Math)) return;
		if (_slotEditors.Count == 0) return;
		_slotEditors[0].Target = _targetParam.Value;
		_slotEditors[0].ExpectedType = _targetParam.ResolvedType;
	}

	// ---------------------------------------------------------------- layout

	private void AddRow(int row, string label, Control control) => AddRow(row, NewLabel(label), control);

	private void AddRow(int row, Control label, Control control) {
		_root.Controls.Add(label, 0, row);
		_root.Controls.Add(control, 1, row);
	}

	private void AddSpanningRow(int row, Control control) {
		_root.Controls.Add(control, 0, row);
		_root.SetColumnSpan(control, 2);
	}

	private void UpdateRowVisibility() {
		var type = SelectedActionType;
		var signature = CurrentSignature();
		var storesResult = type == ActionType.DoFunction && signature is { IsVoid: false };

		// A function call writes TargetParam too, but as a result destination bound against
		// the local context rather than the target object — that is the "Store result in" row,
		// and showing both would be two controls fighting over one field.
		SetRowVisible(RowTargetParam,
			type is ActionType.SetParam or ActionType.Math or ActionType.SetExpression);
		SetRowVisible(RowCallee, type is ActionType.DoFunction or ActionType.RaiseEvent);
		SetRowVisible(RowResult, storesResult);

		if (storesResult) _result.ExpectedType = signature!.ReturnTypeInfo;

		_returnType.Text = type == ActionType.DoFunction && signature != null
			? signature.IsVoid ? "returns nothing" : $"returns {Describe(signature.ReturnTypeInfo)}"
			: "";
	}

	/// <summary>
	/// Collapses a row to zero height rather than merely hiding its controls, so an
	/// inapplicable field leaves no gap behind. Only the fixed-height rows are ever toggled:
	/// the source-value group applies to every action type and keeps the leftover height.
	/// </summary>
	private void SetRowVisible(int row, bool visible) {
		_root.RowStyles[row] = new RowStyle(SizeType.Absolute, visible ? RowHeight : 0);

		foreach (Control control in _root.Controls)
			if (_root.GetCellPosition(control).Row == row)
				control.Visible = visible;
	}

	// ---------------------------------------------------------------- expression

	/// <summary>
	/// A SetExpression action is meaningless without one, so it is created on demand — when
	/// the user opens the expression editor, and on save. Every other type discards it, which
	/// is what keeps a retyped action from carrying a SourceExpression the engine never reads.
	/// </summary>
	private void EnsureExpression() {
		if (SelectedActionType != ActionType.SetExpression) return;
		if (_action.SourceExpression != null) return;

		var expression = VmElement.CreateDefault<Expression>(_vm, _action);
		// Expression.New leaves TargetObject default-constructed, which reads as a Holder with
		// no holder behind it — the expression editor writes that field out on open and would
		// dereference the null. Pointing a new expression at the action's own owner both avoids
		// that and is the sensible default.
		if (_scope.Owner != null)
			expression.TargetObject = TargetObject.Read(_scope.Owner.ParamId, _vm, _scope.LocalContext);
		_action.SourceExpression = expression;
	}

	private void EditExpression() {
		EnsureExpression();
		if (_action.SourceExpression == null) return;
		using var editor = new ExpressionEditorForm(_vm, _action.SourceExpression);
		editor.ShowDialog(this);
		UpdateExpressionPreview();
		RefreshPreview();
	}

	/// <summary>
	/// No-op unless an expression row is on screen: the box belongs to the source-value group
	/// and only exists while a SetExpression action is being edited.
	/// </summary>
	private void UpdateExpressionPreview() {
		if (_expressionPreview == null) return;
		_expressionPreview.Text = _action.SourceExpression == null
			? "(none)"
			: $"{PreviewHelper.Preview(_action.SourceExpression)}   [id {_action.SourceExpression.Id}]";
	}

	// ---------------------------------------------------------------- preview

	private void RefreshPreview() {
		if (_loading) return;
		var lines = new List<string> {
			$"TargetFuncName  {CalleeText()}",
			$"TargetObject    {_targetObject.SerializedValue}",
			$"TargetParam     {TargetParamText()}"
		};
		if (SelectedActionType == ActionType.SetExpression)
			lines.Add($"SourceExpression  {_action.SourceExpression?.Id.ToString() ?? "(none)"}");
		for (var i = 0; i < _slotEditors.Count; i++)
			lines.Add($"SourceParams[{i}]  {_slotEditors[i].SerializedValue}");
		_preview.Text = string.Join(Environment.NewLine, lines);
	}

	private string CalleeText() => SelectedActionType switch {
		ActionType.DoFunction => SelectedCallee?.Id ?? _callee.Text,
		ActionType.RaiseEvent => SelectedCallee?.Id ?? "",
		_ => ""
	};

	/// <summary>
	/// TargetParam comes from whichever control owns it for this action type — the result
	/// destination for a function call, the plain target otherwise. A void call has no
	/// destination, and the engine ignores one, so it is written empty.
	/// </summary>
	private string TargetParamText() {
		if (SelectedActionType != ActionType.DoFunction) return _targetParam.SerializedValue;
		return CurrentSignature() is { IsVoid: false } && _resultToggle.Checked ? _result.SerializedValue : "%";
	}

	// ---------------------------------------------------------------- saving

	/// <summary>
	/// Why the action cannot be saved as it stands, or null when it can. Anything that would be
	/// a guaranteed runtime error is refused here rather than written out.
	/// </summary>
	private string? ValidationError(out FunctionSignature? signature) {
		signature = null;
		var type = SelectedActionType;

		if (type == ActionType.DoFunction) {
			var name = SelectedCallee?.Id ?? _callee.Text;
			signature = string.IsNullOrEmpty(name) ? null : FunctionSignature.Of(name, _vm);
			if (signature == null) return $"'{name}' is not a known function.";
			if (!signature.IsVoid && _resultToggle.Checked && _result.ValidationError is { } error) return error;
		}

		if (type == ActionType.RaiseEvent && SelectedCallee?.Event == null)
			return "Select an event to raise.";

		if (type == ActionType.Math && _mathOperation == MathOperationType.None)
			return "Choose the operation to perform.";

		// Every action in both corpora names one, and the writer calls TargetObject.Write()
		// unconditionally — an unset one takes down the whole save, not just this action.
		if (!_targetObject.Value.IsSet)
			return "Choose the object this action runs on.";

		return TargetParamError(type);
	}

	/// <summary>
	/// The destination this action would write to, against the two things the data never says.
	///
	/// A SetParam, Math or SetExpression action always names one: all 11610 empty TargetParams
	/// in PathologicSandbox and 1232 in MarbleNest belong to void function calls and raised
	/// events, which write nowhere by design. And nothing writes into an expression's constant
	/// — see <see cref="Parameter.IsConstant"/> — whatever route it was reached by.
	/// </summary>
	private string? TargetParamError(ActionType type) {
		var needsTarget = type is ActionType.SetParam or ActionType.Math or ActionType.SetExpression;
		var missing = needsTarget ? "Choose the parameter this action writes to." : null;

		if (!ParamTarget.TryRead(TargetParamText(), _vm, out var target)) return missing;
		if (target.Kind == ParamTargetKind.Empty) return missing;

		return target.Parameter?.Element is Parameter { IsConstant: true } constant
			? $"'{constant.Name}' holds an expression's constant value and cannot be written to."
			: null;
	}

	private void Save() {
		if (ValidationError(out var signature) is { } problem) {
			Reject(problem);
			return;
		}

		var type = SelectedActionType;
		_action.Name = _name.Text;
		_action.ActionType = type;
		_action.MathOperationType = type == ActionType.Math ? _mathOperation : MathOperationType.None;

		_action.TargetObject = _targetObject.Value;
		_action.TargetParam = ParamTarget.TryRead(TargetParamText(), _vm, out var targetParam)
			? targetParam
			: ParamTarget.Empty();

		// Every type owns a different subset of the payload fields, and the writer emits
		// whatever is non-null, so the ones this type does not own are cleared rather than
		// left to leak into the XML.
		switch (type) {
			case ActionType.SetParam:
			case ActionType.Math:
				_action.Source = _slotEditors.Count > 0 ? _slotEditors[0].Value : (ParameterSource?)null;
				_action.Function = null;
				_action.EventToRaise = null;
				_action.EventParams = null;
				DiscardExpression();
				_action.ClearRawTargetFuncName();
				break;
			case ActionType.SetExpression:
				// Created here rather than when the radio is picked, so opening the form,
				// looking at SetExpression and cancelling does not leave an orphan behind.
				EnsureExpression();
				_action.Source = null;
				_action.Function = null;
				_action.EventToRaise = null;
				_action.EventParams = null;
				_action.ClearRawTargetFuncName();
				break;
			case ActionType.DoFunction:
				_action.Function = FunctionSignature.Create(signature!.Name, _vm,
					_slotEditors.Select(e => e.SerializedValue).ToList());
				_action.Source = null;
				_action.EventToRaise = null;
				_action.EventParams = null;
				DiscardExpression();
				break;
			case ActionType.RaiseEvent:
				_action.Source = null;
				_action.Function = null;
				_action.EventToRaise = SelectedCallee!.Event;
				_action.EventParams = _slotEditors.Select(e => e.Value).ToList();
				DiscardExpression();
				break;
		}

		DialogResult = DialogResult.OK;
	}

	private void Reject(string message) =>
		MessageBox.Show(this, message, "Cannot save", MessageBoxButtons.OK, MessageBoxIcon.Warning);

	/// <summary>
	/// An expression only belongs to a SetExpression action; leaving it attached after a
	/// retype would keep writing a SourceExpression the engine never reads.
	/// </summary>
	private void DiscardExpression() {
		if (_action.SourceExpression == null) return;
		_vm.RemoveElement(_action.SourceExpression);
		_action.SourceExpression = null;
	}

	/// <summary>
	/// Everything else on the action is written in Save, but an expression has to exist before
	/// the expression editor can be opened on it, so it is created against the live machine
	/// mid-edit. Cancelling has to take that back — otherwise dismissing the form still leaves
	/// a new Expression registered and a SourceExpression on the action.
	///
	/// Edits made to an expression the action already had are not rolled back: the expression
	/// editor writes through to the same objects, and undoing that needs a general undo rather
	/// than a special case here.
	/// </summary>
	protected override void OnFormClosed(FormClosedEventArgs e) {
		if (DialogResult != DialogResult.OK && !ReferenceEquals(_action.SourceExpression, _originalExpression)) {
			if (_action.SourceExpression != null) _vm.RemoveElement(_action.SourceExpression);
			_action.SourceExpression = _originalExpression;
		}
		base.OnFormClosed(e);
	}

	protected override void Dispose(bool disposing) {
		if (disposing) _toolTip.Dispose();
		base.Dispose(disposing);
	}

	private sealed class CalleeItem(string id, string label, Event? raisable) {
		public string Id { get; } = id;
		public Event? Event { get; } = raisable;
		public override string ToString() => label;
	}
}
