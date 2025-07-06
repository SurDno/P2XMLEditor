using P2XMLEditor.GameData.VirtualMachineElements.Abstract;

namespace P2XMLEditor.GameData.VirtualMachineElements.Interfaces;

public interface IGraphElement {
	List<EntryPoint> EntryPoints { get; set; }
	List<GraphLink>? InputLinks { get; set; }
	List<GraphLink>? OutputLinks { get; set; }
	public ParameterHolder Owner { get; set; }
    public string Name { get; set; }
    public bool? IgnoreBlock { get; set; }
    public bool? Initial { get; set; }
}