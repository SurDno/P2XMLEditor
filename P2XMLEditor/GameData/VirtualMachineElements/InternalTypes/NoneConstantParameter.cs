namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes;

public class NoneConstantParameter : ConstantParameter {
	public override string ParamId => "none";
	public override object? GetValue() => null;
}
