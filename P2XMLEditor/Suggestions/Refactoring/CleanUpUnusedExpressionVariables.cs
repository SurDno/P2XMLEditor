using System;
using System.Diagnostics.CodeAnalysis;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.GameData.VirtualMachineElements.Enums;
using P2XMLEditor.Logging;

namespace P2XMLEditor.Suggestions.Refactoring;

[Refactoring("Refactor/Clean up unused expression variables"), SuppressMessage("ReSharper", "UnusedType.Global")]
public class CleanUpUnusedExpressionVariables(VirtualMachine vm) : Suggestion(vm) {
	public override void Execute() {
		int count = 0;
		foreach (var expression in Vm.GetElementsByType<Expression>()) {
			bool cleaned = false;
			switch (expression.ExpressionType) {
				case ExpressionType.Function:
					if (expression.Const != null || expression.FormulaChilds != null || expression.FormulaOperations != null || expression.TargetParam != null) cleaned = true;
					expression.Const = null;
					expression.FormulaChilds = null;
					expression.FormulaOperations = null;
					expression.TargetParam = null;
					break;
				case ExpressionType.Complex:
					if (expression.Const != null || expression.TargetParam != null || expression.Function != null) cleaned = true;
					expression.Const = null;
					expression.TargetParam = null;
					expression.Function = null;
					break;
				case ExpressionType.Const:
					if (expression.Function != null || expression.FormulaChilds != null || expression.FormulaOperations != null || expression.TargetParam != null) cleaned = true;
					expression.Function = null;
					expression.FormulaChilds = null;
					expression.FormulaOperations = null;
					expression.TargetParam = null;
					break;
				case ExpressionType.Param:
					if (expression.Const != null || expression.Function != null || expression.FormulaChilds != null || expression.FormulaOperations != null) cleaned = true;
					expression.Const = null;
					expression.Function = null;
					expression.FormulaChilds = null;
					expression.FormulaOperations = null;
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
			if (cleaned) {
				Logger.Log(LogLevel.Info, $"Cleaned up unused variables for Expression {expression.Id} ({expression.ExpressionType})");
				count++;
			}
		}
		Logger.Log(LogLevel.Info, $"Completed: Cleaned up {count} expression variables.");
	}
}
