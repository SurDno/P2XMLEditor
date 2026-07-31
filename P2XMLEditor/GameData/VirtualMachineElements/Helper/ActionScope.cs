using System.Collections.Generic;
using System.Runtime.CompilerServices;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes;
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
}
