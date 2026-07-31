using System.Collections.Generic;
using P2XMLEditor.GameData.Enums;
using P2XMLEditor.Helper;

namespace P2XMLEditor.GameData.VirtualMachineElements.Helper;

/// <summary>
/// Whether a value of one declared type may fill a slot of another.
///
/// This is what stops the editor offering an object reference for a System.Boolean. It is
/// deliberately a little looser than exact equality, because the data is: the Sample family
/// (ISampleRef, IModel, ILipSyncObject, …) is one runtime type behind several declarations,
/// and IBlueprintRef and IBlueprintRefStorable are used interchangeably. Anything it cannot
/// prove — an Unknown on either side, a by-name lookup resolved at runtime — is allowed
/// through, so the filter never blocks an edit it merely fails to understand.
/// </summary>
public static class VmTypeCompatibility {
	private static readonly HashSet<VmType> Numeric = [VmType.Int32, VmType.Single, VmType.UInt64];
	private static readonly HashSet<VmType> Blueprint = [VmType.BlueprintRef, VmType.BlueprintRefStorable];

	public static bool Accepts(VmTypeInfo? expected, VmTypeInfo? declared) {
		if (expected == null || declared == null) return true;
		if (expected.BaseType == VmType.Unknown || declared.BaseType == VmType.Unknown) return true;

		if (expected.BaseType == declared.BaseType)
			return expected.BaseType != VmType.List || ElementTypesAgree(expected, declared);

		if (Numeric.Contains(expected.BaseType) && Numeric.Contains(declared.BaseType)) return true;
		if (Blueprint.Contains(expected.BaseType) && Blueprint.Contains(declared.BaseType)) return true;

		// Several VmTypes are distinct declarations of one runtime type; those are the same
		// slot as far as a value is concerned.
		var expectedSystem = VmTypeHelper.GetSystemType(expected.BaseType);
		var declaredSystem = VmTypeHelper.GetSystemType(declared.BaseType);
		return expectedSystem != null && expectedSystem == declaredSystem;
	}

	/// <summary>An untyped list fits any list; two typed lists have to agree on the element.</summary>
	private static bool ElementTypesAgree(VmTypeInfo expected, VmTypeInfo declared) =>
		expected.UnderlyingType == null || declared.UnderlyingType == null ||
		Accepts(expected.UnderlyingType, declared.UnderlyingType);

	/// <summary>Convenience for the many places holding an xml type string rather than a VmTypeInfo.</summary>
	public static bool Accepts(VmTypeInfo? expected, string? declaredXmlType, Core.VirtualMachine vm) {
		if (expected == null || string.IsNullOrEmpty(declaredXmlType)) return true;
		try {
			return Accepts(expected, VmTypeHelper.GetVmTypeInfo(declaredXmlType, vm));
		} catch {
			return true;
		}
	}

	/// <summary>True for slots that hold a reference to a VM element rather than a value.</summary>
	public static bool IsElementLike(VmTypeInfo? type) {
		if (type == null) return false;
		if (type.BaseType is VmType.GameObject or VmType.EntityRef or VmType.BlueprintRef
			or VmType.BlueprintRefStorable)
			return true;
		var systemType = VmTypeHelper.GetSystemType(type.BaseType);
		return systemType != null && typeof(Abstract.VmElement).IsAssignableFrom(systemType);
	}

	/// <summary>True for slots whose value is typed in, rather than chosen from the data.</summary>
	public static bool IsLiteralLike(VmTypeInfo? type) =>
		type == null || type.BaseType is VmType.Unknown or VmType.Boolean or VmType.Int32 or VmType.Single
			or VmType.String or VmType.UInt64 or VmType.GameTime || EnumTypeOf(type) != null;

	public static System.Type? EnumTypeOf(VmTypeInfo? type) {
		if (type == null) return null;
		var systemType = VmTypeHelper.GetSystemType(type.BaseType);
		return systemType is { IsEnum: true } ? systemType : null;
	}
}
