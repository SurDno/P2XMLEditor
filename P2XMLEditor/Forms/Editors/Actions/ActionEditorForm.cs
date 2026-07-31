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
using P2XMLEditor.GameData.VirtualMachineElements.Interfaces;
using P2XMLEditor.GameData.VirtualMachineElements.Helper;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes;
using P2XMLEditor.Helper;
using VmAction = P2XMLEditor.GameData.VirtualMachineElements.Action;

namespace P2XMLEditor.Forms.Editors.Actions;

/// <summary>
/// Edits a single <see cref="VmAction"/>.
///
/// The form is driven by two things it works out up front: the action's
/// <see cref="ActionScope"/>, which decides which messages, input parameters and loop
/// variables can be referenced here at all, and the declared type of whatever the action is
/// writing to or calling, which decides what each value slot will accept. Both flow into the
/// child editors, so selecting a function or an event reshapes the slots beneath it, and
/// changing the target parameter re-types the source next to it.
/// </summary>
public sealed class ActionEditorForm : Form {
	private const int RowHeight = 30;

	private readonly VirtualMachine _vm;
	private readonly VmAction _action;
	private readonly ActionScope _scope;

	private readonly TableLayoutPanel _root;
	private readonly TextBox _name;
	private readonly CheckBox _enabled;
	private readonly ComboBox _actionType;
	private readonly ComboBox _mathOperation;
	private readonly TargetObjectEditor _targetObject;
	private readonly ParamTargetEditor _targetParam;
	private readonly Label _calleeLabel;
	private readonly ComboBox _callee;
	private readonly CheckBox _allEvents;
	private readonly TableLayoutPanel _slots;
	private readonly Panel _expressionPanel;
	private readonly Button _editExpression;
	private readonly TextBox _preview;

	private readonly List<ParameterSourceEditor> _slotEditors = [];
	private bool _loading = true;
	private bool _suppressCalleeEvents;

	private const int RowName = 0;
	private const int RowEnabled = 1;
	private const int RowType = 2;
	private const int RowOperation = 3;
	private const int RowTargetObject = 4;
	private const int RowTargetParam = 5;
	private const int RowCallee = 6;
	private const int RowSlots = 7;
	private const int RowExpression = 8;
	private const int RowPreview = 9;

