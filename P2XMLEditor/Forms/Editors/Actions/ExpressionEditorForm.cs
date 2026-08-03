using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using P2XMLEditor.Core;
using P2XMLEditor.GameData;
using P2XMLEditor.GameData.Enums;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.GameData.VirtualMachineElements.Helper;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes;
using P2XMLEditor.Helper;
using P2XMLEditor.GameData.VirtualMachineElements.Enums;
using ExprKind = P2XMLEditor.GameData.VirtualMachineElements.Enums.ExpressionType;

namespace P2XMLEditor.Forms.Editors.Actions;

/// <summary>
/// Edits one <see cref="Expression"/>, on the same footing as
/// <see cref="ActionEditorForm"/>: the kind on top, a target object below it, and whatever that
/// combination needs after — reusing <see cref="TargetObjectEditor"/>,
/// <see cref="ParamTargetEditor"/> and <see cref="ParameterSourceEditor"/> rather than growing
/// its own half of each.
///
/// The difference from an action is that an expression is an operand: it always produces a
/// value, and something is usually waiting for it to be of a particular type. That expected
/// type — <see cref="ExpressionTyping.ExpectedFor"/> for a condition, the target parameter for
/// a SetExpression action — narrows what is on offer here. When nothing constrains it yet, as
/// on the first side of an empty comparison, everything is offered; that is not a gap but the
/// honest answer, since either side may be filled in first and the other then follows it.
///
/// Void functions are never offered whatever the expected type. An expression exists to be
/// read, and a function with no return value has nothing to give it.
/// </summary>
public sealed class ExpressionEditorForm : Form {
	private const int RowHeight = 34;
	private const int LabelWidth = 150;

	private readonly VirtualMachine _vm;
	private readonly Expression _expression;
	private readonly ActionScope _scope;
	private readonly VmTypeInfo? _expectedType;

	// Set when the expression is one side of a comparison. Then "fits" is not "is the same
	// type" but "can the engine actually compare these", which is neither symmetric nor the
	// same for every operator — see ExpressionComparability.
	private readonly ConditionType? _comparison;
	private readonly bool _firstSide;

	private readonly ComboBox _kind;
	private readonly ComboBox _function;
	private readonly CheckBox _inversion;
	private readonly Label _expects;
	private readonly TextBox _preview;
	private readonly TableLayoutPanel _rows;
	private readonly TableLayoutPanel _slots;
	private readonly TargetObjectEditor _targetObject;
	private readonly ParamTargetEditor _targetParam;
	private readonly ParameterSourceEditor _constant;
	private readonly FormulaEditor _formula;

	private readonly List<ParameterSourceEditor> _slotEditors = [];
	private Panel _targetObjectRow = null!;
	private Panel _targetParamRow = null!;
	private Panel _functionRow = null!;
	private Panel _constantRow = null!;

	private bool _loading = true;

