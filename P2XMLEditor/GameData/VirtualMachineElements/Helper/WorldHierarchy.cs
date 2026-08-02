using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.GameData.VirtualMachineElements.Enums;
using P2XMLEditor.Helper;

namespace P2XMLEditor.GameData.VirtualMachineElements.Helper;

/// <summary>
/// Where each object sits in the built world, and which hierarchy guids name it.
///
/// A hierarchy guid is not the object's own parent chain — that is the VM ownership tree and
/// matches no path in the data. It is the path the engine's hierarchy builder produces while
/// descending <see cref="GameRoot.HierarchyScenesStructure"/> from the engine root, and the
/// builder appends an id only when it crosses a <see cref="ChildContainerType.Scenes"/> edge:
///
///   Scenes edge to C        →  guid(C) = guid(N) + [C]        (a scene mounted at N)
///   Childs/SimpleChilds     →  guid(C) = guid(N)[..^1] + [C]  (addressed relative to the mount)
///
/// which is what <c>VMWorldHierarchyObject.Parent</c> does, propagating the mount point rather
/// than itself down to its plain children. Rebuilt against both shipped corpora this reproduces
/// every hierarchy guid in the data — 650/650 in Marble Nest, 5042/5042 in the Sandbox — so a
/// path outside <see cref="IsRegistrable"/> is one <c>GetWorldHierarhyObjectByGuid</c> would
/// never find.
///
/// The same walk answers the other question the pickers need: how many times an object is
/// placed. None means it is a template and is named by bare id; once means it is named by its
/// path; more than once means the id alone cannot say which instance.
/// </summary>
public sealed class WorldHierarchy {
	/// <summary>An object's WorldPositionGuid when it is the root the builder starts from.</summary>
	private const string EngineRootMarker = "18446744073709551615";

	/// <summary>
	/// Depth and total guards. The structure is a DAG — a scene mounted in several places is
	/// one node with several parents — so the walk is bounded by hand rather than by trusting
	/// the data to be shallow. Both shipped corpora sit far under these (max depth 8, 70626
	/// placements), so the caps only ever fire on data that is malformed.
	/// </summary>
	private const int MaxDepth = 16;
	private const int MaxPlacements = 400_000;

	/// <summary>Read in the order the builder descends: a mount is what extends the path.</summary>
	private static readonly ChildContainerType[] ContainerOrder =
		[ChildContainerType.Scenes, ChildContainerType.Childs, ChildContainerType.SimpleChilds];

	private static readonly (ulong Child, ChildContainerType Kind)[] NoChildren = [];
	private static readonly ulong[][] NoPlacements = [];

	private static readonly ConditionalWeakTable<VirtualMachine, WorldHierarchy> Cache = new();

	private readonly Dictionary<ulong, List<(ulong Child, ChildContainerType Kind)>> _children;
	private readonly Dictionary<ulong, List<ulong[]>> _placements = new();
	private readonly HashSet<string> _registrable = [];

	public ulong Root { get; }

	/// <summary>False when the machine carries no hierarchy structure to reason about.</summary>
	public bool IsAvailable { get; }

	/// <summary>True when the walk hit a guard and the index is therefore incomplete.</summary>
	public bool Truncated { get; private set; }

	/// <summary>Every placement in the world.</summary>
	public IReadOnlyList<Placement> AllPlacements { get; }

	/// <summary>One object at one spot in the world: the path that names it there.</summary>
	public readonly record struct Placement(ulong LeafId, ulong[] Path) {
		public string Write() => string.Join("H", Path);
	}

	private WorldHierarchy(VirtualMachine vm) {
		_children = ReadStructure(vm);
		Root = FindRoot(vm, _children);
		IsAvailable = _children.Count > 0 && Root != 0;

		var placements = new List<Placement>();
		if (IsAvailable) Walk(placements);
		AllPlacements = placements;
	}

	public static WorldHierarchy For(VirtualMachine vm) => Cache.GetValue(vm, machine => new WorldHierarchy(machine));

	/// <summary>How many distinct spots in the world this object occupies.</summary>
	public int PlacementCount(ulong id) => _placements.TryGetValue(id, out var paths) ? paths.Count : 0;

	/// <summary>Every path that names this object, or empty when it is never placed.</summary>
	public IReadOnlyList<ulong[]> Placements(ulong id) =>
		_placements.TryGetValue(id, out var paths) ? paths : NoPlacements;

	/// <summary>
	/// The one path naming this object, or null when it is placed nowhere or in several spots —
	/// in which case there is nothing to choose on the user's behalf.
	/// </summary>
	public ulong[]? SolePlacement(ulong id) =>
		_placements.TryGetValue(id, out var paths) && paths.Count == 1 ? paths[0] : null;

	/// <summary>True when the builder produces this path, so the engine can resolve it.</summary>
	public bool IsRegistrable(IEnumerable<ulong> path) => _registrable.Contains(string.Join("H", path));