	public ActionEditorForm(VirtualMachine vm, VmAction action) {
		_vm = vm;
		_action = action;
		_scope = ActionScope.For(action, vm);

		Text = $"Action {action.Id}   —   {ContextDescription()}";
		Size = new Size(960, 680);
		MinimumSize = new Size(720, 480);
		StartPosition = FormStartPosition.CenterParent;

		_root = new TableLayoutPanel {
			Dock = DockStyle.Fill,
			ColumnCount = 2,
			RowCount = 10,
			Padding = new Padding(10)
		};
		_root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130));
		_root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
		for (var i = 0; i < _root.RowCount; i++)
			_root.RowStyles.Add(new RowStyle(SizeType.Absolute, RowHeight));
		_root.RowStyles[RowSlots] = new RowStyle(SizeType.Percent, 100);
		_root.RowStyles[RowPreview] = new RowStyle(SizeType.Absolute, 54);

		_name = new TextBox { Dock = DockStyle.Fill };
		_enabled = new CheckBox { Dock = DockStyle.Fill, ThreeState = true, Text = "(indeterminate = unset; demo builds only)" };

		_actionType = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
		foreach (var type in Enum.GetValues<ActionType>())
			_actionType.Items.Add(type);
		_actionType.SelectedIndexChanged += (_, _) => OnActionTypeChanged();

		_mathOperation = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
		foreach (var operation in Enum.GetValues<MathOperationType>())
			_mathOperation.Items.Add(operation);
		_mathOperation.SelectedIndexChanged += (_, _) => RefreshPreview();

		_targetObject = new TargetObjectEditor(_vm, _scope) { Dock = DockStyle.Fill };
		_targetParam = new ParamTargetEditor(_vm, () => _targetObject.ResolvedHolder) { Dock = DockStyle.Fill };

		_targetObject.ValueChanged += (_, _) => {
			// The target decides which parameters and which raisable events are on offer,
			// so both dependent lists are rebuilt whenever it moves.
			_targetParam.RefreshComponentParams();
			if (SelectedActionType == ActionType.RaiseEvent) RefreshCallee();
			RefreshPreview();
		};
		_targetParam.ValueChanged += (_, _) => {
			PushExpectedTypeToSource();
			RefreshPreview();
		};

		_calleeLabel = new Label { Text = "Function", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft };
		_callee = new ComboBox {
			Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDown,
			AutoCompleteMode = AutoCompleteMode.SuggestAppend, AutoCompleteSource = AutoCompleteSource.ListItems
		};
		_callee.SelectedIndexChanged += (_, _) => OnCalleeChanged();

		_allEvents = new CheckBox { Text = "all events", AutoSize = true, Dock = DockStyle.Right };
		_allEvents.CheckedChanged += (_, _) => RefreshCallee();

		var calleeRow = new TableLayoutPanel {
			Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, Margin = Padding.Empty, Padding = Padding.Empty
		};
		calleeRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
		calleeRow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90));
		calleeRow.Controls.Add(_callee, 0, 0);
		calleeRow.Controls.Add(_allEvents, 1, 0);

		_slots = new TableLayoutPanel {
			Dock = DockStyle.Fill, ColumnCount = 2, AutoScroll = true, Margin = new Padding(0, 4, 0, 4)
		};
		_slots.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 220));
		_slots.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

		_editExpression = new Button { Text = "Edit expression…", Width = 160, Height = 24 };
		_editExpression.Click += (_, _) => EditExpression();
		_expressionPanel = new Panel { Dock = DockStyle.Fill };
		_expressionPanel.Controls.Add(_editExpression);

		_preview = new TextBox {
			Dock = DockStyle.Fill, ReadOnly = true, Multiline = true, ScrollBars = ScrollBars.Vertical,
			Font = new Font(FontFamily.GenericMonospace, 8.25f)
		};

		AddRow(RowName, "Name", _name);
		AddRow(RowEnabled, "Enabled", _enabled);
		AddRow(RowType, "Action type", _actionType);
		AddRow(RowOperation, "Operation", _mathOperation);
		AddRow(RowTargetObject, "Target object", _targetObject);
		AddRow(RowTargetParam, "Target param", _targetParam);
		AddRow(RowCallee, _calleeLabel, calleeRow);
		AddSpanningRow(RowSlots, _slots);
		AddSpanningRow(RowExpression, _expressionPanel);
		AddSpanningRow(RowPreview, _preview);

		var buttons = new FlowLayoutPanel {
			Dock = DockStyle.Bottom, FlowDirection = FlowDirection.RightToLeft, Height = 42, Padding = new Padding(6)
		};
		var cancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel };
		var save = new Button { Text = "Save" };
		save.Click += (_, _) => Save();
		buttons.Controls.AddRange([cancel, save]);
		AcceptButton = save;
		CancelButton = cancel;

		Controls.Add(_root);
		Controls.Add(buttons);

		LoadAction();
	}

	// ---------------------------------------------------------------- loading

	private void LoadAction() {
		_loading = true;
		try {
			_name.Text = _action.Name ?? "";
			_enabled.CheckState = _action.Enabled switch {
				true => CheckState.Checked,
				false => CheckState.Unchecked,
				null => CheckState.Indeterminate
			};
			_actionType.SelectedItem = _action.ActionType;
			_mathOperation.SelectedItem = _action.MathOperationType;

			_targetObject.Load(_action.TargetObject);
			_targetParam.Load(_action.TargetParam);

			RefreshCallee();
			BuildSlots();
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

	// ---------------------------------------------------------------- callee (function / event)

	/// <summary>
	/// Rebuilds the function/event list and puts the selection back. Clearing a ComboBox
	/// raises SelectedIndexChanged, so the whole thing is done under suppression — otherwise
	/// merely retargeting the action would wipe the slot values through OnCalleeChanged.
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
				foreach (var name in FunctionSignature.AvailableNames)
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
	/// Raising is a call against a specific object, so the offered events are the ones that
	/// object can see. When the target is only known at runtime — a parameter, a message —
	/// there is no object to ask, and the full list is the only honest answer.
	/// </summary>
	private IEnumerable<Event> RaisableEvents() {
		var holder = _targetObject.ResolvedHolder;
		if (_allEvents.Checked || holder == null)
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

		// The stored callee is outside the offered list — an event on an object the target no
		// longer sees, say. Show it rather than silently dropping it on save.
		if (SelectedActionType == ActionType.RaiseEvent &&
			_vm.GetNullableElement<Event>(ulong.TryParse(id, out var eventId) ? eventId : 0) is { } missing) {
			_callee.Items.Insert(0, new CalleeItem(id, $"{missing.Name}   (not visible from target)", missing));
			_callee.SelectedIndex = 0;
		} else {
			_callee.Text = id;
		}
	}

	private CalleeItem? SelectedCallee => _callee.SelectedItem as CalleeItem;

	private void OnCalleeChanged() {
		if (_loading || _suppressCalleeEvents) return;
		BuildSlots();
		RefreshPreview();
	}

	private void OnActionTypeChanged() {
		if (_loading) return;
		RefreshCallee();
		BuildSlots();
		UpdateRowVisibility();
		RefreshPreview();
	}

	// ---------------------------------------------------------------- value slots

	/// <summary>
	/// Rebuilds the value rows for the current action type: one per declared function
	/// parameter, one per message the selected event carries, or the single source of a
	/// SetParam or Math action.
	/// </summary>
	private void BuildSlots() {
		var existing = CurrentSlotTexts();

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
		var name = SelectedCallee?.Id ?? _callee.Text;
		var signature = FunctionSignature.Of(name, _vm, existing);
		if (signature == null) {
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

	private void BuildEventSlots(IReadOnlyList<string> existing) {
		var raisable = SelectedCallee?.Event;
		if (raisable == null) {
			AddNote("Select an event.");
			return;
		}
		if (raisable.Messages.Count == 0 && existing.Count == 0) {
			AddNote($"{raisable.Name} carries no messages.");
			return;
		}

		for (var i = 0; i < raisable.Messages.Count; i++) {
			var message = raisable.Messages[i];
			AddSlot($"{message.ParamName}   [{message.Type}]", SafeTypeInfo(message.Type), ValueAt(existing, i), null);
		}

		// Two actions in the corpus carry more parameters than their event declares messages.
		// EventParams is a free list, so the surplus is shown and kept rather than dropped on
		// save the way a mismatched function argument would have to be.
		for (var i = raisable.Messages.Count; i < existing.Count; i++)
			AddSlot($"(surplus {i + 1})   — not declared by {raisable.Name}", null, ValueAt(existing, i), null);
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
		var row = _slots.RowCount;
		_slots.RowCount = row + 1;
		_slots.RowStyles.Add(new RowStyle(SizeType.Absolute, RowHeight));
		_slots.Controls.Add(new Label { Text = label, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft },
			0, row);
		_slots.Controls.Add(editor, 1, row);
	}

	private void AddNote(string text) {
		var row = _slots.RowCount;
		_slots.RowCount = row + 1;
		_slots.RowStyles.Add(new RowStyle(SizeType.Absolute, RowHeight));
		var note = new Label {
			Text = text, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, ForeColor = SystemColors.GrayText
		};
		_slots.Controls.Add(note, 0, row);
		_slots.SetColumnSpan(note, 2);
	}

	/// <summary>
	/// Slot values as they stand, so rebuilding after a function or event change keeps what
	/// the user already filled in wherever the arity still allows it.
	/// </summary>
	private IReadOnlyList<string> CurrentSlotTexts() {
		if (_slotEditors.Count > 0)
			return _slotEditors.Select(e => e.SerializedValue).ToList();
		return _action.GetParamStrings() ?? [];
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

	private void AddRow(int row, string label, Control control) =>
		AddRow(row, new Label { Text = label, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, control);

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
		SetRowVisible(RowOperation, type == ActionType.Math, RowHeight);
		SetRowVisible(RowTargetParam, type is ActionType.SetParam or ActionType.Math or ActionType.SetExpression,
			RowHeight);
		SetRowVisible(RowCallee, type is ActionType.DoFunction or ActionType.RaiseEvent, RowHeight);
		SetRowVisible(RowExpression, type == ActionType.SetExpression, RowHeight);
		_allEvents.Visible = type == ActionType.RaiseEvent;
	}

	/// <summary>
	/// Collapses a row to zero height rather than merely hiding its controls, so an
	/// inapplicable field leaves no gap behind.
	/// </summary>
	private void SetRowVisible(int row, bool visible, int height) {
		_root.RowStyles[row] = new RowStyle(SizeType.Absolute, visible ? height : 0);
		foreach (Control control in _root.Controls) {
			var position = _root.GetCellPosition(control);
			if (position.Row == row) control.Visible = visible;
		}
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
			$"TargetParam     {_targetParam.SerializedValue}"
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

	// ---------------------------------------------------------------- saving

	private void Save() {
		var type = SelectedActionType;

		if (type == ActionType.DoFunction) {
			var name = SelectedCallee?.Id ?? _callee.Text;
			// The empty name is the placeholder function, which exists only to round-trip a
			// self-closing TargetFuncName, and is not something to save a DoFunction as.
			if (string.IsNullOrEmpty(name) ||
				FunctionSignature.Create(name, _vm, _slotEditors.Select(e => e.SerializedValue).ToList())
					is not { } function) {
				MessageBox.Show(this, $"'{name}' is not a known function.", "Cannot save", MessageBoxButtons.OK,
					MessageBoxIcon.Warning);
				return;
			}
			_action.Function = function;
		}

		if (type == ActionType.RaiseEvent && SelectedCallee?.Event == null) {
			MessageBox.Show(this, "Select an event to raise.", "Cannot save", MessageBoxButtons.OK,
				MessageBoxIcon.Warning);
			return;
		}

		_action.Name = _name.Text;
		_action.Enabled = _enabled.CheckState switch {
			CheckState.Checked => true,
			CheckState.Unchecked => false,
			_ => null
		};
		_action.ActionType = type;
		_action.MathOperationType = type == ActionType.Math
			? (_mathOperation.SelectedItem as MathOperationType?) ?? MathOperationType.None
			: MathOperationType.None;

		_action.TargetObject = _targetObject.Value;
		_action.TargetParam = _targetParam.Value;

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
				_action.Source = null;
				_action.EventToRaise = null;
				_action.EventParams = null;
				DiscardExpression();
				break;
			case ActionType.RaiseEvent:
				_action.Source = null;
				_action.Function = null;
				_action.EventToRaise = SelectedCallee?.Event;
				_action.EventParams = CollectEventParams(SelectedCallee!.Event!);
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
	/// One entry per declared message, plus any surplus the action was carrying — but with
	/// empty surplus entries dropped, so switching to an event with fewer messages does not
	/// leave a tail of blank parameters behind.
	/// </summary>
	private List<ParameterSource> CollectEventParams(Event raisable) {
		var values = _slotEditors.Select(e => e.Value).ToList();
		while (values.Count > raisable.Messages.Count && SafeWrite(values[^1]).Length == 0)
			values.RemoveAt(values.Count - 1);
		return values;
	}

	private static string SafeWrite(ParameterSource source) {
		try {
			return source.Write();
		} catch {
			return "";
		}
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
