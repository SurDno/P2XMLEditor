using System.Globalization;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes;

public class FloatConstantParameter(float value, bool usesComma) : ConstantParameter {
	public float Value { get; set; } = value;
	public bool UsesComma { get; set; } = usesComma;
	public override string ParamId {
		get {
			var text = Value.ToString(CultureInfo.InvariantCulture);
			if (!UsesComma) {
			return text;
		}
			return text.Replace('.', ',');
		}
	}
	public override object GetValue() => Value;
}
