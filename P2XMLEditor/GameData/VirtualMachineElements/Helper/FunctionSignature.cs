using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.Enums;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;
using P2XMLEditor.Helper;

namespace P2XMLEditor.GameData.VirtualMachineElements.Helper;

/// <summary>
/// The declared shape of a <see cref="VmFunction"/> — one named, typed slot per parameter —
/// so an editor can render the right control per slot instead of a list of raw strings.
///
/// The 247 function classes are generated with one <c>FunctionSourceParam&lt;T&gt;</c>
/// property per parameter, declared in call order and written back by GetParamStrings in
/// that same order; this was checked to hold for every one of them, and
/// <see cref="Of"/> still verifies the count against ParamCount before trusting it.
/// </summary>
public sealed class FunctionSignature {
	public sealed record Slot(int Index, string Name, VmTypeInfo Type);

	public string Name { get; }
	public VmType ReturnType { get; }
	public IReadOnlyList<Slot> Slots { get; }

	public int ParamCount => Slots.Count;

	/// <summary>
	/// True when the call produces nothing to store. 187 of the 246 functions are void, and in
	/// the corpus a void call binds TargetParam 20 times out of 11391 — the engine ignores it
	/// there, so the editor treats a result destination as meaningless for these.
	/// </summary>
	public bool IsVoid => ReturnType is VmType.Void or VmType.Unknown;

	/// <summary>The return type as a slot type, for filtering result destinations.</summary>
	public VmTypeInfo ReturnTypeInfo => new(ReturnType);

	private FunctionSignature(string name, VmType returnType, IReadOnlyList<Slot> slots) {
		Name = name;
		ReturnType = returnType;
		Slots = slots;
	}

	/// <summary>Every function name that can be selected, "" (the placeholder) excluded.</summary>
	public static IEnumerable<string> AvailableNames =>
		VmFunction.GetAvailableFunctions().Where(n => !string.IsNullOrEmpty(n)).OrderBy(n => n, StringComparer.Ordinal);

	/// <summary>
	/// Describes <paramref name="name"/>. Pass the parameter strings already stored on the
	/// action when there are any: a handful of list functions declare a slot as
	/// <c>object</c> and take its real type from a sibling CommonList at construction time,
	/// so the live instance knows something reflection alone cannot.
	/// </summary>
	public static FunctionSignature? Of(string name, VirtualMachine vm, IReadOnlyList<string>? currentParams = null) {
		var type = VmFunction.GetFunctionType(name);
		if (type == null) return null;

		var properties = SlotProperties(type);
		var instance = TryCreate(name, vm, Pad(currentParams, properties.Length));

		// The return type has to be known even when the stored values do not construct, since
		// it decides whether a result destination is offered at all; an all-empty instance is
		// enough to read it off.
		var returnType = instance?.ReturnType
						 ?? TryCreate(name, vm, Pad(null, properties.Length))?.ReturnType
						 ?? VmType.Unknown;

		if (instance != null && instance.ParamCount != properties.Length)
			return Untyped(name, returnType, instance.ParamCount);

		var slots = new List<Slot>(properties.Length);
		for (var i = 0; i < properties.Length; i++)
			slots.Add(new Slot(i, properties[i].Name, SlotType(properties[i], instance)));

		return new FunctionSignature(name, returnType, slots);
	}

	/// <summary>
	/// Function names whose component prefix appears on <paramref name="components"/>.
	/// A function lives on a FunctionalComponent, and calling one the target object does not
	/// have is not a thing the engine can do — verified across PathologicSandbox, where
	/// 10631 of 10632 DoFunction calls with a resolvable target name a component on that
	/// object's own inheritance chain.
	/// </summary>
	public static IEnumerable<string> NamesForComponents(IReadOnlySet<string> components) =>
		AvailableNames.Where(n => components.Contains(ComponentOf(n)));

	public static string ComponentOf(string functionName) {
		var dot = functionName.IndexOf('.');
		return dot <= 0 ? functionName : functionName[..dot];
	}

	/// <summary>
	/// Builds the function from slot values, padding or trimming to the declared arity so a
	/// half-filled editor still produces a constructible function.
	/// </summary>
	public static VmFunction? Create(string name, VirtualMachine vm, IReadOnlyList<string>? paramValues) {
		var type = VmFunction.GetFunctionType(name);
		if (type == null) return null;
		return TryCreate(name, vm, Pad(paramValues, SlotProperties(type).Length));
	}

	private static FunctionSignature Untyped(string name, VmType returnType, int count) =>
		new(name, returnType,
			Enumerable.Range(0, count).Select(i => new Slot(i, $"Param {i + 1}", VmTypeInfo.Unknown)).ToList());

	private static string[] Pad(IReadOnlyList<string>? values, int count) {
		var result = new string[count];
		for (var i = 0; i < count; i++)
			result[i] = values != null && i < values.Count ? values[i] ?? "" : "";
		return result;
	}

	private static VmFunction? TryCreate(string name, VirtualMachine vm, string[] parameters) {
		try {
			return VmFunction.GetFunction(name, vm, parameters);
		} catch {
			// A slot value that no longer resolves must not stop the editor from opening;
			// the form falls back to untyped slots and the user can fix the value.
			return null;
		}
	}

	private static VmTypeInfo SlotType(PropertyInfo property, VmFunction? instance) {
		var declared = property.PropertyType.GetGenericArguments()[0];

		// ObjRef is the editor's own IObjRef wrapper and has no VmToSystemType entry;
		// mapping it here keeps VmTypeHelper — and therefore parsing — untouched.
		if (declared == typeof(ObjRef)) return VmTypeInfo.GameObject;

		var vmType = VmTypeHelper.GetVmType(declared);
		if (vmType != VmType.Unknown) return new VmTypeInfo(vmType);

		// object slots: whatever the constructor worked out from its sibling list.
		return LiveSource(property, instance)?.TypeInfo ?? VmTypeInfo.Unknown;
	}

	private static ParameterSource? LiveSource(PropertyInfo property, VmFunction? instance) {
		if (instance == null) return null;
		var slot = property.GetValue(instance);
		if (slot == null) return null;
		return property.PropertyType.GetProperty(nameof(FunctionSourceParam<object>.Source))?
			.GetValue(slot) as ParameterSource?;
	}

	private static readonly ConcurrentDictionary<Type, PropertyInfo[]> SlotPropertyCache = new();

	private static PropertyInfo[] SlotProperties(Type functionType) =>
		SlotPropertyCache.GetOrAdd(functionType, static t => t
			.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
			.Where(p => p.PropertyType.IsGenericType &&
						p.PropertyType.GetGenericTypeDefinition() == typeof(FunctionSourceParam<>))
			.ToArray());
}
