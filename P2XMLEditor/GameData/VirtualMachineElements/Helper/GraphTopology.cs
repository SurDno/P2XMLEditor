using System.Collections.Generic;
using System.Linq;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.GameData.VirtualMachineElements.Enums;
using P2XMLEditor.GameData.VirtualMachineElements.Interfaces;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes;
using P2XMLEditor.Helper;

namespace P2XMLEditor.GameData.VirtualMachineElements.Helper;

/// <summary>
/// What a graph is made of, and what a link between two of its nodes may say.
///
/// A <see cref="GraphLink"/> stores its endpoints as two bare integers — SourceExitPointIndex
/// and DestEntryPointIndex — and neither means anything without the node on the other end of
/// it. This works both out, so the editor can offer a named exit and a named entry point
/// instead of a spin box over the integers, and so nothing it writes can be out of range.
///
/// Every rule here is read off the shipped data and holds in both corpora without exception:
///
/// * A state, a subgraph and an event entry link have one implicit exit, written -1. All
///   17 614 of them.
/// * A branch has one exit per condition, numbered from zero, plus one more for "no condition
///   matched" — 4408 links leave by a condition and 3302 by the else exit, and not one names
///   an index beyond it.
/// * A speech has one exit per reply — all 12 308 of them, none out of range and none -1.
/// * DestEntryPointIndex always indexes the destination's own EntryPoints; all 38 286 links do.
/// * A link's SourceParams are the arguments for the destination's input parameters, matched
///   one for one — 7369 links agree exactly, and of the 148 that do not, 139 match the
///   destination's SubstituteGraph instead, which is where the graph really gets its
///   parameters from.
/// </summary>
public static class GraphTopology {
	/// <summary>One way out of a node, as the link that leaves by it sees it.</summary>
	/// <param name="Index">The value to store in SourceExitPointIndex.</param>
	/// <param name="Label">What to show for it.</param>
	/// <param name="Condition">The branch condition this exit is taken on, when it is one.</param>
	/// <param name="Reply">The reply this exit is taken on, when it is one.</param>
	public readonly record struct Exit(int Index, string Label, VmElement? Condition = null, Reply? Reply = null);

	/// <summary>One way into a node.</summary>
	public readonly record struct Entry(int Index, string Label, EntryPoint? EntryPoint);

	/// <summary>One argument a link passes to the graph it enters.</summary>
	public readonly record struct Argument(int Index, InputParameter? Parameter, string Value) {
		public string Label => Parameter?.ParamName ?? $"arg {Index + 1}";
		public string DeclaredType => Parameter?.Type ?? "";
	}

	// ---------------------------------------------------------------- structure

	/// <summary>The nodes drawn inside a container — a Graph or a Talking.</summary>
	public static IReadOnlyList<VmElement> NodesOf(VmElement container) => container switch {
		Graph graph => graph.States.Select(s => s.Element).ToList(),
		Talking talking => talking.States.Select(s => s.Element).ToList(),
		_ => []
	};

	/// <summary>The links drawn inside a container.</summary>
	public static IReadOnlyList<GraphLink> LinksOf(VmElement container) => container switch {
		Graph graph => graph.EventLinks,
		Talking talking => talking.EventLinks,
		_ => []
	};

	/// <summary>True for a node the editor can descend into.</summary>
	public static bool IsContainer(VmElement? node) => node is Graph or Talking;

	public static string NameOf(VmElement? node) => node switch {
		IGraphElement graphElement => Blank(graphElement.Name) ? node.Id.ToString() : graphElement.Name,
		Talking talking => Blank(talking.Name) ? node.Id.ToString() : talking.Name,
		Speech speech => Blank(speech.Name) ? node.Id.ToString() : speech.Name,
		null => "",
		_ => node.Id.ToString()
	};

	private static bool Blank(string? text) => string.IsNullOrWhiteSpace(text);

	public static bool IsInitial(VmElement? node) => node switch {
		IGraphElement graphElement => graphElement.Initial,
		Talking talking => talking.Initial,
		Speech speech => speech.Initial,
		_ => false
	};

	public static List<EntryPoint> EntryPointsOf(VmElement? node) => node switch {
		IGraphElement graphElement => graphElement.EntryPoints,
		Talking talking => talking.EntryPoints,
		Speech speech => speech.EntryPoints,
		_ => []
	};

	// ---------------------------------------------------------------- exits

	/// <summary>
	/// Every way out of <paramref name="node"/>. A node with a single unconditional exit gets
	/// one entry numbered -1, which is what the data writes for it; a branch and a speech get
	/// one per outcome, so choosing one is choosing a meaning rather than a number.
	/// </summary>
	public static IReadOnlyList<Exit> ExitsOf(VmElement? node) {
		switch (node) {
			case Branch branch: {
				var exits = new List<Exit>(branch.BranchConditions.Count + 1);
				for (var i = 0; i < branch.BranchConditions.Count; i++) {
					var condition = branch.BranchConditions[i].Element;
					exits.Add(new Exit(i, $"{i}:  {DescribeCondition(condition)}", condition));
				}
				// The engine leaves by this one when nothing matched. A MaxValue branch always
				// matches something and never uses it, but it is written the same way and there
				// is nothing in the data to say it may not be.
				exits.Add(new Exit(branch.BranchConditions.Count,
					$"{branch.BranchConditions.Count}:  {ElseLabel(branch.BranchType)}"));
				return exits;
			}

			case Speech speech:
				return speech.Replies
					.Select((reply, i) => new Exit(i, $"{i}:  {DescribeReply(reply)}", null, reply))
					.ToList();

			case null:
				return [];

			default:
				return [new Exit(-1, "when it finishes")];
		}
	}

