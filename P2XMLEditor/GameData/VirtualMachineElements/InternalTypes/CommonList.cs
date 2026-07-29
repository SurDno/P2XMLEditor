using P2XMLEditor.Core;
using P2XMLEditor.GameData.Enums;
using P2XMLEditor.Helper;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes;

public class CommonList {
	public static VmTypeInfo? GetElementType(FunctionSourceParam<CommonList>? list, VirtualMachine vm) {
		if (list == null) return null;

		var source = list.Source;
		var xmlType = source.ParameterReference?.Type;
		if (string.IsNullOrEmpty(xmlType)) xmlType = source.LiteralValue?.XmlType;
		if (string.IsNullOrEmpty(xmlType)) return null;

		var info = VmTypeHelper.GetVmTypeInfo(xmlType, vm);
		return info.BaseType == VmType.List ? info.UnderlyingType : null;
	}
}
