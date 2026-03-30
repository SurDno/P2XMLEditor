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

	public void AddEdge(T from, T to) {
		var a = _nodes[from];
		var b = _nodes[to];
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

	private void AssignLayers() {
		var queue = new Queue<Node>(
			_nodes.Values.Where(n => n.In.Count == 0));

		while (queue.Count > 0) {
			var node = queue.Dequeue();
			foreach (var child in node.Out) {
				child.Layer = Math.Max(child.Layer, node.Layer + 1);
				queue.Enqueue(child);
			}
		}
	}

	private void MinimizeCrossings() {
		var layers = _nodes.Values
			.GroupBy(n => n.Layer)
			.OrderBy(g => g.Key)
			.Select(g => g.ToList())
			.ToList();

		foreach (var layer in layers) {
			for (int i = 0; i < layer.Count; i++) {
				layer[i].Order = i;
			}
		}

		for (int iter = 0; iter < 4; iter++) {
			for (int i = 1; i < layers.Count; i++) 
				OrderLayer(layers[i], true);
			
			for (int i = layers.Count - 2; i >= 0; i--) 
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

		for (int i = 0; i < layer.Count; i++) {
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