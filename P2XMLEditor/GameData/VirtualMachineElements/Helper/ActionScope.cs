using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.Enums;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes;
using P2XMLEditor.GameData.VirtualMachineElements.Placeholders;
using P2XMLEditor.Helper;
using VmAction = P2XMLEditor.GameData.VirtualMachineElements.Action;

namespace P2XMLEditor.GameData.VirtualMachineElements.Helper;

/// <summary>
/// The local variables an action may legally reference.
///
/// This mirrors the engine's own rule (VMState.GetLocalContextVariables): a message is in
/// scope where the graph can *reach* it, so the walk goes backwards through InputLinks
/// collecting the messages of every traversed link's Event, and at a graph boundary ascends
/// to the graph as a node in its parent, picking up that graph's InputParams on the way.
///
/// Ownership is emphatically not the rule. <see cref="EventAccessibilityUtility"/> answers a
/// different question — which events an object owns — and an engine event lives on a
/// FunctionalComponent that is routinely unrelated to the action's holder. Measured over
/// PathologicSandbox, every one of the 114 message references and 124 input-param references
/// made by an action is in scope under this walk; the ownership rule reaches only 86 of the
/// messages. Both halves of the walk are load-bearing: dropping the graph ascent takes it to
/// 54/114, dropping the transitive source walk to 68/114.
///
/// It stays tight as well as complete — median 0 and at most 10 messages offered at any of
/// the corpus's 23133 actions, against 170 declared corpus-wide.
/// </summary>
public sealed class ActionScope {
	/// <summary>Messages reachable at the action's local context, in discovery order.</summary>
	public IReadOnlyList<Message> Messages { get; }

	/// <summary>Input parameters declared by any enclosing graph.</summary>
	public IReadOnlyList<InputParameter> InputParams { get; }

	/// <summary>Loop index/element variables of every loop the action sits inside.</summary>
	public IReadOnlyList<LoopParameter> LoopVariables { get; }

	/// <summary>The State/Graph/Branch/Talking/Speech the action lives in.</summary>
	public VmElement? LocalContext { get; }

	/// <summary>Innermost action line containing the action, if any.</summary>
	public ActionLine? OwningLine { get; }

	/// <summary>Object the local context runs on — the default target for a new action.</summary>
	public ParameterHolder? Owner { get; }

	private ActionScope(IReadOnlyList<Message> messages, IReadOnlyList<InputParameter> inputParams,
		IReadOnlyList<LoopParameter> loopVariables, VmElement? localContext, ActionLine? owningLine,
		ParameterHolder? owner) {
		Messages = messages;
		InputParams = inputParams;
		LoopVariables = loopVariables;
		LocalContext = localContext;
		OwningLine = owningLine;
		Owner = owner;
	}

	public static ActionScope Empty { get; } = new([], [], [], null, null, null);

	public static ActionScope For(VmAction action, VirtualMachine vm) =>
		For(action.LocalContext.Element, FindOwningLine(action, vm), vm);

	public static ActionScope For(ActionLine line, VirtualMachine vm) =>
		For(line.LocalContext.Element, FindOwningLine(line, vm), vm);

	public static ActionScope For(VmElement? localContext, ActionLine? owningLine, VirtualMachine vm) {
		var messages = new List<Message>();
		var inputParams = new List<InputParameter>();
		Walk(localContext, messages, inputParams);
		return new ActionScope(messages, inputParams, CollectLoopVariables(owningLine, vm),
			localContext, owningLine, OwnerOf(localContext));
	}

	/// <summary>
	/// The scope a <see cref="GraphLink"/>'s arguments are written in.
	///
	/// Not the reachability walk: a link is not inside the graph's flow, it is the thing that
	/// starts it. What it has to hand is exactly its own event's payload — the messages of the
	/// event it subscribes to — plus the input parameters of the graph that owns it, which are
	/// already bound by the time the link fires. There is no action line, so no loop variables.
	/// </summary>
	public static ActionScope ForLink(GraphLink link, VirtualMachine vm) {
		var parent = link.Parent.Element;
		return new ActionScope(
			link.Event?.Messages ?? [],
			(parent as Graph)?.InputParams ?? [],
			[],
			parent,
			null,
			OwnerOf(parent));
	}

