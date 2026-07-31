using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using P2XMLEditor.Core;
using P2XMLEditor.Forms.Editors.Actions;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.GameData.VirtualMachineElements.Enums;
using P2XMLEditor.GameData.VirtualMachineElements.Interfaces;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes;
using P2XMLEditor.Helper;
using P2XMLEditor.Logging;
using P2XMLEditor.WindowsFormsExtensions;
using Action = P2XMLEditor.GameData.VirtualMachineElements.Action;

namespace P2XMLEditor.Forms.MainForm.Actions;

public class ActionsBrowser : Panel {
	private readonly VirtualMachine _vm;
	private TreeView _treeView;
	private SearchControl _searchControl;
	private ContextMenuStrip _contextMenu;

	[PerformanceLogHook]
	public ActionsBrowser(VirtualMachine vm) {
		_vm = vm;
		Dock = DockStyle.Fill;

		SetupControls();
		LoadActionLines();
	}

	private void SetupControls() {
		_searchControl = new SearchControl { Dock = DockStyle.Top };
		_searchControl.SearchChanged += (_, _) => LoadActionLines();

		_treeView = new TreeView {
			Dock = DockStyle.Fill,
			FullRowSelect = true,
			HideSelection = false,
			ShowLines = true,
			ShowPlusMinus = true,
			ShowRootLines = true
		};

		_treeView.NodeMouseDoubleClick += OnNodeDoubleClick;
		_treeView.KeyDown += (_, e) => {
			if (e.KeyCode == Keys.Delete) DeleteSelectedActionLine();
			if (e.KeyCode == Keys.Enter) EditSelected();
		};

		SetupContextMenu();
		_treeView.ContextMenuStrip = _contextMenu;

		Controls.Add(_treeView);
		Controls.Add(_searchControl);
	}

	private void SetupContextMenu() {
		_contextMenu = new ContextMenuStrip();

		var editItem = new ToolStripMenuItem("Edit");
		editItem.Click += (_, _) => EditSelected();

		var expandAllItem = new ToolStripMenuItem("Expand All");
		expandAllItem.Click += (_, _) => _treeView.ExpandAll();

		var collapseAllItem = new ToolStripMenuItem("Collapse All");
		collapseAllItem.Click += (_, _) => _treeView.CollapseAll();

		var deleteItem = new ToolStripMenuItem("Delete");
		deleteItem.Click += (_, _) => DeleteSelectedActionLine();

		_contextMenu.Items.AddRange([editItem, new ToolStripSeparator(), expandAllItem, collapseAllItem,
			new ToolStripSeparator(), deleteItem]);
	}

	private void LoadActionLines() {
		_treeView.BeginUpdate();
		_treeView.Nodes.Clear();

		var actionLines = _vm.GetElementsByType<ActionLine>()
			.OrderBy(GetContextName, StringComparer.Ordinal)
			.ThenBy(al => al.Name, StringComparer.Ordinal)
			.ToList();

		var displayedCount = 0;

		foreach (var actionLine in actionLines) {
			var contextName = GetContextName(actionLine);
			var nodeName = $"{actionLine.Name} [{actionLine.ActionLineType.Serialize()}]";

			if (!_searchControl.IsMatchAny(nodeName, contextName, actionLine.Id.ToString()))
				continue;

			var node = new TreeNode(nodeName) {
				Tag = actionLine,
				ToolTipText = $"ID: {actionLine.Id}\nContext: {contextName}"
			};

			AddChildren(node, actionLine);

			_treeView.Nodes.Add(node);
			displayedCount++;
		}

		_treeView.EndUpdate();
		_searchControl.StatusText = $"Displaying {displayedCount}/{actionLines.Count} action lines.";
	}

	private void AddChildren(TreeNode node, ActionLine actionLine) {
		foreach (var child in actionLine.Actions ?? []) {
			var childNode = child.Element switch {
				Action a => CreateActionNode(a),
				ActionLine al => CreateActionLineNode(al),
				_ => null
			};
			if (childNode != null) node.Nodes.Add(childNode);
		}
	}