	/// <summary>
	/// What a bare id means for this object, or null when it raises no question.
	///
	/// Not a validity check — the engine takes a bare id for any object whatsoever.
	/// <c>GuidUtility.GetGuidFormat</c> reads a plain number as GT_BASE before it tries
	/// anything else, and <c>CommonVariable.GetTemplateByGuid</c> then hands back whatever
	/// <c>GetObjectByGuid</c> finds, placed or not. What differs is the answer: a bare id names
	/// the static template, a path names one placement of it.
	///
	/// So for a placed object the two spellings mean different things, and the shipped content
	/// uses both deliberately — all 21 actions naming a singly-placed object by id are
	/// Common.Init on it, and 17 of those objects are targeted by full path elsewhere in the
	/// same corpus. Hence a note rather than a filter.
	/// </summary>
	public string? BareIdNote(ulong id) {
		var count = PlacementCount(id);
		return count switch {
			0 => null,
			1 => "placed once — a bare id names the template, not the placement",
			_ => $"placed {count}× — a bare id names the template, not any one placement"
		};
	}

	/// <summary>Children of a node in the raw structure, mounts first.</summary>
	public IReadOnlyList<(ulong Child, ChildContainerType Kind)> Children(ulong node) =>
		_children.TryGetValue(node, out var kids) ? kids : NoChildren;

	private static Dictionary<ulong, List<(ulong Child, ChildContainerType Kind)>> ReadStructure(VirtualMachine vm) {
		var children = new Dictionary<ulong, List<(ulong Child, ChildContainerType Kind)>>();
		var structure = vm.GetElementsByType<GameRoot>().FirstOrDefault()?.HierarchyScenesStructure;
		if (structure == null) return children;

		foreach (var (node, byKind) in structure) {
			var kids = new List<(ulong Child, ChildContainerType Kind)>();
			foreach (var kind in ContainerOrder)
				if (byKind.TryGetValue(kind, out var ids))
					foreach (var id in ids)
						kids.Add((id, kind));
			if (kids.Count > 0) children[node] = kids;
		}
		return children;
	}

	/// <summary>
	/// The object the builder starts from: the one flagged by WorldPositionGuid = ulong.MaxValue,
	/// which is <c>VMWorldHierarchyObject.IsEngineRoot</c>. Exactly one object carries it in each
	/// shipped corpus. Failing that, the structure node nobody else lists as a child.
	/// </summary>
	private static ulong FindRoot(VirtualMachine vm, Dictionary<ulong, List<(ulong Child, ChildContainerType Kind)>> children) {
		foreach (var element in vm.AllElements())
			if (element is GameObject { WorldPositionGuid: EngineRootMarker })
				return element.Id;

		var listed = new HashSet<ulong>();
		foreach (var kids in children.Values)
			foreach (var kid in kids)
				listed.Add(kid.Child);

		foreach (var node in children.Keys)
			if (!listed.Contains(node))
				return node;
		return 0;
	}

	/// <summary>
	/// Enumerates the builder's states. A state is a node plus the guid it was reached with —
	/// not just a node, because the same scene mounted twice is one node with two guids, which
	/// is the whole reason hierarchy guids exist.
	/// </summary>
	private void Walk(List<Placement> placements) {
		var stack = new Stack<(ulong Node, ulong[] Guid, bool Registrable)>();
		var seen = new HashSet<string>();
		stack.Push((Root, new[] { Root }, true));

		while (stack.Count > 0) {
			var (node, guid, registrable) = stack.Pop();

			var written = string.Join("H", guid);
			// Keyed on the state, not the node: one scene mounted twice is a single node
			// reached with two different guids, and both are real placements.
			if (!seen.Add($"{node}@{written}@{registrable}")) continue;

			if (registrable) {
				// SimpleChilds get an engine instance guid but are never added to
				// worldHierarhyObjectsDict, so a path ending on one resolves to nothing. They
				// stay walkable — a scene can hang below one — but are not offered as targets.
				_registrable.Add(written);
				if (!_placements.TryGetValue(node, out var paths)) {
					paths = [];
					_placements[node] = paths;
				}
				paths.Add(guid);
				placements.Add(new Placement(node, guid));
			}

			if (placements.Count >= MaxPlacements || guid.Length >= MaxDepth) {
				Truncated = true;
				continue;
			}

			foreach (var (child, kind) in Children(node)) {
				// A plain child replaces the last id rather than extending it: it is addressed
				// relative to the mount point its scene hangs from, not to the scene.
				var keep = kind == ChildContainerType.Scenes ? guid.Length : guid.Length - 1;
				var next = new ulong[keep + 1];
				for (var i = 0; i < keep; i++) next[i] = guid[i];
				next[keep] = child;
				stack.Push((child, next, kind != ChildContainerType.SimpleChilds));
			}
		}
	}
}