	/// <param name="expectedType">
	/// What the value is going to be used as, or null when nothing constrains it yet.
	/// </param>
	/// <param name="comparison">
	/// The condition this expression is an operand of, when it is one. Supplying it lets the
	/// editor refuse pairs the engine loads but cannot compare, and the ones it compares wrongly.
	/// </param>
	/// <param name="firstSide">Which operand this is; the rules are not symmetric.</param>
	public ExpressionEditorForm(VirtualMachine vm, Expression expression, VmTypeInfo? expectedType = null,
		ConditionType? comparison = null, bool firstSide = true) {
		_vm = vm;
		_expression = expression;
		_expectedType = expectedType;
		_comparison = comparison;
		_firstSide = firstSide;
		_scope = ActionScope.For(expression.LocalContext.Element, null, vm);

		Text = "Edit expression";
		Size = new Size(1120, 620);
		StartPosition = FormStartPosition.CenterParent;
		MinimizeBox = false;
		ShowInTaskbar = false;

		_kind = NewCombo();
		_kind.SelectedIndexChanged += (_, _) => OnKindChanged();

		_targetObject = new TargetObjectEditor(vm, _scope);
		_targetObject.ValueChanged += (_, _) => {
			// The target decides which components — and so which functions — are callable, in
			// the same way it does for an action.
			PopulateFunctions();
			RefreshPreview();
		};

		_targetParam = new ParamTargetEditor(vm, () => new TargetObjectBinding(
			_targetObject.EffectiveHolder, _targetObject.IsConcreteTarget)) { ExpectedType = expectedType };
		_targetParam.ValueChanged += (_, _) => RefreshPreview();

		_function = NewCombo();
		_function.SelectedIndexChanged += (_, _) => {
			RebuildSlots();
			RefreshPreview();
		};

		// A constant is a value like any other slot's, so it is edited by the same control —
		// which also means the expected type filters it exactly as it does everywhere else.
		_constant = new ParameterSourceEditor(vm, _scope, expectedType);
		_constant.ValueChanged += (_, _) => RefreshPreview();

		_formula = new FormulaEditor(vm, expression) { Dock = DockStyle.Fill };
		_formula.ValueChanged += (_, _) => RefreshPreview();

		_inversion = new CheckBox { Text = "Invert result", AutoSize = true, Dock = DockStyle.Left };
		_inversion.CheckedChanged += (_, _) => {
			// Inversion negates the whole formula, so the rendering follows it — through the
			// control's own flag, not the model, which is only written on save.
			_formula.Inverted = _inversion.Checked;
			RefreshPreview();
		};

		// Two lines' worth: with a function selected this carries the expected type and the
		// reason the function list holds what it holds, which does not fit on one.
		_expects = new Label {
			Dock = DockStyle.Top, Height = 44, TextAlign = ContentAlignment.MiddleLeft,
			Padding = new Padding(10, 0, 0, 0), ForeColor = SystemColors.GrayText
		};

		_slots = new TableLayoutPanel {
			Dock = DockStyle.Top, AutoSize = true, ColumnCount = 2, RowCount = 0,
			Padding = new Padding(0, 4, 0, 4)
		};
		_slots.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, LabelWidth));
		_slots.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

		_rows = new TableLayoutPanel {
			Dock = DockStyle.Top, AutoSize = true, ColumnCount = 2, RowCount = 0,
			Padding = new Padding(10, 10, 10, 0)
		};
		_rows.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, LabelWidth));
		_rows.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

		AddRow("Expression is", _kind);
		_targetObjectRow = AddRow("Target object", _targetObject);
		_targetParamRow = AddRow("Parameter", _targetParam);
		_functionRow = AddRow("Function", _function);
		_constantRow = AddRow("Value", _constant);
		AddRow("", _inversion);

		_preview = new TextBox {
			Dock = DockStyle.Bottom, Height = 150, ReadOnly = true, Multiline = true,
			ScrollBars = ScrollBars.Vertical, Font = new Font(FontFamily.GenericMonospace, 9f)
		};

		var buttons = new FlowLayoutPanel {
			Dock = DockStyle.Bottom, FlowDirection = FlowDirection.RightToLeft, Height = 52,
			Padding = new Padding(10, 10, 10, 10)
		};
		var cancel = new Button { Text = "Cancel", Size = new Size(100, 32), DialogResult = DialogResult.Cancel };
		var ok = new Button { Text = "Save", Size = new Size(100, 32), Margin = new Padding(8, 0, 0, 0) };
		ok.Click += (_, _) => Save();
		buttons.Controls.AddRange([cancel, ok]);
		AcceptButton = ok;
		CancelButton = cancel;

		var body = new Panel { Dock = DockStyle.Fill };
		body.Controls.Add(_preview);
		body.Controls.Add(_formula);
		body.Controls.Add(_slots);
		body.Controls.Add(_rows);
		body.Controls.Add(_expects);

		Controls.Add(body);
		Controls.Add(buttons);

		PopulateKinds();
		Load(expression);
		_loading = false;
		UpdateVisibleRows();
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

		_rows.Controls.Add(new Label {
			Text = label, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft
		}, 0, row);
		_rows.Controls.Add(host, 1, row);
		return host;
	}

	// ---------------------------------------------------------------- kinds

	private static readonly ExprKind[] Kinds =
		[ExprKind.Param, ExprKind.Const, ExprKind.Function, ExprKind.Complex];

	private void PopulateKinds() {
		_kind.Items.Clear();
		foreach (var kind in Kinds) _kind.Items.Add(new KindItem(kind));
		_kind.SelectedIndex = 0;
	}

	private ExprKind SelectedKind => _kind.SelectedItem is KindItem item ? item.Kind : ExprKind.Param;

	private void OnKindChanged() {
		UpdateVisibleRows();
		if (SelectedKind == ExprKind.Function) PopulateFunctions();
		RefreshPreview();
	}

	private void UpdateVisibleRows() {
		var kind = SelectedKind;
		// A formula is edited as its children elsewhere; here it only keeps its shape rather
		// than being retyped into something else by accident.
		_targetObjectRow.Visible = kind is ExprKind.Param or ExprKind.Function;
		_targetParamRow.Visible = kind == ExprKind.Param;
		_functionRow.Visible = kind == ExprKind.Function;
		_constantRow.Visible = kind == ExprKind.Const;
		_slots.Visible = kind == ExprKind.Function;
		_formula.Visible = kind == ExprKind.Complex;

		RefreshExpectsText();
	}

	// ---------------------------------------------------------------- functions

	/// <summary>
	/// Functions that may stand here: callable on the target, and returning something this slot
	/// can use. Void is excluded whatever the expected type — see
	/// <see cref="ExpressionTyping.CanBeExpression"/>.
	/// </summary>
	private void PopulateFunctions() {
		var selected = SelectedFunctionName;
		_function.Items.Clear();

		var components = _targetObject.ResolvedComponents;
		var names = components == null
			? FunctionSignature.AvailableNames
			: FunctionSignature.NamesForComponents(components);

		var listed = false;
		foreach (var name in names) {
			var signature = FunctionSignature.Of(name, _vm);
			if (!ExpressionTyping.CanBeExpression(signature)) continue;
			if (!Comparable(signature!.ReturnTypeInfo).IsAllowed) continue;
			_function.Items.Add(new FunctionItem(name, $"{name}   → {Describe(signature!.ReturnTypeInfo)}"));
			listed |= name == selected;
		}

		// What the expression already calls stays selectable even where the filters would drop
		// it, so opening and saving cannot silently retarget it.
		if (!listed && !string.IsNullOrEmpty(selected))
			_function.Items.Insert(0, new FunctionItem(selected!, $"{selected}   (does not fit here)"));

		SelectFunction(selected);
		RefreshExpectsText();
	}

	/// <summary>
	/// The grey line above the form: what this expression has to produce, and — while a function
	/// is being chosen — what narrowed the list to what it holds. The second half matters most
	/// when the list is empty or complete, either of which reads as a broken control until it
	/// says which it is.
	/// </summary>
	private void RefreshExpectsText() {
		// PopulateFunctions can run from a child control's event before the label exists.
		if (_expects == null) return;

		var expects = _expectedType == null
			? "Nothing constrains the type yet — anything may be chosen, and whatever is chosen "
			  + "will constrain the other side."
			: $"Must produce {Describe(_expectedType)}.";

		_expects.Text = SelectedKind != ExprKind.Function
			? expects
			: $"{expects}   {_function.Items.Count} function(s) offered — "
			  + FunctionSignature.DescribeComponentFilter(_targetObject.ResolvedComponents) + ".";
	}

	/// <summary>
	/// Whether a value of this type may stand here. Inside a comparison that is
	/// <see cref="ExpressionComparability"/> with the sides the right way round; anywhere else
	/// it is the ordinary type match.
	/// </summary>
	private ExpressionComparability.Result Comparable(VmTypeInfo? mine) {
		if (_comparison is not { } comparison)
			return VmTypeCompatibility.Matches(_expectedType, mine)
				? ExpressionComparability.Result.Ok
				: ExpressionComparability.Result.No($"does not fit {Describe(_expectedType)}");

		return _firstSide
			? ExpressionComparability.Check(mine, _expectedType, comparison, _vm)
			: ExpressionComparability.Check(_expectedType, mine, comparison, _vm);
	}

	private string? SelectedFunctionName => (_function.SelectedItem as FunctionItem)?.Name;

	private void SelectFunction(string? name) {
		if (string.IsNullOrEmpty(name)) return;
		for (var i = 0; i < _function.Items.Count; i++) {
			if (_function.Items[i] is FunctionItem item && item.Name == name) {
				_function.SelectedIndex = i;
				return;
			}
		}
	}

	private FunctionSignature? CurrentSignature() {
		var name = SelectedFunctionName;
		return string.IsNullOrEmpty(name) ? null : FunctionSignature.Of(name, _vm, ExistingSlotValues());
	}

	private List<string> ExistingSlotValues() => _slotEditors.Select(e => e.SerializedValue).ToList();

	// ---------------------------------------------------------------- slots

	private void RebuildSlots(IReadOnlyList<string>? values = null) {
		var existing = values ?? ExistingSlotValues();

		_slots.SuspendLayout();
		_slots.Controls.Clear();
		_slots.RowStyles.Clear();
		_slots.RowCount = 0;
		_slotEditors.Clear();

		var signature = CurrentSignature();
		if (signature != null)
			foreach (var slot in signature.Slots)
				AddSlot(slot, ValueAt(existing, slot.Index));

		_slots.ResumeLayout();
	}

	private void AddSlot(FunctionSignature.Slot slot, string value) {
		var editor = new ParameterSourceEditor(_vm, _scope, slot.Type) { Dock = DockStyle.Fill };
		if (slot.Constraint != null) editor.Constraint = slot.Constraint;
		editor.ValueChanged += (_, _) => RefreshPreview();

		if (!string.IsNullOrEmpty(value)) {
			try {
				using var fillScope = VirtualMachine.EnterFillScope(_scope.LocalContext);
				editor.Load(ParameterSource.Create(value, _vm, null, slot.Type), value);
			} catch {
				editor.LoadRaw(value);
			}
		}
		_slotEditors.Add(editor);

		var row = _slots.RowCount;
		_slots.RowCount = row + 1;
		_slots.RowStyles.Add(new RowStyle(SizeType.Absolute, RowHeight));
		_slots.Controls.Add(new Label {
			Text = $"{slot.Name}   [{Describe(slot.Type)}]", Dock = DockStyle.Fill,
			TextAlign = ContentAlignment.MiddleLeft
		}, 0, row);
		_slots.Controls.Add(editor, 1, row);
	}

	private static string ValueAt(IReadOnlyList<string> values, int index) =>
		index >= 0 && index < values.Count ? values[index] ?? "" : "";

	// ---------------------------------------------------------------- load / save

	private void Load(Expression expression) {
		SelectKind(expression.ExpressionType);
		UpdateVisibleRows();

		_inversion.Checked = expression.Inversion;
		_formula.Inverted = expression.Inversion;
		_targetObject.Load(expression.TargetObject);

		if (expression.TargetParam is { } target && target.Param is { } param)
			_targetParam.Load(param);

		if (expression.Const is { } constant) {
			var text = SafeSerialize(constant.Value);
			try {
				_constant.Load(ParameterSource.Create(text, _vm, null, ConstantType(constant)), text);
			} catch {
				_constant.LoadRaw(text);
			}
		}

		if (expression.ExpressionType == ExprKind.Complex)
			_formula.Load(expression.FormulaChilds, expression.FormulaOperations);

		if (expression.ExpressionType == ExprKind.Function) {
			PopulateFunctions();
			SelectFunction(expression.Function?.Name);
			RebuildSlots(expression.Function?.GetParamStrings() ?? []);
		}
	}

	private void SelectKind(ExprKind kind) {
		for (var i = 0; i < _kind.Items.Count; i++) {
			if (_kind.Items[i] is KindItem item && item.Kind == kind) {
				_kind.SelectedIndex = i;
				return;
			}
		}
	}

	private string? ValidationError() {
		switch (SelectedKind) {
			case ExprKind.Param:
				if (!_targetObject.Value.IsSet) return "The expression reads a parameter, so it needs a target object.";
				return null;
			case ExprKind.Function:
				if (string.IsNullOrEmpty(SelectedFunctionName)) return "Choose a function.";
				var signature = CurrentSignature();
				if (!ExpressionTyping.CanBeExpression(signature))
					return $"{SelectedFunctionName} returns nothing, so it cannot be an expression.";
				return null;
			case ExprKind.Const:
				return string.IsNullOrEmpty(_constant.SerializedValue) ? "Give the constant a value." : null;
			case ExprKind.Complex:
				return _formula.Validate();
			default:
				return null;
		}
	}

	private void Save() {
		if (ValidationError() is { } error) {
			MessageBox.Show(this, error, "Cannot save", MessageBoxButtons.OK, MessageBoxIcon.Warning);
			return;
		}

		var kind = SelectedKind;
		_expression.ExpressionType = kind;
		_expression.Inversion = _inversion.Checked;

		if (kind is ExprKind.Param or ExprKind.Function)
			_expression.TargetObject = TargetObject.Read(_targetObject.SerializedValue, _vm, _scope.LocalContext);

		_expression.TargetParam = kind == ExprKind.Param
			? ExpressionParamTarget.Read(_targetParam.Value.Write(), _vm, _scope.LocalContext)
			: null;

		_expression.Function = kind == ExprKind.Function
			? FunctionSignature.Create(SelectedFunctionName!, _vm, ExistingSlotValues())
			: null;

		if (kind == ExprKind.Complex) {
			_expression.FormulaChilds = _formula.Children.ToList();
			_expression.FormulaOperations = _formula.Operations.ToList();
		} else if (_expression.FormulaChilds != null) {
			// Retyping away from a formula leaves its terms behind; they belong to this
			// expression and nothing else refers to them.
			foreach (var child in _expression.FormulaChilds) _vm.RemoveElement(child);
			_expression.FormulaChilds = null;
			_expression.FormulaOperations = null;
		}

		if (kind == ExprKind.Const && _expression.Const is { } constant) {
			// The constant is a Parameter holding a typed value, not a string, so what the box
			// says has to survive being read as that type before it is stored. Failing here
			// rather than storing something unreadable keeps a bad literal out of the data.
			ParameterValue? value;
			try {
				value = ParameterValue.Create(_vm, ConstantType(constant), _constant.SerializedValue);
			} catch (Exception ex) {
				MessageBox.Show(this, $"Cannot store the constant: {ex.Message}", "Cannot save",
					MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}
			if (value == null) {
				MessageBox.Show(this, $"'{_constant.SerializedValue}' is not a {Describe(ConstantType(constant))}.",
					"Cannot save", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}
			constant.Value = value;
		}

		DialogResult = DialogResult.OK;
		Close();
	}

	// ---------------------------------------------------------------- preview

	private void RefreshPreview() {
		if (_loading) return;

		var lines = new List<string> {
			$"kind      {SelectedKind}",
			$"expects   {(_expectedType == null ? "(unconstrained)" : Describe(_expectedType))}"
		};

		switch (SelectedKind) {
			case ExprKind.Param:
				lines.Add($"object    {_targetObject.SerializedValue}");
				lines.Add($"param     {SafeWrite(_targetParam)}");
				break;
			case ExprKind.Function:
				lines.Add($"object    {_targetObject.SerializedValue}");
				lines.Add($"function  {SelectedFunctionName}");
				lines.Add($"returns   {Describe(CurrentSignature()?.ReturnTypeInfo)}");
				for (var i = 0; i < _slotEditors.Count; i++)
					lines.Add($"  arg[{i}]  {_slotEditors[i].SerializedValue}");
				break;
			case ExprKind.Const:
				lines.Add($"value     {_constant.SerializedValue}");
				break;
			case ExprKind.Complex:
				lines.Add($"formula   {_formula.Render()}");
				break;
		}

		var verdict = Comparable(ExpressionTyping.TypeOf(_expression, _vm) ?? CurrentSignature()?.ReturnTypeInfo);
		if (verdict.Verdict != ExpressionComparability.Verdict.Fine && verdict.Reason != null)
			lines.Add($"\r\n{(verdict.IsAllowed ? "note" : "!")} {verdict.Reason}");

		if (_inversion.Checked) lines.Add("inverted");
		if (ValidationError() is { } error) lines.Add($"\r\n! {error}");

		_preview.Text = string.Join("\r\n", lines);
	}

	/// <summary>
	/// The constant's own declared type, falling back to what the slot expects. A constant
	/// carries its type on the Parameter that holds it, so an existing one keeps it; a fresh
	/// one takes the type of whatever it is being compared against.
	/// </summary>
	private VmTypeInfo ConstantType(Parameter constant) {
		try {
			if (!string.IsNullOrEmpty(constant.Type))
				return VmTypeHelper.GetVmTypeInfo(constant.Type, _vm);
		} catch {
			// An unreadable declaration is no reason to refuse to show the value.
		}
		return _expectedType ?? VmTypeInfo.Unknown;
	}

	private static string SafeSerialize(ParameterValue? value) {
		try {
			return value?.Serialize() ?? "";
		} catch {
			return "";
		}
	}

	private static string SafeWrite(ParamTargetEditor editor) {
		try {
			return editor.Value.Write();
		} catch {
			return "";
		}
	}

	private static string Describe(VmTypeInfo? type) {
		try {
			return type?.Serialize() ?? "?";
		} catch {
			return "?";
		}
	}

	private sealed class KindItem(ExprKind kind) {
		public ExprKind Kind { get; } = kind;
		public override string ToString() => Kind switch {
			ExprKind.Param => "a parameter's value",
			ExprKind.Const => "a constant",
			ExprKind.Function => "a function's result",
			ExprKind.Complex => "a formula",
			_ => Kind.ToString()
		};
	}

	private sealed class FunctionItem(string name, string label) {
		public string Name { get; } = name;
		public override string ToString() => label;
	}
}
