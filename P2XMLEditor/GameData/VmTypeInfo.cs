using P2XMLEditor.GameData.Enums;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.Helper;

namespace P2XMLEditor.GameData;

public class VmTypeInfo(VmType baseType) {
	public VmComponentMask RequiredComponents;
	public VmType BaseType { get; set; } = baseType;
	public VmTypeInfo? UnderlyingType { get; set; }
	public ParameterHolder? ObjBlueprint { get; set; }
	
	public static VmTypeInfo Unknown => new(VmType.Unknown);
	public static VmTypeInfo Int32 => new(VmType.Int32);
	public static VmTypeInfo Single => new(VmType.Single);
	public static VmTypeInfo Boolean => new(VmType.Boolean);
	public static VmTypeInfo String => new(VmType.String);
	public static VmTypeInfo GameObject => new(VmType.GameObject);
	
	public static implicit operator VmTypeInfo(VmType type) => new(type);
	
	public string Serialize() => VmTypeHelper.ToXmlType(this);
	
	public override string ToString() => Serialize();
}
