using System;
using System.Linq;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.GameData.VirtualMachineElements.Interfaces;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes;

/// <summary>
/// A graph input parameter — the editor's equivalent of the engine's InputParam
/// (a ContextVariable owned by the FSM). One instance per declaration, owned by its Graph;
/// references resolve to that same instance rather than to a copy.
///
/// At runtime the engine treats these as flat local variables: FSMGraphManager stores
/// InputParams[i].Name verbatim via AddSubgraphLocalVariable and reads it back by string in
/// GetLocalVariableValue. The graph id inside the name is never parsed out — it is a
/// uniqueness prefix, like "local_&lt;id&gt;_Loop_Index". So Name is written back verbatim and
/// never rebuilt from Graph: the original editor's duplicate-graph copied InputParamsInfo
/// without regenerating names, so a clone's parameters carry the id of the graph they were
/// copied from, and 9 of those originals have since been deleted.
///
/// Cloned graphs therefore own genuinely separate parameters that merely share a name
/// string, which is why the declaring graph is half the identity and resolution goes through
/// the scope. Verified across the corpus: all 156 references resolve to a scope that
/// declares the name they use.
/// </summary>
public class InputParameter : IEquatable<InputParameter> {
	/// <summary>The identifier as written. Emitted verbatim.</summary>
	public string Name { get; }

	public string Type { get; set; }

	/// <summary>Declaring graph — the other half of the identity.</summary>
	public Graph Graph { get; }

	/// <summary>Trailing part, e.g. "TargetShop". Display only — never used for lookup.</summary>
	public string ParamName => Name.Split(["_inputparam_"], StringSplitOptions.None)[^1];

	public string ParamId => Name;

	/// <summary>Deserialization: takes the name verbatim from the data.</summary>
	public InputParameter(string name, string type, Graph graph) {
		Name = name;
		Type = type;
		Graph = graph;
	}

	/// <summary>Authoring: derives a fresh name from the owning graph.</summary>
	public static InputParameter Create(Graph graph, string paramName, string type) =>
		new($"{graph.Id}_inputparam_{paramName}", type, graph);

	/// <param name="scope">
	/// Element the reference lives in. Defaults to the element currently being filled;
	/// pass explicitly when resolving outside a load.
	/// </param>
	public static bool TryParse(string input, VirtualMachine vm, out InputParameter? result, VmElement? scope = null) {
		result = null;
		if (!input.Contains("_inputparam_")) return false;

		// Returns the graph's own instance — a reference and its declaration are the
		// same object, so navigation and rename work without any copy to keep in sync.
		result = OwningGraph(scope ?? vm.FillScope)?
			.InputParams?.FirstOrDefault(p => p.Name == input);
		return result != null;
	}

	/// <summary>
	/// Walks a local context up to its containing Graph. In practice input-param references
	/// only ever sit in a State (103) or a Branch (33), but the Talking/Speech arms cost
	/// nothing and keep the walk total.
	/// </summary>
	private static Graph? OwningGraph(VmElement? scope) {
		for (var guard = 0; guard < 8 && scope != null; guard++) {
			switch (scope) {
				case Graph g: return g;
				case State s: scope = s.Parent; break;
				case Branch b: scope = b.Parent.Element; break;
				case Talking t: scope = t.Parent; break;
				case Speech sp: scope = sp.Parent; break;
				default: return null;
			}
		}
		return null;
	}

	public bool Equals(InputParameter? other) =>
		other != null && Name == other.Name && ReferenceEquals(Graph, other.Graph);

	public override bool Equals(object? obj) => Equals(obj as InputParameter);
	public override int GetHashCode() => HashCode.Combine(Name, Graph.Id);
	public override string ToString() => $"{Name} @ {Graph.Id}";
}