	/// <summary>
	/// Backwards reachability walk. Every node is visited at most once, so a cyclic graph
	/// terminates and a node reachable by several routes contributes its messages once.
	/// </summary>
	private static void Walk(VmElement? start, List<Message> messages, List<InputParameter> inputParams) {
		if (start == null) return;

		var seenNodes = new HashSet<VmElement>();
		var seenMessages = new HashSet<Message>();
		var seenInputParams = new HashSet<InputParameter>();
		var pending = new Stack<VmElement>();
		pending.Push(start);

		while (pending.Count > 0) {
			var node = pending.Pop();
			if (!seenNodes.Add(node)) continue;

			// A graph contributes its own declarations, whether we started inside it or
			// arrived by ascending from a child.
			if (node is Graph graph)
				foreach (var inputParam in graph.InputParams ?? [])
					if (seenInputParams.Add(inputParam))
						inputParams.Add(inputParam);

			foreach (var link in InputLinksOf(node)) {
				foreach (var message in link.Event?.Messages ?? [])
					if (seenMessages.Add(message))
						messages.Add(message);

				// An unconditional link carries the source's own scope forward, so the walk
				// has to continue past it rather than stop at the first link.
				if (link.Source?.Element is { } source)
					pending.Push(source);
			}

			if (ParentNodeOf(node) is { } parent)
				pending.Push(parent);
		}
	}

	private static IEnumerable<GraphLink> InputLinksOf(VmElement node) {
		List<GraphLink>? links = node switch {
			State s => s.InputLinks,
			Graph g => g.InputLinks,
			Branch b => b.InputLinks,
			Talking t => t.InputLinks,
			Speech sp => sp.InputLinks,
			_ => null
		};
		return links ?? [];
	}

	/// <summary>
	/// The enclosing graph node. A Graph whose parent is a ParameterHolder is the object's
	/// root event graph and ends the walk.
	/// </summary>
	private static VmElement? ParentNodeOf(VmElement node) => node switch {
		State s => s.Parent,
		Graph g => g.Parent.Element as Graph,
		Branch b => b.Parent.Element,
		Talking t => t.Parent,
		Speech sp => sp.Parent,
		_ => null
	};

	private static ParameterHolder? OwnerOf(VmElement? node) {
		for (var guard = 0; guard < 32 && node != null; guard++) {
			switch (node) {
				case State s: return s.Owner;
				case Graph g: return g.Owner;
				case Branch b: return b.Owner;
				case Talking t: return t.Owner.Element as ParameterHolder;
				case Speech sp: node = sp.Parent; break;
				default: return null;
			}
		}
		return null;
	}

	/// <summary>
	/// Loop variables of every enclosing loop, innermost first. Only lines that actually
	/// carry <see cref="ActionLine.LoopInfo"/> declare any — the local_&lt;id&gt;_Loop_* names
	/// are scoped to their own line and are not visible to siblings.
	/// </summary>
	private static List<LoopParameter> CollectLoopVariables(ActionLine? line, VirtualMachine vm) {
		var result = new List<LoopParameter>();
		var index = ParentIndexOf(vm);

		for (var guard = 0; guard < 64 && line != null; guard++) {
			if (line.LoopInfo != null) {
				result.Add(new LoopParameter(line, isIndex: true, null));
				var listName = line.LoopInfo.Name.GetVariableName();
				if (!string.IsNullOrEmpty(listName))
					result.Add(new LoopParameter(line, isIndex: false, listName));
			}
			line = index.GetValueOrDefault(line.Id);
		}

		return result;
	}

	public static ActionLine? FindOwningLine(VmElement element, VirtualMachine vm) =>
		ParentIndexOf(vm).GetValueOrDefault(element.Id);

	// Actions and action lines record their local context but not the line that holds them,
	// so the containment edges are indexed once per VirtualMachine and reused. The action
	// editor never reparents a line, so the index cannot go stale under it.
	private static readonly ConditionalWeakTable<VirtualMachine, Dictionary<ulong, ActionLine>> ParentIndexCache = new();

	private static Dictionary<ulong, ActionLine> ParentIndexOf(VirtualMachine vm) =>
		ParentIndexCache.GetValue(vm, BuildParentIndex);

	private static Dictionary<ulong, ActionLine> BuildParentIndex(VirtualMachine vm) {
		var index = new Dictionary<ulong, ActionLine>();
		foreach (var line in vm.GetElementsByType<ActionLine>())
			foreach (var child in line.Actions ?? [])
				index[child.Id] = line;
		return index;
	}

	/// <summary>
	/// Events the action may raise on <paramref name="target"/>. Raising is a call against a
	/// specific object, so this is genuinely the ownership question and
	/// <see cref="EventAccessibilityUtility"/> is the right answer here.
	/// </summary>
	public static IEnumerable<Event> RaisableEvents(ParameterHolder? target, VirtualMachine vm) =>
		target == null ? [] : EventAccessibilityUtility.GetAccessibleEvents(target, vm);

