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
using P2XMLEditor.GameData.VirtualMachineElements.Enums;
using P2XMLEditor.GameData.VirtualMachineElements.Helper;
using P2XMLEditor.GameData.VirtualMachineElements.Interfaces;
using P2XMLEditor.Helper;
using P2XMLEditor.WindowsFormsExtensions;
using ExprKind = P2XMLEditor.GameData.VirtualMachineElements.Enums.ExpressionType;
using VmAction = P2XMLEditor.GameData.VirtualMachineElements.Action;

namespace P2XMLEditor.Forms.MainForm.Expressions;

/// <summary>
/// Browses every <see cref="Expression"/> in the machine, the way
/// <see cref="Forms.MainForm.Actions.ActionsBrowser"/> browses actions: grouped under whatever
/// owns them, searchable, and opening the editor on a double-click.
///
/// The grouping is not decoration. An expression on its own says nothing about what it has to
/// produce — that comes entirely from where it sits — so the owner is what makes a row
/// meaningful, and it is also what the editor needs in order to filter anything. A condition
/// operand is typed by the operand opposite and by the comparison between them; an action's
/// source expression by the parameter it is written into; a formula term by the formula, which
/// is always a number. Each row carries its own, so editing from here constrains exactly as
/// editing from the owning form does.
/// </summary>
public class ExpressionsBrowser : Panel {
	private readonly VirtualMachine _vm;
	private TreeView _treeView = null!;
	private SearchControl _searchControl = null!;
	private ContextMenuStrip _contextMenu = null!;

	/// <summary>Where one expression sits, and therefore what it has to produce.</summary>
	private sealed record Slot(
		Expression Expression,
		string Role,
		VmTypeInfo? Expected,
		ConditionType? Comparison,
		bool FirstSide);

	public ExpressionsBrowser(VirtualMachine vm) {
		_vm = vm;
		Dock = DockStyle.Fill;

		SetupControls();
		LoadExpressions();
	}

	private void SetupControls() {
		_searchControl = new SearchControl { Dock = DockStyle.Top };
		_searchControl.SearchChanged += (_, _) => LoadExpressions();

		_treeView = new TreeView {
			Dock = DockStyle.Fill, FullRowSelect = true, HideSelection = false,
			ShowLines = true, ShowPlusMinus = true, ShowRootLines = true
		};

		_treeView.NodeMouseDoubleClick += (_, e) => EditSelected(e.Node);
		_treeView.KeyDown += (_, e) => {
			if (e.KeyCode == Keys.Enter) EditSelected(_treeView.SelectedNode);
		};

		SetupContextMenu();
		_treeView.ContextMenuStrip = _contextMenu;

		Controls.Add(_treeView);
		Controls.Add(_searchControl);
	}

	private void SetupContextMenu() {
		_contextMenu = new ContextMenuStrip();

		var editItem = new ToolStripMenuItem("Edit");
		editItem.Click += (_, _) => EditSelected(_treeView.SelectedNode);

		var expandAllItem = new ToolStripMenuItem("Expand All");
		expandAllItem.Click += (_, _) => _treeView.ExpandAll();

		var collapseAllItem = new ToolStripMenuItem("Collapse All");
		collapseAllItem.Click += (_, _) => _treeView.CollapseAll();

		_contextMenu.Items.AddRange([editItem, new ToolStripSeparator(), expandAllItem, collapseAllItem]);
	}

	// ---------------------------------------------------------------- loading