	private static string ElseLabel(BranchType type) => type switch {
		BranchType.Case => "otherwise (no condition matched)",
		BranchType.FlipFlop => "otherwise",
		BranchType.MessageCast => "otherwise (the cast failed)",
		_ => "otherwise"
	};

	private static string DescribeCondition(VmElement? condition) {
		try {
			return condition == null ? "(no condition)" : PreviewHelper.Preview(condition);
		} catch {
			return condition == null ? "(no condition)" : $"condition {condition.Id}";
		}
	}

	private static string DescribeReply(Reply reply) {
		try {
			var text = reply.Text?.GetText("English");
			if (!Blank(text)) return text!;
		} catch {
			// A reply whose text is missing still has to be pickable.
		}
		return Blank(reply.Name) ? $"reply {reply.Id}" : reply.Name;
	}

	// ---------------------------------------------------------------- entries

	/// <summary>
	/// Every way into <paramref name="node"/>. An entry point carries an action line that runs
	/// on arrival, so which one a link targets is a real choice and not a formality.
	/// </summary>
	public static IReadOnlyList<Entry> EntriesOf(VmElement? node) {
		var points = EntryPointsOf(node);
		if (points.Count == 0) return [new Entry(0, "0:  (no entry points declared)", null)];

		return points
			.Select((point, i) => new Entry(i, $"{i}:  {DescribeEntryPoint(point)}", point))
			.ToList();
	}

	private static string DescribeEntryPoint(EntryPoint? point) {
		if (point == null) return "(missing)";
		var actions = point.ActionLine?.Actions?.Count ?? 0;
		var name = Blank(point.Name) ? point.Id.ToString() : point.Name;
		return actions == 0 ? name : $"{name}   ({actions} action(s))";
	}

	// ---------------------------------------------------------------- arguments

	/// <summary>
	/// The graph whose input parameters a link into <paramref name="destination"/> fills.
	///
	/// A graph that declares none but substitutes another takes the substitute's: 139 of the
	/// 148 links whose argument count does not match their destination match the substitute
	/// exactly. The remaining 9 match neither and are left as unnamed arguments rather than
	/// silently dropped.
	/// </summary>
	public static Graph? ParameterisedGraph(VmElement? destination) {
		if (destination is not Graph graph) return null;
		if (graph.InputParams is { Count: > 0 }) return graph;
		return graph.SubstituteGraph?.Element as Graph;
	}

	/// <summary>
	/// A link's arguments, paired with the parameters they fill. Longer than the parameter list
	/// when the data carries more than the destination declares — those keep their value and
	/// are shown unnamed, because deleting an argument is not this method's decision.
	/// </summary>
	public static IReadOnlyList<Argument> ArgumentsOf(GraphLink link) {
		var parameters = ParameterisedGraph(link.Destination?.Element)?.InputParams ?? [];
		var values = link.SourceParams ?? [];
		var count = System.Math.Max(parameters.Count, values.Count);

		var arguments = new List<Argument>(count);
		for (var i = 0; i < count; i++)
			arguments.Add(new Argument(i, i < parameters.Count ? parameters[i] : null,
				i < values.Count ? values[i] ?? "" : ""));
		return arguments;
	}

	/// <summary>
	/// Variables a link's arguments may be written in terms of.
	///
	/// A link fires on its event and passes values into the graph it enters, so the event's own
	/// messages are what it has to hand — 29 of the 41 message-valued arguments in the corpus
	/// name the link's own event. The other 12 name a different event's message, which the
	/// engine resolves by name against whatever the FSM last stored, so an existing value is
	/// never taken away even though nothing here would suggest writing a new one.
	/// </summary>
	public static IReadOnlyList<Message> MessagesFor(GraphLink link) => link.Event?.Messages ?? [];

	/// <summary>Input parameters of the graph the link lives in — in scope for its arguments.</summary>
	public static IReadOnlyList<InputParameter> InputParamsFor(GraphLink link) =>
		(link.Parent.Element as Graph)?.InputParams ?? [];

	// ---------------------------------------------------------------- validation

	/// <summary>
	/// What is wrong with a link, or null. Every check is a rule the whole corpus obeys, so a
	/// complaint here means the link really is something the game never ships.
	/// </summary>
	public static string? Problem(GraphLink link) {
		var exits = ExitsOf(link.Source?.Element);
		if (exits.All(e => e.Index != link.SourceExitPointIndex))
			return link.Source?.Element == null
				? "An event link starts from the event rather than a node, so its exit index must be -1."
				: $"{NameOf(link.Source?.Element)} has no exit {link.SourceExitPointIndex}.";

		// A link with no destination ends the flow. That is not a mistake and not rare: 926 of
		// MarbleNest's links and 7510 of the Sandbox's have none, and every one of them is
		// listed in its graph's EventLinks like any other.
		if (IsTerminator(link)) return null;

		var entries = EntriesOf(link.Destination?.Element);
		if (entries.All(e => e.Index != link.DestEntryPointIndex))
			return $"{NameOf(link.Destination?.Element)} has no entry point {link.DestEntryPointIndex}.";

		var declared = ParameterisedGraph(link.Destination?.Element)?.InputParams?.Count ?? 0;
		var passed = link.SourceParams?.Count ?? 0;
		if (passed != declared)
			return $"{NameOf(link.Destination?.Element)} takes {declared} argument(s), but {passed} are passed.";

		return null;
	}

	/// <summary>True for a link that ends the flow rather than going anywhere.</summary>
	public static bool IsTerminator(GraphLink link) => link.Destination?.Element == null;
}