	/// <summary>
	/// Names of every functional component the object has. This is what decides which
	/// functions can be called on it: a function is named "&lt;Component&gt;.&lt;Method&gt;"
	/// and every one of the 29 prefixes is a component name.
	///
	/// The walk follows InheritanceInfo — the prototype an object was derived from — and
	/// emphatically not Parent, which is scene nesting. Every object's Parent chain ends at
	/// the GameRoot, so walking it hands all 7291 holders the global managers and the filter
	/// degenerates to no filter at all. Following inheritance instead, GlobalStorageManager
	/// belongs to exactly one holder, and the rule still covers all 10632 DoFunction calls in
	/// PathologicSandbox whose target resolves (own components alone miss one).
	/// </summary>
	public static IReadOnlySet<string> ComponentsOf(ParameterHolder? target, VirtualMachine vm) {
		var names = new HashSet<string>(StringComparer.Ordinal);
		if (target == null) return names;

		var visited = new HashSet<ParameterHolder>();
		var pending = new Stack<ParameterHolder>();
		pending.Push(target);

		while (pending.Count > 0) {
			var holder = pending.Pop();
			if (!visited.Add(holder)) continue;

			foreach (var component in holder.FunctionalComponents ?? [])
				if (!string.IsNullOrEmpty(component.Name))
					names.Add(component.Name);

			foreach (var prototype in holder.InheritanceInfo ?? [])
				if (ulong.TryParse(prototype, out var id) &&
					vm.GetNullableElement<ParameterHolder>(id) is { } inherited)
					pending.Push(inherited);
		}

		return names;
	}

	/// <summary>One parameter an object can be asked for, and where it was declared.</summary>
	/// <param name="DeclaredOn">
	/// The object carrying the declaration — the target itself, or a blueprint it derives from.
	/// </param>
	public readonly record struct AvailableParameter(Parameter Parameter, ParameterHolder DeclaredOn, bool Inherited);

	/// <summary>
	/// Every parameter an object can be asked for: its own, and the ones it inherits.
	///
	/// An inherited parameter is not copied per instance — the declaration is shared and only the
	/// value is per-object — so an action that writes one names the *blueprint's* parameter id.
	/// The engine resolves it against the object all the same: VMBlueprint.GetContextVariables
	/// merges every base blueprint's params ahead of the object's own, FSMParamsManager keys them
	/// all by their declaring guid, and the lookup is GetContextParam(BaseGuid) with a fallback by
	/// name. 393 references across the two corpora resolve exactly this way, and offering only an
	/// object's own parameters made every one of them unpickable.
	///
	/// A parameter whose name the object already declares itself is dropped. That is the standard
	/// component set, which every object redeclares with ids of its own: merging it back in adds a
	/// median of 36 entries per object of which 33 are duplicate names. With the rule it is a
	/// median of 3, and nothing is lost — all 393 references are to custom parameters, and not one
	/// of them shares a name with a parameter of the object itself.
	/// </summary>
	public static IReadOnlyList<AvailableParameter> ParametersOf(ParameterHolder? target, VirtualMachine vm) {
		var available = new List<AvailableParameter>();
		if (target == null) return available;

		var names = new HashSet<string>(StringComparer.Ordinal);
		var seen = new HashSet<ulong>();

		// Breadth-first from the object outwards, so a nearer declaration wins the name.
		var visited = new HashSet<ParameterHolder>();
		var pending = new Queue<ParameterHolder>();
		pending.Enqueue(target);

		while (pending.Count > 0) {
			var holder = pending.Dequeue();
			if (!visited.Add(holder)) continue;
			var inherited = !ReferenceEquals(holder, target);

			foreach (var parameter in OwnParameters(holder)) {
				if (!seen.Add(parameter.Id)) continue;
				var name = parameter.Name ?? "";
				if (inherited && !names.Add(name)) continue;
				names.Add(name);
				available.Add(new AvailableParameter(parameter, holder, inherited));
			}

			foreach (var prototype in holder.InheritanceInfo ?? [])
				if (ulong.TryParse(prototype, out var id) &&
					vm.GetNullableElement<ParameterHolder>(id) is { } prototypeHolder)
					pending.Enqueue(prototypeHolder);
		}

		return available;
	}