	private void LoadExpressions() {
		_treeView.BeginUpdate();
		_treeView.Nodes.Clear();

		var owners = 0;
		var shown = 0;
		var total = 0;

		foreach (var (owner, slots) in Owners()) {
			total += slots.Count;

			var ownerText = DescribeOwner(owner);
			var matching = slots
				.Where(slot => _searchControl.IsMatchAny(ownerText, Describe(slot.Expression),
					slot.Expression.Id.ToString(), owner.Id.ToString()))
				.ToList();
			if (matching.Count == 0) continue;

			var node = new TreeNode(ownerText) {
				Tag = owner,
				ToolTipText = $"ID: {owner.Id}\nContext: {ContextName(owner)}"
			};
			foreach (var slot in matching) node.Nodes.Add(CreateSlotNode(slot));

			_treeView.Nodes.Add(node);
			owners++;
			shown += matching.Count;
		}

		_treeView.EndUpdate();
		_searchControl.StatusText = $"Displaying {shown}/{total} expressions in {owners} owners.";
	}

	/// <summary>
	/// Everything that holds an expression, with the slots it holds them in. A formula's terms
	/// are not listed here — they hang off their own expression's node, where the formula they
	/// belong to is visible.
	/// </summary>
	private IEnumerable<(VmElement Owner, List<Slot> Slots)> Owners() {
		foreach (var condition in _vm.GetElementsByType<PartCondition>().OrderBy(c => c.Id)) {
			var slots = new List<Slot>();
			if (condition.FirstExpression is { } first)
				slots.Add(new Slot(first, "first", ExpectedFor(condition, true),
					condition.ConditionType, true));
			if (condition.SecondExpression is { } second)
				slots.Add(new Slot(second, "second", ExpectedFor(condition, false),
					condition.ConditionType, false));
			if (slots.Count > 0) yield return (condition, slots);
		}

		foreach (var action in _vm.GetElementsByType<VmAction>().OrderBy(a => a.Id)) {
			if (action.SourceExpression is not { } expression) continue;
			yield return (action, [new Slot(expression, "source", TargetParamType(action), null, true)]);
		}
	}

	private VmTypeInfo? ExpectedFor(PartCondition condition, bool firstSide) {
		try {
			return ExpressionTyping.ExpectedFor(condition, firstSide, _vm);
		} catch {
			return null;
		}
	}

	/// <summary>The declared type of the parameter a SetExpression action writes into.</summary>
	private VmTypeInfo? TargetParamType(VmAction action) {
		try {
			var parameter = action.TargetParam.Parameter?.Element as Parameter;
			return parameter == null || string.IsNullOrEmpty(parameter.Type)
				? null
				: VmTypeHelper.GetVmTypeInfo(parameter.Type, _vm);
		} catch {
			return null;
		}
	}

	private TreeNode CreateSlotNode(Slot slot) {
		var node = new TreeNode($"{slot.Role}:  {Describe(slot.Expression)}") {
			Tag = slot,
			ToolTipText = $"ID: {slot.Expression.Id}\nKind: {slot.Expression.ExpressionType.Serialize()}\n"
						  + $"Expects: {DescribeType(slot.Expected)}",
			ForeColor = Color.DarkBlue
		};
		AddFormulaTerms(node, slot.Expression);
		return node;
	}

	/// <summary>
	/// A formula's terms, nested. Each is an expression in its own right and is edited as one;
	/// the engine requires the whole formula to evaluate to a number, so each term expects one.
	/// </summary>
	private void AddFormulaTerms(TreeNode node, Expression expression, int depth = 0) {
		if (expression.ExpressionType != ExprKind.Complex || depth > 8) return;

		var children = expression.FormulaChilds ?? [];
		var operations = expression.FormulaOperations ?? [];
		for (var i = 0; i < children.Count; i++) {
			var operation = i < operations.Count ? operations[i] : FormulaOperation.None;
			var term = new TreeNode($"{OperationSymbol(operation)}  {Describe(children[i])}") {
				Tag = new Slot(children[i], "term", VmTypeInfo.Int32, null, true),
				ToolTipText = $"ID: {children[i].Id}\nOperation: {operation}",
				ForeColor = Color.DarkSlateGray
			};
			AddFormulaTerms(term, children[i], depth + 1);
			node.Nodes.Add(term);
		}
	}

