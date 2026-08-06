using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using P2XMLEditor.Core;
using P2XMLEditor.GameData;
using P2XMLEditor.GameData.Enums;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.GameData.VirtualMachineElements.Enums;
using P2XMLEditor.GameData.VirtualMachineElements.Helper;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes;
using Action = System.Action;
using ExprKind = P2XMLEditor.GameData.VirtualMachineElements.Enums.ExpressionType;

namespace P2XMLEditor.Forms.Editors.Actions;

/// <summary>
/// Edits the terms of a Complex expression — the engine's formula interpreter.
///
/// A formula is two lists read in step: <c>FormulaChilds</c>, each an expression of its own, and
/// <c>FormulaOperations</c>, one operator per child saying how that child folds into the result.
/// There is no syntax tree. <c>ExpressionUtility.CalculateExpressionResult</c> walks the terms
/// left to right carrying two accumulators — one for the current run of multiplications and
/// divisions, one for the additions and subtractions — and adds them together at the end. That
/// is what gives A + B * C its usual meaning without any parentheses being stored.
///
/// So the order of the rows is the formula, and this control shows the grouping that order
/// actually produces rather than leaving the user to work it out. Every term must evaluate to a
/// number: a child that does not makes the whole formula return 0.0 at runtime, which is flagged
/// here rather than discovered in play.
/// </summary>
public sealed class FormulaEditor : UserControl {
	private const int RowHeight = 30;

	/// <summary>
	/// Operators that fold into the multiplicative accumulator, so they bind to the term before
	/// them. Plus and Minus start a new additive block instead.
	///
	/// Multiply and Divide are the two the interpreter is documented to treat this way; RDivide
	/// and Power sit in the same tier by kind rather than by anything observed, and no formula
	/// in either corpus uses them — the two that exist are +/- and none/*.
	/// </summary>
	private static readonly FormulaOperation[] Multiplicative =
		[FormulaOperation.Multiply, FormulaOperation.Divide, FormulaOperation.RDivide, FormulaOperation.Power];

	/// <summary>
	/// Operators applied to the term itself before it folds in. The interpreter runs the function
	/// over the child's value and then treats the result as though it carried None or Plus, so a
	/// unary term always starts a new additive block.
	/// </summary>
	private static readonly FormulaOperation[] Unary =
		[FormulaOperation.Log, FormulaOperation.Log10, FormulaOperation.Exp,
		 FormulaOperation.Sin, FormulaOperation.Cos];

	private readonly VirtualMachine _vm;
	private readonly Expression _owner;
	private readonly TableLayoutPanel _rows;
	private readonly Label _rendered;

	private readonly List<Term> _terms = [];
	private bool _inverted;
	private bool _suppressEvents;

	public event EventHandler? ValueChanged;

	/// <summary>
	/// Whether the expression negates the whole formula. Held here rather than read off the
	/// expression so that toggling the checkbox redraws the rendering without writing to the
	/// model — a cancelled edit must leave the expression as it was.
	/// </summary>
	public bool Inverted {
		get => _inverted;
		set {
			_inverted = value;
			_rendered.Text = Render();
		}
	}

	private sealed class Term(Expression expression, FormulaOperation operation) {
		public Expression Expression { get; } = expression;
		public FormulaOperation Operation { get; set; } = operation;
	}

