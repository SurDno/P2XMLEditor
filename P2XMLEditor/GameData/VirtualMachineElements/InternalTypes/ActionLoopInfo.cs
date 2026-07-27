namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes;

public class ActionLoopInfo(ParameterSource name, ParameterSource start, ParameterSource end, bool? random) {
	public ParameterSource Name { get; init; } = name;
	public ParameterSource Start { get; init; } = start;
	public ParameterSource End { get; init; } = end;
	public bool? Random { get; set; } = random;
}