	private TreeNode CreateActionNode(Action action) {
		var targetObject = Describe(action.TargetObject);
		var targetParam = Describe(action.TargetParam);
		var sourceParams = string.Join(", ", action.GetParamStrings() ?? []);

		var actionText = action.ActionType switch {
			ActionType.SetParam => $"{targetObject}.{targetParam} = {sourceParams}",
			ActionType.SetExpression =>
				$"{targetObject}.{targetParam} = {PreviewHelper.Preview(action.SourceExpression)}",
			ActionType.Math => $"{targetObject}.{targetParam} {MathSymbol(action.MathOperationType)}= {sourceParams}",
			ActionType.DoFunction => $"{targetObject}.{action.Function?.Name ?? action.TargetFuncName}({sourceParams})",
			ActionType.RaiseEvent => $"{targetObject} ⇒ {action.EventToRaise?.Name ?? action.TargetFuncName}({sourceParams})",
			_ => $"{action.ActionType.Serialize()} {targetObject}"
		};

		if (!string.IsNullOrEmpty(action.Name)) actionText = $"{action.Name}:  {actionText}";

		return new TreeNode(actionText) {
			Tag = action,
			ToolTipText = $"ID: {action.Id}\nType: {action.ActionType.Serialize()}\nOrder: {action.OrderIndex}",
			ForeColor = Color.DarkBlue
		};
	}

	private static string MathSymbol(MathOperationType operation) => operation switch {
		MathOperationType.Addition => "+",
		MathOperationType.Subtraction => "-",
		MathOperationType.Multiply => "*",
		MathOperationType.Division => "/",
		_ => "?"
	};

	/// <summary>Resolves the ids in a target to names, falling back to the raw text.</summary>
	private string Describe(TargetObject target) {
		try {
			return target.Kind switch {
				TargetObjectKind.Holder => target.Holder?.Name ?? target.Write(),
				TargetObjectKind.ParameterRef => target.ParameterRef?.Name ?? target.Write(),
				TargetObjectKind.Hierarchy => string.Join("/",
					target.Hierarchy!.Elements.Select(e => (e.Element as INamedElement)?.Name ?? e.Id.ToString())),
				TargetObjectKind.Message => target.Message?.ParamName ?? target.Write(),
				TargetObjectKind.InputParam => target.InputParam?.ParamName ?? target.Write(),
				TargetObjectKind.Loop => target.Loop?.ParamId ?? target.Write(),
				_ => "?"
			};
		} catch {
			return "?";
		}
	}

	private string Describe(ParamTarget target) => target.Kind switch {
		ParamTargetKind.Empty => "",
		ParamTargetKind.Parameter => (target.Parameter?.Element as Parameter)?.Name ?? target.Parameter?.Id.ToString() ?? "",
		ParamTargetKind.ComponentParam => target.ComponentParamName ?? "",
		_ => ""
	};

	private TreeNode CreateActionLineNode(ActionLine actionLine) {
		var node = new TreeNode($"[ActionLine] {actionLine.Name} [{actionLine.ActionLineType.Serialize()}]") {
			Tag = actionLine,
			ToolTipText = $"ID: {actionLine.Id}\nType: {actionLine.ActionLineType.Serialize()}",
			ForeColor = Color.DarkGreen
		};

		AddChildren(node, actionLine);
		return node;
	}

	private static string GetContextName(ActionLine actionLine) => actionLine.LocalContext.Element switch {
		State s => $"State: {s.Name}",
		Graph g => $"Graph: {g.Name}",
		Branch b => $"Branch: {b.Name}",
		Talking t => $"Talking: {t.Name}",
		Speech sp => $"Speech: {sp.Name}",
		_ => "Unknown Context"
	};

	private void OnNodeDoubleClick(object? sender, TreeNodeMouseClickEventArgs e) {
		if (e.Node?.Tag is Action) EditSelected(e.Node);
	}

	private void EditSelected() => EditSelected(_treeView.SelectedNode);

	private void EditSelected(TreeNode? node) {
		if (node?.Tag is not Action action) return;

		using var editor = new ActionEditorForm(_vm, action);
		if (editor.ShowDialog(FindForm()) != DialogResult.OK) return;

		// Only the edited node changed, so it is refreshed in place rather than rebuilding
		// the whole tree and losing the user's expansion state.
		var refreshed = CreateActionNode(action);
		node.Text = refreshed.Text;
		node.ToolTipText = refreshed.ToolTipText;
	}

	private void DeleteSelectedActionLine() {
		if (_treeView.SelectedNode?.Tag is not ActionLine actionLine)
			return;

		var result = MessageBox.Show(
			$"Are you sure you want to delete the action line '{actionLine.Name}'?\n\nThis will also delete all child actions.",
			"Confirm Delete",
			MessageBoxButtons.YesNo,
			MessageBoxIcon.Warning
		);

		if (result != DialogResult.Yes)
			return;

		_vm.RemoveElement(actionLine);
		LoadActionLines();
	}
}
