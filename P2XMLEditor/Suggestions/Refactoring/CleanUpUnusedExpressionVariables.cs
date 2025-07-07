using System.Diagnostics.CodeAnalysis;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.GameData.VirtualMachineElements.Enums;
using P2XMLEditor.Suggestions.Abstract;
using P2XMLEditor.Suggestions.Attributes;

namespace P2XMLEditor.Suggestions.Refactoring;

[Refactoring("Refactor/Clean up unused expression variables")]
[SuppressMessage("ReSharper", "UnusedType.Global")]
public class CleanUpUnusedExpressionVariables(VirtualMachine vm) : Suggestion(vm) {
    public override void Execute() {
        foreach (var expression in Vm.GetElementsByType<Expression>()) {
            switch (expression.ExpressionType) {
                case ExpressionType.Function:
                    expression.Const = null;
                    expression.FormulaChilds = null;
                    expression.FormulaOperations = null;
                    expression.TargetParam = null;
                    break;
                case ExpressionType.Complex:
                    expression.Const = null;
                    expression.TargetParam = null;
                    expression.Function = null;
                    break;
                case ExpressionType.Const:
                    expression.Function = null;
                    expression.FormulaChilds = null;
                    expression.FormulaOperations = null;
                    expression.TargetParam = null;
                    break;
                case ExpressionType.Param:
                    expression.Const = null;
                    expression.Function = null;
                    expression.FormulaChilds = null;
                    expression.FormulaOperations = null;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}