	public FormulaEditor(VirtualMachine vm, Expression owner) {
		_vm = vm;
		_owner = owner;

		_rows = new TableLayoutPanel {
			Dock = DockStyle.Top, AutoSize = true, ColumnCount = 6, RowCount = 0,
			Padding = new Padding(0, 2, 0, 2)
		};
		_rows.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));
		_rows.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
		_rows.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 70));
		_rows.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 34));
		_rows.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 34));
		_rows.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 34));

		_rendered = new Label {
			Dock = DockStyle.Top, Height = 26, TextAlign = ContentAlignment.MiddleLeft,
			Font = new Font(FontFamily.GenericMonospace, 9f)
		};

		var add = new Button { Text = "Add term", Dock = DockStyle.Left, Width = 110, Height = 26 };
		add.Click += (_, _) => AddTerm();
		var toolbar = new Panel { Dock = DockStyle.Top, Height = 32 };
		toolbar.Controls.Add(add);

		var scroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true };
		scroll.Controls.Add(_rows);

		Controls.Add(scroll);
		Controls.Add(toolbar);
		Controls.Add(_rendered);
	}

	/// <summary>The terms, in the order the interpreter reads them.</summary>
	public IReadOnlyList<Expression> Children => _terms.Select(t => t.Expression).ToList();

	public IReadOnlyList<FormulaOperation> Operations => _terms.Select(t => t.Operation).ToList();

	public void Load(IReadOnlyList<Expression>? children, IReadOnlyList<FormulaOperation>? operations) {
		_suppressEvents = true;
		try {
			_terms.Clear();
			for (var i = 0; i < (children?.Count ?? 0); i++)
				// A formula whose lists have drifted out of step still has to open. The missing
				// operator reads as None, which is what the first term carries anyway.
				_terms.Add(new Term(children![i], OperationAt(operations, i)));
			Rebuild();
		} finally {
			_suppressEvents = false;
		}
	}

	private static FormulaOperation OperationAt(IReadOnlyList<FormulaOperation>? operations, int index) =>
		operations != null && index < operations.Count ? operations[index] : FormulaOperation.None;

	/// <summary>
	/// The formula as the interpreter groups it, with the multiplicative runs bracketed so the
	/// precedence is visible, and the whole thing negated when the expression is inverted.
	/// </summary>
	public string Render() {
		if (_terms.Count == 0) return "(no terms)";

		var blocks = new List<string>();
		var signs = new List<FormulaOperation>();
		var current = new StringBuilder();

		foreach (var term in _terms) {
			var text = Describe(term.Expression);

			if (Unary.Contains(term.Operation)) {
				// Applied to the term, then folded in as though it were a plain addition.
				Flush(blocks, current);
				signs.Add(FormulaOperation.Plus);
				current.Append($"{UnaryName(term.Operation)}({text})");
				continue;
			}

			if (Multiplicative.Contains(term.Operation) && current.Length > 0) {
				current.Append($" {Symbol(term.Operation)} {text}");
				continue;
			}

			Flush(blocks, current);
			signs.Add(term.Operation == FormulaOperation.Minus ? FormulaOperation.Minus : FormulaOperation.Plus);
			current.Append(text);
		}
		Flush(blocks, current);

		var formula = new StringBuilder();
		for (var i = 0; i < blocks.Count; i++) {
			var block = blocks[i].Contains(' ') && blocks.Count > 1 ? $"({blocks[i]})" : blocks[i];
			if (i == 0) formula.Append(signs[i] == FormulaOperation.Minus ? "-" + block : block);
			else formula.Append(signs[i] == FormulaOperation.Minus ? $" - {block}" : $" + {block}");
		}

		return Inverted ? $"-({formula})" : formula.ToString();
	}

	private static void Flush(List<string> blocks, StringBuilder current) {
		if (current.Length == 0) return;
		blocks.Add(current.ToString());
		current.Clear();
	}

	/// <summary>
	/// What stops this formula evaluating, or null when nothing does. Every term has to be a
	/// number — the interpreter gives up and returns 0.0 on the first one that is not.
	/// </summary>
	public string? Validate() {
		if (_terms.Count == 0) return "A formula needs at least one term.";

		foreach (var term in _terms) {
			var type = ExpressionTyping.TypeOf(term.Expression, _vm);
			// Unknown is not an error: a term whose type cannot be read yet is unfinished, not
			// wrong, and saying otherwise would block editing it.
			if (type == null || type.BaseType == VmType.Unknown) continue;
			if (!VmTypeCompatibility.Accepts(VmTypeInfo.Int32, type))
				return $"{Describe(term.Expression)} is {type.Serialize()}, and a formula term has "
					   + "to be a number — the whole formula would evaluate to 0.";
		}
		return null;
	}

	// ---------------------------------------------------------------- rows

	private void Rebuild() {
		_rows.SuspendLayout();
		_rows.Controls.Clear();
		_rows.RowStyles.Clear();
		_rows.RowCount = 0;

		for (var i = 0; i < _terms.Count; i++) AddRow(i);

		_rows.ResumeLayout();
		_rendered.Text = Render();
	}

	private void AddRow(int index) {
		var term = _terms[index];
		var row = _rows.RowCount;
		_rows.RowCount = row + 1;
		_rows.RowStyles.Add(new RowStyle(SizeType.Absolute, RowHeight));

		var operation = new ComboBox {
			Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList, IntegralHeight = false,
			Margin = new Padding(0, 2, 4, 2)
		};
		foreach (var value in Enum.GetValues<FormulaOperation>())
			operation.Items.Add(new OperationItem(value));
		SelectOperation(operation, term.Operation);
		operation.SelectedIndexChanged += (_, _) => {
			if (operation.SelectedItem is not OperationItem item) return;
			term.Operation = item.Operation;
			_rendered.Text = Render();
			OnChanged();
		};

		var description = new TextBox {
			Dock = DockStyle.Fill, ReadOnly = true, Margin = new Padding(0, 2, 4, 2),
			Text = DescribeDetailed(term.Expression)
		};

		var edit = NewButton("Edit…", () => EditTerm(term, description));
		var up = NewButton("↑", () => Move(index, -1));
		var down = NewButton("↓", () => Move(index, +1));
		var remove = NewButton("✕", () => Remove(index));

		_rows.Controls.Add(operation, 0, row);
		_rows.Controls.Add(description, 1, row);
		_rows.Controls.Add(edit, 2, row);
		_rows.Controls.Add(up, 3, row);
		_rows.Controls.Add(down, 4, row);
		_rows.Controls.Add(remove, 5, row);
	}

	private static Button NewButton(string text, Action onClick) {
		var button = new Button { Text = text, Dock = DockStyle.Fill, Margin = new Padding(0, 2, 2, 2) };
		button.Click += (_, _) => onClick();
		return button;
	}

	private void AddTerm() {
		var child = VmElement.CreateDefault<Expression>(_vm, _owner);
		// The first term is what the accumulators start from, so it carries no operator; every
		// later one defaults to addition.
		_terms.Add(new Term(child, _terms.Count == 0 ? FormulaOperation.None : FormulaOperation.Plus));
		Rebuild();
		OnChanged();
	}

	private void EditTerm(Term term, TextBox description) {
		// Every term has to be a number, so that is what its own editor is told to expect.
		// Accepts widens across the numeric types, so an Int32 parameter is still on offer.
		using var editor = new ExpressionEditorForm(_vm, term.Expression, VmTypeInfo.Int32);
		if (editor.ShowDialog(FindForm()) != DialogResult.OK) return;

		description.Text = DescribeDetailed(term.Expression);
		_rendered.Text = Render();
		OnChanged();
	}

	private void Move(int index, int delta) {
		var target = index + delta;
		if (target < 0 || target >= _terms.Count) return;
		(_terms[index], _terms[target]) = (_terms[target], _terms[index]);
		Rebuild();
		OnChanged();
	}

	private void Remove(int index) {
		if (index < 0 || index >= _terms.Count) return;
		var term = _terms[index];
		_terms.RemoveAt(index);
		// The term is this formula's own expression and nothing else refers to it, so dropping
		// the row drops the element too rather than leaving it orphaned in the machine.
		_vm.RemoveElement(term.Expression);
		Rebuild();
		OnChanged();
	}

	private void OnChanged() {
		if (_suppressEvents) return;
		ValueChanged?.Invoke(this, EventArgs.Empty);
	}

	// ---------------------------------------------------------------- naming

	/// <summary>A term in one word, for the rendered formula.</summary>
	private string Describe(Expression expression) => expression.ExpressionType switch {
		ExprKind.Const => SafeSerialize(expression.Const?.Value) is { Length: > 0 } value ? value : "const",
		ExprKind.Param => expression.TargetParam?.Param?.Parameter?.Element is Parameter parameter
			? parameter.Name
			: "param",
		ExprKind.Function => expression.Function?.Name ?? "function",
		ExprKind.Complex => "(…)",
		_ => "?"
	};

	/// <summary>The same, with its type, for the row itself.</summary>
	private string DescribeDetailed(Expression expression) {
		var type = ExpressionTyping.TypeOf(expression, _vm);
		var typeName = type == null ? "?" : SafeTypeName(type);
		return $"{Describe(expression)}   [{typeName}]   {expression.ExpressionType}";
	}

	private static string SafeSerialize(ParameterValue? value) {
		try {
			return value?.Serialize() ?? "";
		} catch {
			return "";
		}
	}

	private static string SafeTypeName(VmTypeInfo type) {
		try {
			return type.Serialize();
		} catch {
			return type.BaseType.ToString();
		}
	}

	private static void SelectOperation(ComboBox box, FormulaOperation operation) {
		for (var i = 0; i < box.Items.Count; i++) {
			if (box.Items[i] is OperationItem item && item.Operation == operation) {
				box.SelectedIndex = i;
				return;
			}
		}
	}

	private static string Symbol(FormulaOperation operation) => operation switch {
		FormulaOperation.Plus => "+",
		FormulaOperation.Minus => "-",
		FormulaOperation.Multiply => "*",
		FormulaOperation.Divide => "/",
		FormulaOperation.RDivide => "\\",
		FormulaOperation.Power => "^",
		_ => operation.ToString()
	};

	private static string UnaryName(FormulaOperation operation) => operation switch {
		FormulaOperation.Log => "log",
		FormulaOperation.Log10 => "log10",
		FormulaOperation.Exp => "exp",
		FormulaOperation.Sin => "sin",
		FormulaOperation.Cos => "cos",
		_ => operation.ToString().ToLowerInvariant()
	};

	private sealed class OperationItem(FormulaOperation operation) {
		public FormulaOperation Operation { get; } = operation;
		public override string ToString() => Operation switch {
			FormulaOperation.None => "(first term)",
			FormulaOperation.Plus => "+   add",
			FormulaOperation.Minus => "−   subtract",
			FormulaOperation.Multiply => "×   multiply",
			FormulaOperation.Divide => "÷   divide",
			FormulaOperation.RDivide => "\\   divide into",
			FormulaOperation.Power => "^   power",
			FormulaOperation.Log => "log( )",
			FormulaOperation.Log10 => "log10( )",
			FormulaOperation.Exp => "exp( )",
			FormulaOperation.Sin => "sin( )",
			FormulaOperation.Cos => "cos( )",
			_ => Operation.ToString()
		};
	}
}
