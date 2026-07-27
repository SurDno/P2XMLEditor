namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes;

public class StringConstantParameter(string value) : ConstantParameter {
	public string Value { get; set; } = value;
	public override string ParamId => Value;
	public override object GetValue() => Value;
}
