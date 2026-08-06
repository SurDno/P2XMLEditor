using P2XMLEditor.Core;
using P2XMLEditor.GameData;
using P2XMLEditor.GameData.Enums;
using P2XMLEditor.GameData.VirtualMachineElements.Enums;
using P2XMLEditor.Helper;

namespace P2XMLEditor.GameData.VirtualMachineElements.Helper;

/// <summary>
/// Which pairs of types a condition may actually compare.
///
/// The engine checks a comparison twice, and the two checks disagree often enough that passing
/// one is no guarantee of the other. <c>VMTypeUtility.IsTypesCompatible</c> runs when the map
/// loads and throws a compiler error on failure; <c>IsValueEqual</c> / <c>IsValueLarger</c> run
/// during play. Only a pair that survives both is offered here — something that loads and then
/// silently answers the wrong thing is worse than something that will not load at all.
///
/// The asymmetries are real and are the reason this is a matrix rather than a symmetric
/// "are these the same type" test:
///
/// * An integer on the left and a float on the right FAILS validation, while float-left and
///   integer-right passes. Order matters.
/// * GameTime compares fine with &lt; and &gt;, which read TotalSeconds, but == is reference
///   equality on a class, so two GameTimes holding the same instant come out unequal.
/// * Enum against String passes at runtime and fails to load; IRef against a raw engine object
///   likewise. Both are rejected.
/// * System.Object waves the load-time check through whatever is on the other side.
///
/// Two cases pass both checks and are still traps, so they are allowed with a caveat rather
/// than silently: number comparison casts both sides to a 32-bit float, so two different
/// uint64s above 16777216 read as equal; and list equality is an identity check on the list
/// object, not on its contents.
/// </summary>
public static class ExpressionComparability {
	public enum Verdict {
		/// <summary>Loads and behaves.</summary>
		Fine,

		/// <summary>Loads and runs, but the result is not what it looks like.</summary>
		Caveat,

		/// <summary>Fails to load, or loads and cannot answer correctly.</summary>
		Rejected
	}

	public readonly record struct Result(Verdict Verdict, string? Reason) {
		public bool IsAllowed => Verdict != Verdict.Rejected;
		public static Result Ok => new(Verdict.Fine, null);
		public static Result Warn(string reason) => new(Verdict.Caveat, reason);
		public static Result No(string reason) => new(Verdict.Rejected, reason);
	}

	/// <summary>
	/// Whether <paramref name="left"/> may be compared to <paramref name="right"/> under
	/// <paramref name="comparison"/>. Sides are not interchangeable — pass them as the condition
	/// stores them, First then Second.
	/// </summary>
	public static Result Check(VmTypeInfo? left, VmTypeInfo? right, ConditionType comparison, VirtualMachine vm) {
		// Nothing chosen yet on one side: everything is still open, and saying otherwise would
		// stop the user filling the first side of an empty condition.
		if (left == null || right == null) return Result.Ok;

		// Unknown covers both "not chosen yet" and the engine's System.Object wildcard, which
		// waves the load-time check through whatever is on the other side. Neither can be ruled
		// on here: the first is not an answer yet, the second answers only at runtime, from the
		// value. There is no separate VmType for the wildcard — the editor reads an untyped
		// value as Unknown, so the two arrive at the same place.
		if (IsUnknown(left) || IsUnknown(right)) return Result.Ok;

		var ordering = comparison is ConditionType.ValueLess or ConditionType.ValueLessEqual
			or ConditionType.ValueLarger or ConditionType.ValueLargerEqual;

		if (IsGameTime(left) || IsGameTime(right)) return CheckGameTime(left, right, ordering);
		if (IsNumber(left) || IsNumber(right)) return CheckNumbers(left, right);
		if (IsBoolean(left) || IsBoolean(right)) return CheckBoolean(left, right, ordering);
		if (IsEnum(left) || IsEnum(right)) return CheckEnums(left, right);
		if (IsString(left) || IsString(right)) return CheckStrings(left, right);
		if (IsList(left) || IsList(right)) return CheckLists(left, right, vm);
		if (IsReference(left) || IsReference(right)) return CheckReferences(left, right, ordering, vm);

		return VmTypeCompatibility.Matches(left, right)
			? Result.Ok
			: Result.No($"{Name(left)} and {Name(right)} are unrelated types");
	}

	private static Result CheckGameTime(VmTypeInfo left, VmTypeInfo right, bool ordering) {
		if (!IsGameTime(left) || !IsGameTime(right))
			// The runtime happily reads TotalSeconds against a number, but IsTypesCompatible
			// refuses the pair, so the map never loads to find out.
			return Result.No("game time can only be compared to game time — against a number it "
							 + "runs but fails to load");

		return ordering
			? Result.Ok
			: Result.No("game time equality compares object identity, not the instant, so two "
						+ "equal times read as different — compare with < or > instead");
	}

