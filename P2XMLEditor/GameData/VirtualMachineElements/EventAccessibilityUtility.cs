using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;

namespace P2XMLEditor.GameData.VirtualMachineElements {
	public static class EventAccessibilityUtility {
		private static readonly ConditionalWeakTable<ParameterHolder, HashSet<Event>> _cache = new();

		public static bool IsEventAccessibleFrom(Event e, ParameterHolder target, VirtualMachine vm) {
			return GetAccessibleEvents(target, vm).Contains(e);
		}

		public static IEnumerable<Event> GetAccessibleEvents(ParameterHolder target, VirtualMachine vm) {
			if (_cache.TryGetValue(target, out var cachedEvents)) {
				return cachedEvents;
			}

			var yieldedEvents = new HashSet<Event>();

			// 1. Gather all events from the target's parent hierarchy
			var current = target;
			while (current != null) {
				if (current.Events != null) {
					foreach (var e in current.Events) {
						yieldedEvents.Add(e);
					}
				}

				if (current.FunctionalComponents != null) {
					foreach (var fc in current.FunctionalComponents) {
						if (fc.Events != null) {
							foreach (var e in fc.Events) {
								yieldedEvents.Add(e);
							}
						}
					}
				}

				current = current.Parent;
			}

			// 2. Gather all events explicitly linked in any graphs owned by 'target'
			if (target.EventGraph != null) {
				var visitedGraphs = new HashSet<Graph>();
				var graphsToVisit = new Queue<Graph>();
				graphsToVisit.Enqueue(target.EventGraph);

				while (graphsToVisit.Count > 0) {
					var g = graphsToVisit.Dequeue();
					if (!visitedGraphs.Add(g)) continue;

					if (g.EventLinks != null) {
						foreach (var link in g.EventLinks) {
							if (link.Event != null) {
								yieldedEvents.Add(link.Event);
							}
						}
					}

					if (g.States != null) {
						foreach (var stateEither in g.States) {
							if (stateEither.Element is Graph childGraph) {
								graphsToVisit.Enqueue(childGraph);
							}
						}
					}
				}
			}

			_cache.Add(target, yieldedEvents);
			return yieldedEvents;
		}
	}
}