	private static string OperationSymbol(FormulaOperation operation) => operation switch {
		FormulaOperation.None => " ",
		FormulaOperation.Plus => "+",
		FormulaOperation.Minus => "-",
		FormulaOperation.Multiply => "*",
		FormulaOperation.Divide => "/",
		_ => operation.ToString()
	};

	// ---------------------------------------------------------------- text

	/// <summary>
	/// The expression as one line. PreviewHelper is the shared renderer and stays that way, but
	/// it assumes the expression is complete — an operand that has not been filled in yet is null
	/// where it dereferences — so a browser over every expression in the machine cannot call it
	/// unguarded.
	/// </summary>
	private static string Describe(Expression expression) {
		try {
			return PreviewHelper.Preview(expression);
		} catch {
			return $"<{expression.ExpressionType.Serialize()} {expression.Id}>";
		}
	}

	private static string DescribeOwner(VmElement owner) {
		try {
			return owner switch {
				PartCondition condition => $"[Condition] {PreviewHelper.Preview(condition)}",
				VmAction action => $"[Action] {action.Name}",
				_ => owner.ToString() ?? ""
			};
		} catch {
			return owner switch {
				PartCondition condition => $"[Condition] {condition.ConditionType.Serialize()} {condition.Id}",
				VmAction action => $"[Action] {action.Id}",
				_ => owner.Id.ToString()
			};
		}
	}

	private static string DescribeType(VmTypeInfo? type) {
		try {
			return type?.Serialize() ?? "(unconstrained)";
		} catch {
			return "(unconstrained)";
		}
	}

	/// <summary>Where the owner lives, so a condition can be told from an identical one elsewhere.</summary>
	private string ContextName(VmElement owner) {
		var context = owner switch {
			VmAction action => action.LocalContext.Element,
			PartCondition condition => LocalContextOf(condition),
			_ => null
		};
		return context switch {
			State s => $"State: {s.Name}",
			Graph g => $"Graph: {g.Name}",
			Branch b => $"Branch: {b.Name}",
			Talking t => $"Talking: {t.Name}",
			Speech sp => $"Speech: {sp.Name}",
			INamedElement named => $"{context!.GetType().Name}: {named.Name}",
			_ => "Unknown context"
		};
	}

	/// <summary>
	/// A PartCondition carries no context of its own; its operands do, and they all share one.
	/// </summary>
	private static VmElement? LocalContextOf(PartCondition condition) =>
		(condition.FirstExpression ?? condition.SecondExpression)?.LocalContext.Element;

	// ---------------------------------------------------------------- editing

	private void EditSelected(TreeNode? node) {
		if (node?.Tag is not Slot slot) return;

		using var editor = new ExpressionEditorForm(_vm, slot.Expression, slot.Expected,
			slot.Comparison, slot.FirstSide);
		if (editor.ShowDialog(FindForm()) != DialogResult.OK) return;

		// Only what was edited changed, so the node is refreshed in place rather than rebuilding
		// the tree and losing the user's expansion state. Its terms can have changed wholesale,
		// which is why they are rebuilt rather than patched.
		node.Text = node.Parent?.Tag is Slot
			? $"{OperationSymbol(FormulaOperationOf(node))}  {Describe(slot.Expression)}"
			: $"{slot.Role}:  {Describe(slot.Expression)}";
		node.Nodes.Clear();
		AddFormulaTerms(node, slot.Expression);

		if (node.Parent is { Tag: PartCondition or VmAction } owner)
			owner.Text = DescribeOwner((VmElement)owner.Tag);
	}

	/// <summary>The operation a term node was labelled with, recovered from its own text.</summary>
	private static FormulaOperation FormulaOperationOf(TreeNode node) {
		var parent = node.Parent;
		if (parent?.Tag is not Slot parentSlot) return FormulaOperation.None;
		var index = parent.Nodes.IndexOf(node);
		var operations = parentSlot.Expression.FormulaOperations;
		return operations != null && index >= 0 && index < operations.Count
			? operations[index]
			: FormulaOperation.None;
	}
}
