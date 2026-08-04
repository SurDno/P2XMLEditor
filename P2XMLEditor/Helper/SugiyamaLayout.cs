using System;
using System.Collections.Generic;
using System.Linq;

namespace P2XMLEditor.Helper;

public class SugiyamaLayout<T> where T : notnull {
	public class Node(T data) {
		public readonly T Data = data;
		public readonly List<Node> Out = [];
		public readonly List<Node> In = [];
		public int Layer;
		public int Order;
		public float X;
		public float Y;
	}

	private readonly Dictionary<T, Node> _nodes = new();

	public void AddNode(T data) {
		if (!_nodes.ContainsKey(data)) {
			_nodes[data] = new Node(data);
		}
	}

	/// <summary>
	/// Records an edge, ignoring one whose ends are not both nodes here. A graph's links can
	/// name something outside it — a placeholder standing in for an id the data never defines,
	/// or a node belonging to another graph — and indexing straight into the dictionary turned
	/// that into a KeyNotFoundException at load.
	/// </summary>
	public void AddEdge(T from, T to) {
		if (!_nodes.TryGetValue(from, out var a) || !_nodes.TryGetValue(to, out var b)) return;
		a.Out.Add(b);
		b.In.Add(a);
	}

	public Dictionary<T, (float x, float y)> Layout(
		float horizontalSpacing,
		float verticalSpacing)
	{
		AssignLayers();
		MinimizeCrossings();
		AssignCoordinates(horizontalSpacing, verticalSpacing);

		return _nodes.Values.ToDictionary(n => n.Data, n => (n.X, n.Y));
	}

	/// <summary>
	/// Longest-path layering, over the graph with its cycles broken.
	///
	/// The previous version relaxed layers from a queue with no visited set, which does not
	/// terminate on a cyclic graph: every trip round a loop pushes its members again with a
	/// higher layer, and the queue grows until its backing array cannot. A state machine is
	/// cyclic by nature — a state that can return to an earlier one is the normal case — so this
	/// was not an edge case; the Cathedral graph in PathologicSandbox, 31 nodes and 46 links,
	/// took it out with an OutOfMemoryException.
	///
	/// Cycles are broken by a depth-first pass: an edge back to a node still on the stack is a
	/// back edge and is left out of the layering, which is the usual Sugiyama step. It still
	/// draws — the canvas draws every link regardless — it just does not get a say in what is
	/// upstream of what, because in a cycle nothing is.
	/// </summary>
	private void AssignLayers() {
		var backEdges = FindBackEdges();

		// Kahn's algorithm over the remaining edges: a node's layer is one past the last of its
		// predecessors, and every node is settled exactly once.
		var remaining = _nodes.Values.ToDictionary(n => n,
			n => n.In.Count(parent => !backEdges.Contains((parent, n))));

		var ready = new Queue<Node>(remaining.Where(pair => pair.Value == 0).Select(pair => pair.Key));

		// A component that is one whole cycle has no zero-indegree node even after the back edge
		// is cut, if the cut edge was the only way in. Seeding with the lowest id keeps the
		// result stable rather than dependent on dictionary order.
		if (ready.Count == 0 && _nodes.Count > 0)
			ready.Enqueue(_nodes.Values.OrderBy(n => n.Data.ToString(), StringComparer.Ordinal).First());

		var settled = new HashSet<Node>();
		while (ready.Count > 0) {
			var node = ready.Dequeue();
			if (!settled.Add(node)) continue;

			foreach (var child in node.Out) {
				if (backEdges.Contains((node, child))) continue;
				child.Layer = Math.Max(child.Layer, node.Layer + 1);
				if (--remaining[child] <= 0 && !settled.Contains(child)) ready.Enqueue(child);
			}
		}

		// Anything the walk never reached — a cycle behind a cut edge — is placed after whatever
		// does reach it, so it is at least not on top of its own predecessors.
		foreach (var node in _nodes.Values) {
			if (settled.Contains(node)) continue;
			var upstream = node.In.Where(settled.Contains).Select(n => n.Layer).DefaultIfEmpty(-1).Max();
			node.Layer = upstream + 1;
		}
	}

	/// <summary>
	/// Edges that close a cycle, found by an iterative depth-first walk. Iterative because a
	/// graph here can be deeper than the stack is willing to go, and because the recursion is
	/// the part that would fail silently.
	/// </summary>
	private HashSet<(Node From, Node To)> FindBackEdges() {
		var backEdges = new HashSet<(Node, Node)>();
		var visited = new HashSet<Node>();
		var onStack = new HashSet<Node>();

		foreach (var root in _nodes.Values.OrderByDescending(n => n.In.Count == 0)) {
			if (visited.Contains(root)) continue;

			var stack = new Stack<(Node Node, int Index)>();
			stack.Push((root, 0));
			visited.Add(root);
			onStack.Add(root);

			while (stack.Count > 0) {
				var (node, index) = stack.Pop();
				if (index >= node.Out.Count) {
					onStack.Remove(node);
					continue;
				}

				stack.Push((node, index + 1));
				var child = node.Out[index];

				if (onStack.Contains(child)) {
					backEdges.Add((node, child));
				} else if (visited.Add(child)) {
					onStack.Add(child);
					stack.Push((child, 0));
				}
			}
		}

		return backEdges;
	}

	private void MinimizeCrossings() {
		var layers = _nodes.Values
			.GroupBy(n => n.Layer)
			.OrderBy(g => g.Key)
			.Select(g => g.ToList())
			.ToList();

		foreach (var layer in layers) {
			for (var i = 0; i < layer.Count; i++) {
				layer[i].Order = i;
			}
		}

		for (var iter = 0; iter < 4; iter++) {
			for (var i = 1; i < layers.Count; i++) 
				OrderLayer(layers[i], true);
			
			for (var i = layers.Count - 2; i >= 0; i--) 
				OrderLayer(layers[i], false);
			
		}
	}

	private void OrderLayer(List<Node> layer, bool useIncoming) {
		var barycenters = new Dictionary<Node, double>();

		foreach (var node in layer) {
			var neighbors = useIncoming ? node.In : node.Out;
			if (neighbors.Count == 0) {
				barycenters[node] = node.Order;
			} else {
				barycenters[node] = neighbors.Average(n => n.Order);
			}
		}

		layer.Sort((a, b) =>
			barycenters[a].CompareTo(barycenters[b]));

		for (var i = 0; i < layer.Count; i++) {
			layer[i].Order = i;
		}
	}

	private void AssignCoordinates(
		float horizontalSpacing,
		float verticalSpacing)
	{
		var layers = _nodes.Values.GroupBy(n => n.Layer);

		foreach (var layer in layers) {
			foreach (var node in layer) {
				node.X = node.Order * horizontalSpacing;
				node.Y = -node.Layer * verticalSpacing;
			}
		}
	}
}
