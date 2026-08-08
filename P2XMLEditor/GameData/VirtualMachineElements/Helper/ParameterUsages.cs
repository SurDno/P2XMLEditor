using System;
using System.Collections.Generic;
using System.Linq;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.GameData.VirtualMachineElements.Enums;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;
using VmAction = P2XMLEditor.GameData.VirtualMachineElements.Action;

namespace P2XMLEditor.GameData.VirtualMachineElements.Helper;

/// <summary>Whether a mention of a parameter puts a value into it or takes one out.</summary>
public enum ParameterUse {
	/// <summary>The parameter is assigned: it is an action's target param.</summary>
	Written,

	/// <summary>The parameter's value is consumed — as a value, or as the object to act on.</summary>
	Read,

	/// <summary>
	/// An event fires when the parameter changes. Nothing consumes the value and nothing assigns
	/// it, so this is neither of the other two: an event with an EventParameter is
	/// EVENT_RAISING_TYPE_PARAM_CHANGE, and DynamicEventBody subscribes it to that parameter.
	/// Calling it a read would say something false about a parameter whose only mention in the
	/// data is this one — which is the case for 3 of them in PathologicSandbox.
	/// </summary>
	Watched
}

/// <param name="Owner">The element that mentions it — an action, an expression, a link, a line.</param>
/// <param name="Slot">Which part of that element, e.g. "target param" or "argument 2".</param>
public readonly record struct ParameterUsage(Parameter Parameter, ParameterUse Use, VmElement Owner, string Slot);

/// <summary>
/// Every mention of every parameter in the data, so a parameter can answer what writes it and
/// what reads it.
///
/// A parameter is named from more places than is obvious, and a list that misses one is worse
/// than no list — it reads as proof that nothing else touches the value. So this walks all four
/// element types that can name one:
///
/// * an action's TargetParam is the one thing that <em>writes</em> a parameter, whatever the
///   action type: SetParam, Math and SetExpression assign it, and DoFunction stores the call's
///   result in it;
/// * an action also reads parameters — as the object it acts on (TargetObject naming an IObjRef
///   parameter), as its source value, as an event argument, and in every slot of the function
///   it calls;
/// * an expression reads them the same three ways, as its object, its param, or a function slot;
/// * a graph link's arguments and an action line's loop bounds are parameter sources too.
///
/// Building the index once and reusing it matters: the corpus holds 23133 actions and 8411
/// expressions, and answering "who touches this?" per parameter by rescanning would be a scan
/// per click.
/// </summary>
public sealed class ParameterUsageIndex {
	private readonly Dictionary<ulong, List<ParameterUsage>> _byParameter = [];

	public IReadOnlyList<ParameterUsage> Of(Parameter? parameter) =>
		parameter != null && _byParameter.TryGetValue(parameter.Id, out var usages) ? usages : [];

	public int Count => _byParameter.Values.Sum(list => list.Count);

	public static ParameterUsageIndex Build(VirtualMachine vm) {
		var index = new ParameterUsageIndex();

		foreach (var action in vm.GetElementsByType<VmAction>()) index.AddAction(action, vm);
		foreach (var expression in vm.GetElementsByType<Expression>()) index.AddExpression(expression, vm);
		foreach (var line in vm.GetElementsByType<ActionLine>()) index.AddLine(line);
		foreach (var link in vm.GetElementsByType<GraphLink>()) index.AddLink(link, vm);

		// An event raised by a parameter changing names that parameter and nothing else does:
		// 188 events in PathologicSandbox and 2 in MarbleNest are declared this way, and for
		// three of those parameters it is the only mention in the entire game.
		foreach (var raised in vm.GetElementsByType<Event>())
			index.Add(raised.EventParameter, ParameterUse.Watched, raised, "raised when it changes");

		return index;
	}

	private void Add(Parameter? parameter, ParameterUse use, VmElement owner, string slot) {
		// An expression's constant operand is a Parameter too, but it is storage for a literal
		// rather than a value on an object; it has no holder to list it under.
		if (parameter == null || parameter.IsConstant) return;
		if (!_byParameter.TryGetValue(parameter.Id, out var list)) _byParameter[parameter.Id] = list = [];
		list.Add(new ParameterUsage(parameter, use, owner, slot));
	}

	// ---------------------------------------------------------------- actions

	private void AddAction(VmAction action, VirtualMachine vm) {
		Add(TargetParameter(action.TargetParam), ParameterUse.Written, action,
			action.ActionType == ActionType.DoFunction ? "result destination" : "target param");

		AddTargetObject(action.TargetObject, action, "target object");
		AddSource(action.Source, ParameterUse.Read, action, "source");

		var eventParams = action.EventParams ?? [];
		for (var i = 0; i < eventParams.Count; i++)
			AddSource(eventParams[i], ParameterUse.Read, action, $"event argument {i + 1}");

		AddFunction(action.Function, action, vm);
	}

