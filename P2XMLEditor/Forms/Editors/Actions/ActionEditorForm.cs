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
	private const int RowHeight = 34;
	private const int LabelColumn = 150;

	private readonly VirtualMachine _vm;
	private readonly VmAction _action;
	private readonly ActionScope _scope;

	private readonly TableLayoutPanel _root;
	private readonly TextBox _name;
	private readonly ComboBox _actionType;
	private readonly ComboBox _mathOperation;
	private readonly TargetObjectEditor _targetObject;
	private readonly ParamTargetEditor _targetParam;
	private readonly Label _calleeLabel;
	private readonly ComboBox _callee;
	private readonly CheckBox _allCallees;
	private readonly TableLayoutPanel _slots;
	private readonly Panel _expressionPanel;
	private readonly ResultTargetEditor _result;
	private readonly TextBox _preview;

	private readonly List<ParameterSourceEditor> _slotEditors = [];
	private bool _loading = true;
	private bool _suppressCalleeEvents;

	private const int RowName = 0;
	private const int RowType = 1;
	private const int RowOperation = 2;
	private const int RowTargetObject = 3;
	private const int RowTargetParam = 4;
	private const int RowCallee = 5;
	private const int RowSlots = 6;
	private const int RowExpression = 7;
	private const int RowResult = 8;
	private const int RowPreview = 9;
	private const int TotalRows = 10;

	public ActionEditorForm(VirtualMachine vm, VmAction action) {
		_vm = vm;
		_action = action;
		_scope = ActionScope.For(action, vm);

		Text = $"Action {action.Id}   —   {ContextDescription()}";
		Size = new Size(1080, 760);
		MinimumSize = new Size(820, 560);
		StartPosition = FormStartPosition.CenterParent;

		_root = new TableLayoutPanel {
			Dock = DockStyle.Fill,
			ColumnCount = 2,
			RowCount = TotalRows,
			Padding = new Padding(12, 12, 12, 6)
		};
		_root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, LabelColumn));
		_root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
		for (var i = 0; i < TotalRows; i++)
			_root.RowStyles.Add(new RowStyle(SizeType.Absolute, RowHeight));
		_root.RowStyles[RowSlots] = new RowStyle(SizeType.Percent, 100);
		_root.RowStyles[RowPreview] = new RowStyle(SizeType.Absolute, 84);

		_name = NewTextBox();

		_actionType = NewCombo(ComboBoxStyle.DropDownList);
		foreach (var type in Enum.GetValues<ActionType>())
			_actionType.Items.Add(type);
		_actionType.SelectedIndexChanged += (_, _) => OnActionTypeChanged();

		_mathOperation = NewCombo(ComboBoxStyle.DropDownList);
		foreach (var operation in Enum.GetValues<MathOperationType>())
			_mathOperation.Items.Add(operation);
		_mathOperation.SelectedIndexChanged += (_, _) => RefreshPreview();

		_targetObject = new TargetObjectEditor(_vm, _scope) { Dock = DockStyle.Fill };
		_targetParam = new ParamTargetEditor(_vm, () => _targetObject.ResolvedHolder) { Dock = DockStyle.Fill };
		_result = new ResultTargetEditor(_vm, _scope) { Dock = DockStyle.Fill };

		_targetObject.ValueChanged += (_, _) => OnTargetObjectChanged();
		_targetParam.ValueChanged += (_, _) => {
			PushExpectedTypeToSource();
			RefreshPreview();
		};
		_result.ValueChanged += (_, _) => RefreshPreview();

		_calleeLabel = NewLabel("Function");
		_callee = NewCombo(ComboBoxStyle.DropDown);
		_callee.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
		_callee.AutoCompleteSource = AutoCompleteSource.ListItems;
		_callee.SelectedIndexChanged += (_, _) => OnCalleeChanged();

		_allCallees = new CheckBox { Text = "show all", AutoSize = true, Dock = DockStyle.Left, Margin = new Padding(8, 6, 0, 0) };
		_allCallees.CheckedChanged += (_, _) => RefreshCallee();

		var calleeRow = new TableLayoutPanel {
			Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, Margin = Padding.Empty, Padding = Padding.Empty
		};
		calleeRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
		calleeRow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
		calleeRow.Controls.Add(_callee, 0, 0);
		calleeRow.Controls.Add(_allCallees, 1, 0);

		_slots = new TableLayoutPanel {
			Dock = DockStyle.Fill, ColumnCount = 2, AutoScroll = true, Margin = new Padding(0, 6, 0, 6)
		};
		_slots.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 260));
		_slots.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

		var editExpression = new Button { Text = "Edit expression…", Width = 180, Height = 28, Anchor = AnchorStyles.Left };
		editExpression.Click += (_, _) => EditExpression();
		_expressionPanel = new Panel { Dock = DockStyle.Fill };
		_expressionPanel.Controls.Add(editExpression);

		_preview = new TextBox {
			Dock = DockStyle.Fill, ReadOnly = true, Multiline = true, ScrollBars = ScrollBars.Vertical,
			Font = new Font(FontFamily.GenericMonospace, 8.5f), Margin = new Padding(0, 6, 0, 0)
		};

		AddRow(RowName, "Name", _name);
		AddRow(RowType, "Action type", _actionType);
		AddRow(RowOperation, "Operation", _mathOperation);
		AddRow(RowTargetObject, "Target object", _targetObject);
		AddRow(RowTargetParam, "Target param", _targetParam);
		AddRow(RowCallee, _calleeLabel, calleeRow);
		AddSpanningRow(RowSlots, _slots);
		AddSpanningRow(RowExpression, _expressionPanel);
		AddRow(RowResult, "Save result in", _result);
		AddSpanningRow(RowPreview, _preview);

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

		Controls.Add(_root);
		Controls.Add(buttons);

		LoadAction();
	}

	private static ComboBox NewCombo(ComboBoxStyle style) => new() {
		Dock = DockStyle.Fill, DropDownStyle = style, IntegralHeight = false, Margin = new Padding(0, 3, 0, 3)
	};

	private static TextBox NewTextBox() => new() { Dock = DockStyle.Fill, Margin = new Padding(0, 3, 0, 3) };

	private static Label NewLabel(string text) => new() {
		Text = text, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, Margin = new Padding(0, 3, 6, 3)
	};

	// ---------------------------------------------------------------- loading

	private void LoadAction() {
		_loading = true;
		try {
			_name.Text = _action.Name ?? "";
			_actionType.SelectedItem = _action.ActionType;
			_mathOperation.SelectedItem = _action.MathOperationType;

			_targetObject.Load(_action.TargetObject);

			// TargetParam is one field with two meanings: for a function call it is where the
			// result goes, resolved against the local context's owner rather than the target.
			if (_action.ActionType == ActionType.DoFunction)
				_result.Load(_action.TargetParam);
			else
				_targetParam.Load(_action.TargetParam);

			RefreshCallee();
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

	private ActionType SelectedActionType =>
		_actionType.SelectedItem is ActionType type ? type : ActionType.None;

	private void OnTargetObjectChanged() {
		// The target decides which parameters it has and which functions and events can be
		// called on it, so every dependent list is rebuilt whenever it moves.
		_targetParam.RefreshForTarget();

		var before = SelectedCallee?.Id ?? _callee.Text;
		RefreshCallee();
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
	/// raises SelectedIndexChanged, so the whole thing runs under suppression — otherwise
	/// merely retargeting the action would rebuild the slots twice.
	/// </summary>
	private void RefreshCallee() {
		var wanted = SelectedCallee?.Id ?? StoredCalleeId();
		_suppressCalleeEvents = true;
		try {
			PopulateCallee();
			SelectCallee(wanted);
		} finally {
			_suppressCalleeEvents = false;
		}
	}

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
		if (_allCallees.Checked) return FunctionSignature.AvailableNames;

		var components = ActionScope.ComponentsOf(_targetObject.ResolvedHolder);
		if (components.Count == 0) return FunctionSignature.AvailableNames;
		return FunctionSignature.NamesForComponents(components);
	}

	private IEnumerable<Event> RaisableEvents() {
		var holder = _targetObject.ResolvedHolder;
		if (_allCallees.Checked || holder == null)
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
		if (SelectedActionType == ActionType.RaiseEvent &&
			_vm.GetNullableElement<Event>(ulong.TryParse(id, out var eventId) ? eventId : 0) is { } missing) {
			_callee.Items.Insert(0, new CalleeItem(id, $"{missing.Name}   (not visible from target)", missing));
			_callee.SelectedIndex = 0;
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

	private void OnActionTypeChanged() {
		if (_loading) return;
		RefreshCallee();
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

		switch (SelectedActionType) {
			case ActionType.SetParam:
			case ActionType.Math:
				AddSlot("Source", _targetParam.ResolvedType, ValueAt(existing, 0), _targetParam.Value);
				break;
			case ActionType.DoFunction:
				BuildFunctionSlots(existing);
				break;
			case ActionType.RaiseEvent:
				BuildEventSlots(existing);
				break;
		}

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
			AddSlot($"{slot.Name}   [{Describe(slot.Type)}]", slot.Type, ValueAt(existing, slot.Index), null);
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

	private void AddSlot(string label, VmTypeInfo? expectedType, string value, ParamTarget? target) {
		var editor = new ParameterSourceEditor(_vm, _scope, expectedType, target) { Dock = DockStyle.Fill };
		editor.ValueChanged += (_, _) => RefreshPreview();
		if (!string.IsNullOrEmpty(value)) {
			try {
				editor.Load(ParameterSource.Create(value, _vm, target, expectedType), value);
			} catch {
				// A value the parser rejects still has to be visible and editable, so it is
				// shown verbatim rather than dropped.
				editor.LoadRaw(value);
			}
		}
		_slotEditors.Add(editor);
		AddSlotRow(label, editor);
	}

	private void AddSlotRow(string label, Control editor) {
		var row = NewSlotRow();
		_slots.Controls.Add(NewLabel(label), 0, row);
		_slots.Controls.Add(editor, 1, row);
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

	private void AddRow(int row, Label label, Control control) {
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

		SetRowVisible(RowOperation, type == ActionType.Math);
		// A function call writes TargetParam too, but as a result destination bound against
		// the local context rather than the target object — that is the "Save result in" row,
		// and showing both would be two controls fighting over one field.
		SetRowVisible(RowTargetParam,
			type is ActionType.SetParam or ActionType.Math or ActionType.SetExpression);
		SetRowVisible(RowCallee, type is ActionType.DoFunction or ActionType.RaiseEvent);
		SetRowVisible(RowExpression, type == ActionType.SetExpression);
		SetRowVisible(RowResult, type == ActionType.DoFunction && signature is { IsVoid: false });

		if (signature is { IsVoid: false }) _result.ExpectedType = signature.ReturnTypeInfo;
		_allCallees.Visible = type is ActionType.DoFunction or ActionType.RaiseEvent;
	}

	/// <summary>
	/// Collapses a row to zero height rather than merely hiding its controls, so an
	/// inapplicable field leaves no gap behind.
	/// </summary>
	private void SetRowVisible(int row, bool visible) {
		_root.RowStyles[row] = new RowStyle(SizeType.Absolute, visible ? RowHeight : 0);
		foreach (Control control in _root.Controls)
			if (_root.GetCellPosition(control).Row == row)
				control.Visible = visible;
	}

	// ---------------------------------------------------------------- expression

	private void EditExpression() {
		_action.SourceExpression ??= VmElement.CreateDefault<Expression>(_vm, _action);
		using var editor = new ExpressionEditorForm(_vm, _action.SourceExpression);
		editor.ShowDialog(this);
		RefreshPreview();
	}

	// ---------------------------------------------------------------- preview

	private void RefreshPreview() {
		if (_loading) return;
		var lines = new List<string> {
			$"TargetFuncName  {CalleeText()}",
			$"TargetObject    {_targetObject.SerializedValue}",
			$"TargetParam     {TargetParamText()}"
		};
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
		return CurrentSignature() is { IsVoid: false } ? _result.SerializedValue : "%";
	}

	// ---------------------------------------------------------------- saving

	private void Save() {
		var type = SelectedActionType;
		FunctionSignature? signature = null;

		if (type == ActionType.DoFunction) {
			var name = SelectedCallee?.Id ?? _callee.Text;
			signature = string.IsNullOrEmpty(name) ? null : FunctionSignature.Of(name, _vm);
			if (signature == null) {
				MessageBox.Show(this, $"'{name}' is not a known function.", "Cannot save", MessageBoxButtons.OK,
					MessageBoxIcon.Warning);
				return;
			}
		}

		if (type == ActionType.RaiseEvent && SelectedCallee?.Event == null) {
			MessageBox.Show(this, "Select an event to raise.", "Cannot save", MessageBoxButtons.OK,
				MessageBoxIcon.Warning);
			return;
		}

		_action.Name = _name.Text;
		_action.ActionType = type;
		_action.MathOperationType = type == ActionType.Math
			? (_mathOperation.SelectedItem as MathOperationType?) ?? MathOperationType.None
			: MathOperationType.None;

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
			default:
				_action.ClearRawTargetFuncName();
				break;
		}

		DialogResult = DialogResult.OK;
		Close();
	}

	/// <summary>
	/// An expression only belongs to a SetExpression action; leaving it attached after a
	/// retype would keep writing a SourceExpression the engine never reads.
	/// </summary>
	private void DiscardExpression() {
		if (_action.SourceExpression == null) return;
		_vm.RemoveElement(_action.SourceExpression);
		_action.SourceExpression = null;
	}

	private sealed class CalleeItem(string id, string label, Event? raisable) {
		public string Id { get; } = id;
		public Event? Event { get; } = raisable;
		public override string ToString() => label;
	}
}
