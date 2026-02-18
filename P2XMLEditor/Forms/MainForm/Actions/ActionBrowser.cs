using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.GameData.VirtualMachineElements.Enums;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes;
using P2XMLEditor.GameData.VirtualMachineElements.Placeholders;
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
            Location = new Point(10, 45),
            Size = new Size(Width - 20, Height - 55),
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
            FullRowSelect = true,
            HideSelection = false,
            ShowLines = true,
            ShowPlusMinus = true,
            ShowRootLines = true
        };

        _treeView.NodeMouseDoubleClick += OnNodeDoubleClick;
        _treeView.KeyDown += (s, e) => {
            if (e.KeyCode == Keys.Delete) DeleteSelectedActionLine();
        };

        SetupContextMenu();
        _treeView.ContextMenuStrip = _contextMenu;

        Controls.AddRange([_searchControl, _treeView]);
    }

    private void SetupContextMenu() {
        _contextMenu = new ContextMenuStrip();
        
        var expandAllItem = new ToolStripMenuItem("Expand All");
        expandAllItem.Click += (_, _) => _treeView.ExpandAll();
        
        var collapseAllItem = new ToolStripMenuItem("Collapse All");
        collapseAllItem.Click += (_, _) => _treeView.CollapseAll();
        
        var deleteItem = new ToolStripMenuItem("Delete");
        deleteItem.Click += (_, _) => DeleteSelectedActionLine();

        _contextMenu.Items.AddRange([expandAllItem, collapseAllItem, new ToolStripSeparator(), deleteItem]);
    }

    private void LoadActionLines() {
        _treeView.BeginUpdate();
        _treeView.Nodes.Clear();

        var actionLines = _vm.GetElementsByType<ActionLine>()
            .OrderBy(al => al.LocalContext.Element switch {
                State s => s.Name,
                Graph g => g.Name,
                Branch b => b.Name,
                Talking t => t.Name,
                Speech sp => sp.Name,
                _ => "Unknown"
            })
            .ThenBy(al => al.Name)
            .ToList();

        var displayedCount = 0;

        foreach (var actionLine in actionLines) {
            var contextName = GetContextName(actionLine);
            var nodeName = $"{actionLine.Name} [{actionLine.ActionLineType.Serialize()}]";
            var searchText = $"{nodeName} {contextName}";

            if (!_searchControl.IsMatchAny(searchText, actionLine.Id.ToString()))
                continue;

            var node = new TreeNode(nodeName) {
                Tag = actionLine,
                ToolTipText = $"ID: {actionLine.Id}\nContext: {contextName}"
            };

            // Add child actions/actionlines
            if (actionLine.Actions != null && actionLine.Actions.Any()) {
                foreach (var action in actionLine.Actions) {
                    var childNode = action.Element switch {
                        Action a => CreateActionNode(a),
                        ActionLine al => CreateActionLineNode(al),
                        _ => null
                    };

                    if (childNode != null)
                        node.Nodes.Add(childNode);
                }
            }

            _treeView.Nodes.Add(node);
            displayedCount++;
        }

        _treeView.ExpandAll();
        _treeView.EndUpdate();
        _searchControl.StatusText = $"Displaying {displayedCount}/{actionLines.Count} action lines.";
    }

    private TreeNode CreateActionNode(Action action) {
        var actionText = $"[Action] {action.Name} ";
        
        var targetObject = action.TargetObject;
        if (ulong.TryParse(targetObject, out var targetObjectId)) {
            var targetObjectElement = _vm.GetNullableElement<VmElement>(targetObjectId);
            var name = targetObjectElement switch {
                ParameterHolder ph => ph.Name,
                Parameter p => p.Name,
                _ => ""
            };
            if (targetObjectElement != null)
                targetObject = name;
        } else if (HierarchyGuid.TryParse(targetObject, _vm, out var targetObjectHierarchyGuid)) {
            targetObject = "";
            foreach (VmEither<Scene, Geom, Other, Item, ScenePlaceholder> element in targetObjectHierarchyGuid.Elements) {
                var name = element.Element switch {
                    ParameterHolder ph => ph.Name,
                    ScenePlaceholder sp => sp.Id.ToString(),
                    _ => ""
                };
                targetObject += $"{name}H";
            }
        }
        
        var targetParam = action.TargetParam.Replace("%", "");
        if (ulong.TryParse(targetParam, out var targetParamId)) {
            var targetParamElement = _vm.GetNullableElement<Parameter>(targetParamId);
            if (targetParamElement != null) 
                targetParam = targetParamElement.Name;
        }

        var targetFuncName = action.TargetFuncName;
        if (ulong.TryParse(targetFuncName, out var targetFuncNameId)) {
            var targetEvent = _vm.GetNullableElement<Event>(targetFuncNameId);
            if (targetEvent != null) 
                targetFuncName = targetEvent.Name;
        }
        
        
        switch (action.ActionType) {
            case ActionType.SetParam:
                actionText += $" SetParam → {targetObject} {targetParam} = " +
                              $"({string.Join(",", action.SourceParams ?? [])})";
                break;
            case ActionType.SetExpression:
                actionText += $" SetExpression → {targetObject} {targetParam} = " +
                              $"{PreviewHelper.Preview(action.SourceExpression)}";
                break;
            case ActionType.Math:
                switch (action.MathOperationType) {
                    case MathOperationType.Addition:
                        actionText += $" Add {targetObject} {targetParam} + " +
                                      $"{string.Join(",", action.SourceParams ?? [])}";
                        break;
                    case MathOperationType.Subtraction:
                        actionText += $" Subtract {targetObject} {targetParam} - " +
                                      $"{string.Join(",", action.SourceParams ?? [])}";
                        break;
                    case MathOperationType.Multiply:
                        actionText += $" Multiply {targetObject} {targetParam} * " +
                                      $"{string.Join(",", action.SourceParams ?? [])}";
                        break;
                    case MathOperationType.Division:
                        actionText += $" Divide {targetObject} {targetParam} / " +
                                      $"{string.Join(",", action.SourceParams ?? [])}";
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                break;
            case ActionType.DoFunction:
                actionText += $" DoFunction → {targetObject} {targetParam} " +
                              $"{action.TargetFuncName}({string.Join(",", action.SourceParams ?? [])})";
                break;
            case ActionType.RaiseEvent:
                actionText += $" RaiseEvent → {targetObject} {targetParam} " +
                              $"{targetFuncName}({string.Join(",", action.SourceParams ?? [])})";
                break;
        }

        return new TreeNode(actionText) {
            Tag = action,
            ToolTipText = $"ID: {action.Id}\nType: {action.ActionType.Serialize()}\nOrder: {action.OrderIndex}",
            ForeColor = Color.DarkBlue
        };
    }

    private TreeNode CreateActionLineNode(ActionLine actionLine) {
        var node = new TreeNode($"[ActionLine] {actionLine.Name} [{actionLine.ActionLineType.Serialize()}]") {
            Tag = actionLine,
            ToolTipText = $"ID: {actionLine.Id}\nType: {actionLine.ActionLineType.Serialize()}",
            ForeColor = Color.DarkGreen
        };

        // Recursively add child actions/actionlines
        if (actionLine.Actions != null && actionLine.Actions.Any()) {
            foreach (var action in actionLine.Actions) {
                var childNode = action.Element switch {
                    Action a => CreateActionNode(a),
                    ActionLine al => CreateActionLineNode(al),
                    _ => null
                };

                if (childNode != null)
                    node.Nodes.Add(childNode);
            }
        }

        return node;
    }

    private string GetContextName(ActionLine actionLine) {
        return actionLine.LocalContext.Element switch {
            State s => $"State: {s.Name}",
            Graph g => $"Graph: {g.Name}",
            Branch b => $"Branch: {b.Name}",
            Talking t => $"Talking: {t.Name}",
            Speech sp => $"Speech: {sp.Name}",
            _ => "Unknown Context"
        };
    }

    private void OnNodeDoubleClick(object? sender, TreeNodeMouseClickEventArgs e) {
        if (e.Node?.Tag == null)
            return;

        // TODO: Implement action/actionline editor dialog
        MessageBox.Show(
            $"Editing not yet implemented.\n\nSelected: {e.Node.Text}\nID: {GetElementId(e.Node.Tag)}",
            "Not Implemented",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information
        );
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

    private static ulong GetElementId(object tag) {
        return tag switch {
            ActionLine al => al.Id,
            Action a => a.Id,
            _ => 0
        };
    }
}