	private void AddExpression(Expression expression, VirtualMachine vm) {
		AddTargetObject(expression.TargetObject, expression, "object");

		if (expression.TargetParam is { Kind: ExpressionParamKind.Param, Param: { } param })
			Add(TargetParameter(param), ParameterUse.Read, expression, "param");

		AddFunction(expression.Function, expression, vm);
	}

	private void AddLine(ActionLine line) {
		if (line.LoopInfo is not { } loop) return;
		AddSource(loop.Name, ParameterUse.Read, line, "loop list");
		AddSource(loop.Start, ParameterUse.Read, line, "loop start");
		AddSource(loop.End, ParameterUse.Read, line, "loop end");
	}

	/// <summary>
	/// A link's arguments are stored as the strings the engine parses, so they have to be parsed
	/// here too. A value that no longer resolves is skipped rather than guessed at — this is a
	/// report, and a wrong entry in it is worse than a missing one.
	/// </summary>
	private void AddLink(GraphLink link, VirtualMachine vm) {
		var arguments = link.SourceParams ?? [];
		for (var i = 0; i < arguments.Count; i++) {
			if (string.IsNullOrEmpty(arguments[i])) continue;
			try {
				AddSource(ParameterSource.Create(arguments[i], vm), ParameterUse.Read, link, $"argument {i + 1}");
			} catch {
				// Unparseable argument: nothing to report against.
			}
		}
	}

	// ---------------------------------------------------------------- pieces

	private static Parameter? TargetParameter(ParamTarget target) =>
		target.Kind == ParamTargetKind.Parameter ? target.Parameter?.Element as Parameter : null;

	private void AddTargetObject(TargetObject target, VmElement owner, string slot) {
		// "The object held by this parameter" reads the parameter to find the object; the write,
		// if there is one, lands on whatever it points at.
		if (target.Kind == TargetObjectKind.ParameterRef) Add(target.ParameterRef, ParameterUse.Read, owner, slot);
	}

	private void AddSource(ParameterSource? source, ParameterUse use, VmElement owner, string slot) {
		if (source is not { } value) return;
		Add(value.ParameterReference, use, owner, slot);
		// "%obj%ParamName" — the object comes out of one parameter and a parameter of that object
		// is read by name. The object parameter is a genuine read; the named one is resolved at
		// runtime and has no element here to attribute it to.
		Add(value.DynamicObjectReference, ParameterUse.Read, owner, slot);
	}

	/// <summary>
	/// Every slot of a called function. The values live on the function instance rather than on
	/// the action, one FunctionSourceParam per declared parameter, which is what
	/// <see cref="FunctionSignature"/> already knows how to enumerate.
	/// </summary>
	private void AddFunction(VmFunction? function, VmElement owner, VirtualMachine vm) {
		if (function == null) return;

		var properties = FunctionSignature.SlotProperties(function.GetType());
		for (var i = 0; i < properties.Length; i++) {
			ParameterSource? source;
			try {
				source = FunctionSignature.LiveSource(properties[i], function);
			} catch {
				continue;
			}
			AddSource(source, ParameterUse.Read, owner, $"{function.Name} — {properties[i].Name}");
		}
	}

	// ---------------------------------------------------------------- describing a usage

	/// <summary>
	/// Where a usage sits, as a path from the object down to the node the action runs in. Without
	/// it the list is a column of action ids: the whole value of the report is being able to see
	/// that a parameter is written in one graph and read in four others.
	/// </summary>
	public static string ContextOf(VmElement owner) {
		var node = owner switch {
			VmAction action => action.LocalContext.Element,
			Expression expression => expression.LocalContext.Element,
			ActionLine line => line.LocalContext.Element,
			GraphLink link => link.Parent.Element,
			_ => null
		};

		var path = new List<string>();
		var guard = 0;
		for (var current = node; current != null && guard++ < 16; current = ParentOf(current))
			path.Add(GraphTopology.NameOf(current) is { Length: > 0 } name ? name : current.Id.ToString());

		path.Reverse();
		return path.Count == 0 ? "" : string.Join(" ▸ ", path);
	}

	private static VmElement? ParentOf(VmElement element) {
		if (GraphTopology.ContainerOf(element) is { } container) return container;
		return element switch {
			Graph graph => graph.Parent.Element,
			Talking talking => talking.Owner.Element,
			Speech speech => speech.Parent,
			Event @event => @event.Parent.Element,
			_ => null
		};
	}
}
