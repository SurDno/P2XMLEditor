using System.Globalization;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes;

public class IntConstantParameter(int value) : ConstantParameter {
	public int Value { get; set; } = value;
	public override string ParamId => Value.ToString(CultureInfo.InvariantCulture);
	public override object GetValue() => Value;
}
