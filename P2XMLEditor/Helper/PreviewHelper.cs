using System;
using System.Linq;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.GameData.VirtualMachineElements.Enums;
using P2XMLEditor.GameData.VirtualMachineElements.Interfaces;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes;
using Action = P2XMLEditor.GameData.VirtualMachineElements.Action;

namespace P2XMLEditor.Helper;

public static class PreviewHelper {

	public static string Preview(VmElement vmElement) => vmElement switch {
		PartCondition pc => Preview(pc),
		Condition cond => Preview(cond),
		Expression expr => Preview(expr),
		Action action => Preview(action),
		_ => throw new ArgumentOutOfRangeException()
	};

	public static string Preview(Condition cond) {
		return
			$"({string.Join(cond.Operation switch { ConditionOperation.And => " && ", ConditionOperation.Or => " || ",
				ConditionOperation.Xor => " ^ ", ConditionOperation.Root => "" }, cond.Predicates.Select(p =>
				p.Element switch { Condition subcond => Preview(subcond), PartCondition pc => Preview(pc) }))})";
	}

	public static string Preview(PartCondition pc) {
		return pc.ConditionType switch {
			ConditionType.ConstFalse => "false",
			ConditionType.ConstTrue => "true",
			ConditionType.ValueLess => $"{Preview(pc.FirstExpression!)} < {Preview(pc.SecondExpression!)}",
			ConditionType.ValueLessEqual => $"{Preview(pc.FirstExpression!)} <= {Preview(pc.SecondExpression!)}",
			ConditionType.ValueLarger => $"{Preview(pc.FirstExpression!)} > {Preview(pc.SecondExpression!)}",
			ConditionType.ValueLargerEqual => $"{Preview(pc.FirstExpression!)} >= {Preview(pc.SecondExpression!)}",
			ConditionType.ValueEqual => $"{Preview(pc.FirstExpression!)} == {Preview(pc.SecondExpression!)}",
			ConditionType.ValueNotEqual => $"{Preview(pc.FirstExpression!)} != {Preview(pc.SecondExpression!)}",
			ConditionType.ValueExpression => $"{Preview(pc.FirstExpression!)}",
			_ => throw new NotImplementedException()
		};
	}

	public static string Preview(Parameter p) => Preview(p.Value) ?? p.SerializedValue;

	/// <summary>
	/// One stored value, by whatever is readable about it. Null when there is nothing better to
	/// say than what the xml holds, which the caller falls back to.
	/// </summary>
	private static string? Preview(ParameterValue? value) {
		return value switch {
			// A path first: HierarchyRefValue is also a reference, and its last element is a poor
			// answer to "what is this" when the route to it is the whole point.
			IHierarchyValue path => Preview(path.Hierarchy),
			// A list of references reads as its members; serialized it is one long LIST&ELEM run.
			CommonListValue list => list.TypedValue.Count == 0
				? "(empty list)"
				: string.Join(", ", list.TypedValue.Select(e => Preview(e) ?? e.Serialize())),
			// Text reads as its text, in whichever language has any.
			RefValue<GameString> textRef => textRef.TypedValue?.GetText("English") is { Length: > 0 } en ? en :
											textRef.TypedValue?.GetText("Russian") is { Length: > 0 } ru ? ru :
											textRef.TypedValue?.Id.ToString(),
			// Anything else pointing at an element reads as that element's name. Through the
			// interface, because the generic test only ever matched RefValue<VmElement> exactly —
			// which nothing in the data is, so every object, state and sample reference printed
			// its id.
			IElementValue { Element: { } element } =>
				(element as INamedElement)?.Name ?? element.Id.ToString(),
			BasicValue<bool> b => b.TypedValue ? "true" : "false",
			_ => null
		};
	}

	public static string Preview(Expression? expression) {
		return expression?.ExpressionType switch {
			null => "<none>",
			ExpressionType.Const => Preview(expression.Const!),
			ExpressionType.Function =>
				$"{expression.Function!.Name}({string.Join(',', GetPreviewParamStrings(expression.Function!))})",
			ExpressionType.Param => Preview(expression.TargetObject) + " " + Preview(expression.TargetParam),
			ExpressionType.Complex => "Not supported expression",
			_ => throw new ArgumentOutOfRangeException()
		};
	}

	private static System.Collections.Generic.IEnumerable<string> GetPreviewParamStrings(P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract.VmFunction function) {
		var raw = function.GetParamStrings();
		if (raw == null || raw.Count == 0) return [];

		var properties = P2XMLEditor.GameData.VirtualMachineElements.Helper.FunctionSignature.SlotProperties(function.GetType());
		if (properties.Length != raw.Count) return raw;

		var results = new System.Collections.Generic.List<string>(raw.Count);
		for (var i = 0; i < properties.Length; i++) {
			if (P2XMLEditor.GameData.VirtualMachineElements.Helper.FunctionSignature.LiveSource(properties[i], function) is { } source) {
				// Before GetVariableName, which answers a hierarchy with its own ids — it names
				// variables for the writer, and there the ids are the point.
				results.Add(source.HierarchyReference != null
					? Preview(source.HierarchyReference)
					: source.GetVariableName() ?? source.Write());
			} else {
				results.Add(raw[i]);
			}
		}
		return results;
	}

	/// <summary>
	/// A path through the world, by the names of the things on it.
	///
	/// The ids in one say nothing: a hierarchy is written as "3204H4471H9982" and 45274 of the
	/// Sandbox's placements end at a node the xml never defines, so the names are the only
	/// readable part. They turn up in more places than a target object — an expression compares
	/// against one, a function takes one as an argument, a parameter holds one — and every one of
	/// those printed the raw ids until they all came through here.
	/// </summary>
	public static string Preview(HierarchyGuid? hierarchy) =>
		hierarchy == null
			? "<null>"
			: string.Join("/", hierarchy.Elements.Select(e => (e.Element as INamedElement)?.Name ?? e.Id.ToString()));

	public static string Preview(TargetObject targetObject) {
		if (!targetObject.IsSet) return "<null>";
		var name = targetObject.Kind switch {
			TargetObjectKind.Holder => targetObject.Holder?.Name,
			TargetObjectKind.ParameterRef => targetObject.ParameterRef?.Name,
			// A hierarchy target is a path through the world, and the ids in it say nothing at
			// all — 45274 of the Sandbox's placements end at a node the xml never defines, so
			// the names are the only readable part of one.
			TargetObjectKind.Hierarchy => targetObject.Hierarchy == null ? null : Preview(targetObject.Hierarchy),
			TargetObjectKind.Message => targetObject.Message?.Name,
			TargetObjectKind.InputParam => targetObject.InputParam?.Name,
			TargetObjectKind.Loop => targetObject.Loop?.ParamId,
			_ => null
		};
		var result = name ?? targetObject.Write();
		return targetObject.HasLeadingPercent ? "%" + result : result;
	}

	public static string Preview(ParamTarget target) => target.Kind switch {
		ParamTargetKind.Parameter =>
			(target.Parameter?.Element as Parameter)?.Name ?? target.Parameter?.Id.ToString() ?? "",
		ParamTargetKind.ComponentParam => target.ComponentParamName ?? "",
		_ => ""
	};

	/// <summary>
	/// An action as one line of something like code: what it writes, and what it writes there.
	///
	/// The type alone — "ACTION_TYPE_SET_PARAM" — is the one thing about an action that is never
	/// in question when reading a graph. What is in question is which parameter of which object
	/// it sets and to what, and none of that is visible until it is written out.
	/// </summary>
	public static string Preview(Action action) {
		string body;
		try {
			var target = Preview(action.TargetObject);
			var param = Preview(action.TargetParam);
			// The same rendering the expression preview uses: an action's arguments were the raw
			// strings, so a hierarchy printed as ids and so did everything else.
			var arguments = string.Join(", ", action.Function != null
				? GetPreviewParamStrings(action.Function)
				: action.GetParamStrings() ?? []);

			body = action.ActionType switch {
				ActionType.SetParam => $"{target}.{param} = {arguments}",
				ActionType.SetExpression => $"{target}.{param} = {Preview(action.SourceExpression)}",
				ActionType.Math => $"{target}.{param} {MathSymbol(action.MathOperationType)}= {arguments}",
				ActionType.DoFunction =>
					$"{target}.{action.Function?.Name ?? action.TargetFuncName}({arguments})",
				ActionType.RaiseEvent =>
					$"{target} ⇒ {action.EventToRaise?.Name ?? action.TargetFuncName}({arguments})",
				_ => $"{action.ActionType.Serialize()} {target}"
			};
		} catch {
			// A half-written action still has to be listable; the row is how it gets fixed.
			body = action.ActionType.Serialize();
		}

		return string.IsNullOrWhiteSpace(action.Name) ? body : $"{action.Name}:  {body}";
	}

	public static string MathSymbol(MathOperationType operation) => operation switch {
		MathOperationType.Addition => "+",
		MathOperationType.Subtraction => "-",
		MathOperationType.Multiply => "*",
		MathOperationType.Division => "/",
		_ => "?"
	};

	public static string Preview(ExpressionParamTarget? targetParam) {
		if (targetParam == null) return "<null>";
		var tp = targetParam.Value;
		var name = tp.Kind switch {
			ExpressionParamKind.Param => tp.Param?.Parameter?.Element is Parameter p ? p.Name : null,
			ExpressionParamKind.Message => tp.Message?.Name,
			ExpressionParamKind.InputParam => tp.InputParam?.Name,
			ExpressionParamKind.ObjectLiteral => tp.LiteralHierarchy != null
				? Preview(tp.LiteralHierarchy)
				: tp.ObjectLiteral is INamedElement ne ? ne.Name : null,
			_ => null
		};
		var result = name ?? tp.Write();
		return tp.HasLeadingPercent ? "%" + result : result;
	}

}