	/// <summary>A holder's own parameters, standard then custom, in a stable order.</summary>
	private static IEnumerable<Parameter> OwnParameters(ParameterHolder holder) {
		var standart = holder.StandartParams ?? new Dictionary<string, Parameter>();
		var custom = holder.CustomParams ?? new Dictionary<string, Parameter>();
		return standart.Concat(custom)
			.Where(kvp => kvp.Value != null)
			.OrderBy(kvp => kvp.Key, StringComparer.Ordinal)
			.Select(kvp => kvp.Value);
	}

	/// <summary>
	/// Components callable on whatever the action targets, or null when the target is only
	/// known at runtime and nothing constrains it.
	///
	/// A target that is not a concrete object still usually carries a type: a parameter or
	/// message declared "IObjRef%cf_Controller" promises a Controller, and that promise is
	/// the only thing the editor can honestly filter on.
	/// </summary>
	public static IReadOnlySet<string>? ComponentsOfTarget(TargetObject target, VirtualMachine vm) {
		if (target.ResolvedHolder is { } holder)
			// A placeholder stands in for an object the data references but does not define —
			// most of the world hierarchy is engine-only, 14102 of the Sandbox's 20459 placed
			// objects. It declares no components because nothing here knows any, which is not
			// the same as having none: answering with an empty set would offer no function at
			// all on a scene object that in fact supports plenty. Unknown is the honest answer.
			return holder is IPlaceholder ? null : ComponentsOf(holder, vm);

		return target.Kind switch {
			TargetObjectKind.ParameterRef => ComponentsOfDeclaredType(target.ParameterRef?.Type, vm),
			TargetObjectKind.Message => ComponentsOfDeclaredType(target.Message?.Type, vm),
			TargetObjectKind.InputParam => ComponentsOfDeclaredType(target.InputParam?.Type, vm),
			_ => null
		};
	}

	/// <summary>Whether an object carries a component, its inherited ones included.</summary>
	public static bool HasComponent(ParameterHolder holder, VmComponent component, VirtualMachine vm) =>
		component == VmComponent.None ||
		ComponentsOf(holder, vm).Contains(VmTypeHelper.SerializeComponent(component));

	/// <summary>
	/// The single object a runtime-decided target is nonetheless pinned to.
	///
	/// A parameter, message or input param declared "IObjRef%cf_&lt;blueprintId&gt;" can only ever
	/// hold that one object, so its parameters are known while authoring even though the
	/// reference is indirect. The data draws exactly this line: a parameter-ref target whose
	/// type pins a blueprint writes a concrete parameter id 703 times, and one whose type does
	/// not never writes an id at all — all 50 of those use a dynamic name.
	/// </summary>
	public static ParameterHolder? PinnedBlueprint(TargetObject target, VirtualMachine vm) {
		var declared = target.Kind switch {
			TargetObjectKind.ParameterRef => target.ParameterRef?.Type,
			TargetObjectKind.Message => target.Message?.Type,
			TargetObjectKind.InputParam => target.InputParam?.Type,
			_ => null
		};
		if (string.IsNullOrEmpty(declared)) return null;

		try {
			var info = VmTypeHelper.GetVmTypeInfo(declared, vm);
			return info.BaseType == VmType.GameObject ? info.ObjBlueprint : null;
		} catch {
			return null;
		}
	}

	private static IReadOnlySet<string>? ComponentsOfDeclaredType(string? xmlType, VirtualMachine vm) {
		if (string.IsNullOrEmpty(xmlType)) return null;

		VmTypeInfo info;
		try {
			info = VmTypeHelper.GetVmTypeInfo(xmlType, vm);
		} catch {
			return null;
		}

		if (info.BaseType != VmType.GameObject) return null;
		// "IObjRef%cf_<id>" pins the target to one object, so its own components are the answer.
		if (info.ObjBlueprint != null) return ComponentsOf(info.ObjBlueprint, vm);
		// A bare IObjRef promises nothing; there is nothing to filter on.
		if (info.RequiredComponents.IsEmpty) return null;

		var names = new HashSet<string>(StringComparer.Ordinal);
		foreach (var component in RequiredComponents(info.RequiredComponents))
			names.Add(VmTypeHelper.SerializeComponent(component));
		return names;
	}

	private static IEnumerable<VmComponent> RequiredComponents(VmComponentMask mask) {
		if (!mask.IsOrdered) return mask.CustomOrder ?? [];

		var components = new List<VmComponent>();
		foreach (var component in Enum.GetValues<VmComponent>())
			if (component != VmComponent.None && (mask.Mask & component) != VmComponent.None)
				components.Add(component);
		return components;
	}
}
