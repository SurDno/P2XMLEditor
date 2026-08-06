using System.Collections.Generic;
using P2XMLEditor.GameData.Enums;

namespace P2XMLEditor.GameData.VirtualMachineElements.Helper;

/// <summary>How a slot's value stands for the object it refers to.</summary>
public enum SlotValueForm {
	/// <summary>An id, an engine GUID or a hierarchy — the ordinary object reference.</summary>
	Reference,

	/// <summary>The object's name, written as a plain string.</summary>
	Name
}

/// <summary>
/// What a particular function parameter really accepts, beyond the type its declaration
/// happens to carry.
/// </summary>
/// <param name="Form">Whether the slot names its object or references it.</param>
/// <param name="RequiredComponent">
/// A component the object must have, or None for no restriction.
/// </param>
public sealed record SlotConstraint(SlotValueForm Form, VmComponent RequiredComponent = VmComponent.None);

/// <summary>
/// Per-function narrowing of what an argument accepts.
///
/// The generated function classes only carry a CLR type per parameter, and that type is often
/// wider than the truth: Market.SetMultiStorablesFixedPrices declares its first argument as a
/// string when it is the name of a storable object, and the ByTemplate family declares an
/// EntityRef when only a storable will do. The declaration cannot say so, and the classes are
/// generated, so the extra knowledge lives here rather than in an attribute a regeneration
/// would erase.
///
/// Each entry below is checked against the corpus before being added — see the remarks on the
/// table. Anything absent falls back to the declared type, so this can be filled in as more
/// functions are pinned down.
/// </summary>
public static class FunctionSlotOverrides {
	/// <summary>
	/// Keyed by function name and the slot's declared property name.
	///
	/// Storable-by-name: all 1109 uses of Market.SetMultiStorablesFixedPrices in
	/// PathologicSandbox pass an object name, and 1071 of those name an object carrying the
	/// Storable component. The remaining 38 pass "Pathologic", which is a real object without
	/// that component — so the list guides without being a hard gate, and the control stays
	/// editable.
	///
	/// Storable-by-reference: every one of the 381 ByTemplate calls whose Template argument is
	/// written as a plain object id points at an object carrying Storable; the rest are engine
	/// GUIDs for the same kinds of object.
	/// </summary>
	private static readonly Dictionary<(string Function, string Slot), SlotConstraint> Overrides = new() {
		[("Market.SetMultiStorablesFixedPrices", "StorageGroup")] = new(SlotValueForm.Name, VmComponent.Storable),
		[("Market.SetMultiStorablesPricesFactor", "StorageGroup")] = new(SlotValueForm.Name, VmComponent.Storable),

		[("Storage.PickUpByTemplate", "Template")] = new(SlotValueForm.Reference, VmComponent.Storable),
		[("Storage.PickUpByTemplate_v1", "Template")] = new(SlotValueForm.Reference, VmComponent.Storable),
		[("Storage.PickUpToInentoryByTemplate", "Template")] = new(SlotValueForm.Reference, VmComponent.Storable),
		[("Storage.AddItemOrDropByTemplate", "Template")] = new(SlotValueForm.Reference, VmComponent.Storable)
	};

	public static SlotConstraint? For(string functionName, string slotName) =>
		Overrides.GetValueOrDefault((functionName, slotName));
}
