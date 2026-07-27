namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes;

public class BoolConstantParameter(bool value) : ConstantParameter {
	public bool Value { get; set; } = value;
	public override string ParamId {
		get {
			if (!Value) {
			return "False";
		}
			return "True";
		}
	}
	public override object GetValue() => Value;
}
