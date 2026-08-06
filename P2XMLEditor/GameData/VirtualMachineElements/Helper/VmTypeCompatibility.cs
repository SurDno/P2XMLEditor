using System.Collections.Generic;
using P2XMLEditor.GameData.Enums;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
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
		if (IsObjectFamily(expected) && IsObjectFamily(declared) &&
			(IsGenericObjectRef(expected) || IsGenericObjectRef(declared)))
			return true;

		// Several VmTypes are distinct declarations of one runtime type; those are the same
		// slot as far as a value is concerned.
		var expectedSystem = VmTypeHelper.GetSystemType(expected.BaseType);
		var declaredSystem = VmTypeHelper.GetSystemType(declared.BaseType);
		return expectedSystem != null && expectedSystem == declaredSystem;
	}

	/// <summary>
	/// One world object under several declarations: IObjRef, IEntity, and the narrower
	/// ICharacterRef / ISceneRef.
	/// </summary>
	private static bool IsObjectFamily(VmTypeInfo type) {
		if (type.BaseType == VmType.EntityRef) return true;
		var systemType = VmTypeHelper.GetSystemType(type.BaseType);
		return systemType != null && typeof(GameObject).IsAssignableFrom(systemType);
	}

	/// <summary>
	/// The two declarations that name a world object without saying which kind: IObjRef, the
	/// general reference, and IEntity, the same object named as the engine entity it is placed
	/// as. Either fits against any specific object reference, and the shipped data relies on
	/// it — Position.GetRegion returns IEntity and is compared against IObjRef%cf_Region
	/// parameters and against hierarchy paths thirteen times across the two corpora.
	///
	/// Two *specific* references still have to agree, which is why this is a separate test:
	/// ICharacterRef against ISceneRef is a mistake, not a widening. Blueprint references are
	/// outside the family entirely — a blueprint is the template, not the object.
	/// </summary>
	private static bool IsGenericObjectRef(VmTypeInfo type) =>
		type.BaseType is VmType.EntityRef or VmType.GameObject;

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

	/// <summary>
	/// Whether a value of the declared type demonstrably fits the slot.
	///
	/// The difference from <see cref="Accepts(VmTypeInfo?, VmTypeInfo?)"/> is what happens when
	/// the declared type cannot be worked out: Accepts lets it through, because it judges values
	/// already stored in the data and hiding one would be worse than showing a doubtful one.
	/// This one refuses, because it builds the lists the user picks from, and offering a
	/// candidate that cannot be shown to fit is how the wrong type gets chosen in the first
	/// place. An unknown *expected* type still filters nothing — there is no slot type to
	/// disagree with.
	/// </summary>
	public static bool Matches(VmTypeInfo? expected, VmTypeInfo? declared) {
		if (expected == null || expected.BaseType == VmType.Unknown) return true;
		if (declared == null || declared.BaseType == VmType.Unknown) return false;
		return Accepts(expected, declared);
	}

	public static bool Matches(VmTypeInfo? expected, string? declaredXmlType, Core.VirtualMachine vm) {
		if (expected == null || expected.BaseType == VmType.Unknown) return true;
		if (string.IsNullOrEmpty(declaredXmlType)) return false;
		try {
			return Matches(expected, VmTypeHelper.GetVmTypeInfo(declaredXmlType, vm));
		} catch {
			return false;
		}
	}

	/// <summary>
	/// True when a parameter's declared type really is an object reference. Unlike
	/// <see cref="Accepts(VmTypeInfo?, string?, Core.VirtualMachine)"/> this does not give an
	/// unresolvable type the benefit of the doubt: it backs the pickers that offer "the object
	/// held by this parameter", where anything that is not an IObjRef cannot hold one.
	/// </summary>
	public static bool IsObjectValued(string? declaredXmlType, Core.VirtualMachine vm) {
		if (string.IsNullOrEmpty(declaredXmlType)) return false;
		try {
			return VmTypeHelper.GetVmTypeInfo(declaredXmlType, vm).BaseType == VmType.GameObject;
		} catch {
			return false;
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
	/// <summary>
	/// Types whose value is written out rather than referenced.
	///
	/// TypeValue is one of them: a VMType slot takes a type name, optionally narrowed by
	/// functional component — "IObjRef%cf_Building", or "IObjRef%cf_Common&amp;Gate" for a
	/// conjunction. That is a value the engine reads whole, not a reference to anything, so it
	/// belongs here; without it a VMType slot offers no way to fill it at all.
	/// </summary>
	public static bool IsLiteralLike(VmTypeInfo? type) =>
		type == null || type.BaseType is VmType.Unknown or VmType.Boolean or VmType.Int32 or VmType.Single
			or VmType.String or VmType.UInt64 or VmType.GameTime or VmType.TypeValue || EnumTypeOf(type) != null;

	public static System.Type? EnumTypeOf(VmTypeInfo? type) {
		if (type == null) return null;
		var systemType = VmTypeHelper.GetSystemType(type.BaseType);
		return systemType is { IsEnum: true } ? systemType : null;
	}
}