	private static Result CheckNumbers(VmTypeInfo left, VmTypeInfo right) {
		if (!IsNumber(left) || !IsNumber(right))
			return Result.No($"{Name(left)} and {Name(right)} are not both numbers");

		// The engine's own check is (!firstType.IsIntegerNumber || !secondType.IsFloat) — an
		// integer on the left with a float on the right is the one ordering it rejects.
		if (IsInteger(left) && IsFloat(right))
			return Result.No("an integer on the left of a float fails the load-time check — "
							 + "swap the sides, which the engine does accept");

		return IsWideInteger(left) || IsWideInteger(right)
			? Result.Warn("numbers are compared as 32-bit floats, so values above 16777216 lose "
						  + "precision and unequal ones can read as equal")
			: Result.Ok;
	}

	private static Result CheckBoolean(VmTypeInfo left, VmTypeInfo right, bool ordering) {
		if (!IsBoolean(left) || !IsBoolean(right))
			return Result.No($"a boolean can only be compared to a boolean, not to {Name(IsBoolean(left) ? right : left)}");
		return ordering ? Result.No("booleans have no order — compare them for equality") : Result.Ok;
	}

	private static Result CheckEnums(VmTypeInfo left, VmTypeInfo right) {
		if (!IsEnum(left) || !IsEnum(right))
			// Enum against string is the classic one: the runtime converts and compares happily,
			// but the map will not load.
			return Result.No("an enum can only be compared to the same enum — against a string it "
							 + "runs but fails to load");

		return VmTypeCompatibility.EnumTypeOf(left) == VmTypeCompatibility.EnumTypeOf(right)
			? Result.Ok
			: Result.No($"{Name(left)} and {Name(right)} are different enums");
	}

	private static Result CheckStrings(VmTypeInfo left, VmTypeInfo right) =>
		IsString(left) && IsString(right)
			? Result.Ok
			: Result.No($"a string can only be compared to a string, not to {Name(IsString(left) ? right : left)}");

	private static Result CheckLists(VmTypeInfo left, VmTypeInfo right, VirtualMachine vm) {
		if (!IsList(left) || !IsList(right))
			return Result.No($"a list can only be compared to a list, not to {Name(IsList(left) ? right : left)}");

		// The load-time check recurses into the element types; the runtime does not look at
		// elements at all.
		var elements = Check(left.UnderlyingType, right.UnderlyingType, ConditionType.ValueEqual, vm);
		if (!elements.IsAllowed) return Result.No($"the lists hold incompatible elements: {elements.Reason}");

		return Result.Warn("lists compare by identity, not by contents — this is true only when "
						   + "both sides are the very same list");
	}

	private static Result CheckReferences(VmTypeInfo left, VmTypeInfo right, bool ordering, VirtualMachine vm) {
		if (!IsReference(left) || !IsReference(right))
			return Result.No($"an object reference can only be compared to another reference, not to "
							 + $"{Name(IsReference(left) ? right : left)}");

		if (ordering) return Result.No("object references have no order — compare them for equality");

		// A specific reference against an untyped one is fine; two specific ones have to agree.
		return VmTypeCompatibility.Matches(left, right) || VmTypeCompatibility.Matches(right, left)
			? Result.Ok
			: Result.No($"{Name(left)} and {Name(right)} are different kinds of reference");
	}

	// ---------------------------------------------------------------- type tests

	private static bool IsUnknown(VmTypeInfo type) => type.BaseType == VmType.Unknown;

	private static bool IsGameTime(VmTypeInfo type) => type.BaseType == VmType.GameTime;

	private static bool IsBoolean(VmTypeInfo type) => type.BaseType == VmType.Boolean;

	private static bool IsString(VmTypeInfo type) => type.BaseType == VmType.String;

	private static bool IsList(VmTypeInfo type) => type.BaseType == VmType.List;

	private static bool IsEnum(VmTypeInfo type) => VmTypeCompatibility.EnumTypeOf(type) != null;

	private static bool IsFloat(VmTypeInfo type) => type.BaseType == VmType.Single;

	/// <summary>
	/// Every integer width the engine lumps into IsIntegerNumber. Enums are excluded: they are
	/// integers underneath but the engine compares them as enums.
	/// </summary>
	private static bool IsInteger(VmTypeInfo type) =>
		type.BaseType is VmType.Int32 or VmType.UInt64;

	private static bool IsNumber(VmTypeInfo type) => IsInteger(type) || IsFloat(type);

	/// <summary>Wide enough that the cast to 32-bit float can lose the value.</summary>
	private static bool IsWideInteger(VmTypeInfo type) => type.BaseType == VmType.UInt64;

	private static bool IsReference(VmTypeInfo type) => VmTypeCompatibility.IsElementLike(type);

	private static string Name(VmTypeInfo type) {
		try {
			return type.Serialize();
		} catch {
			return type.BaseType.ToString();
		}
	}
}
