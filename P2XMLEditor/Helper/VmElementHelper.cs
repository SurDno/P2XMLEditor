using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.GameData.VirtualMachineElements.Interfaces;
using P2XMLEditor.GameData.VirtualMachineElements.Placeholders;

namespace P2XMLEditor.Helper;

public static class VmElementExtensions {
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static VmElement GetElementById(VirtualMachine vm, ulong id) {
		return vm.ElementsById[id];
	}

	private static void ValidateVmElementType<T>() {
		if (!typeof(T).IsInterface && !typeof(T).IsAssignableTo(typeof(VmElement)))
			throw new ArgumentException("T is not a VmElement");
	}

	private static string BuildTypeErrorMessage(ulong Id, Type actualType, params Type[] expectedTypes) {
		var expectedTypeNames = string.Join(" or ", expectedTypes.Select(t => t.Name));
		return $"Element {Id} is a {actualType.Name} instead of {expectedTypeNames}";
	}
	
	

	public static VmElement GetElement(this VirtualMachine vm, ulong id) {

		return GetElementById(vm, id);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static T GetElement<T>(this VirtualMachine vm, ulong id) where T: VmElement {
		return (T)GetElementById(vm, id);
	}
	
	public static T GetElementInterface<T>(this VirtualMachine vm, ulong id) {
		var el = GetElementById(vm, id);

		if (el is not T value)
			throw new ArgumentException(BuildTypeErrorMessage(id, el.GetType(), typeof(T)));
		return value;
	}

	public static VmEither<T1, T2> GetElement<T1, T2>(this VirtualMachine vm, ulong id)
		where T1 : VmElement
		where T2 : VmElement {
		var el = GetElementById(vm, id);

		return new(el);
	}

	public static VmEither<T1, T2, T3> GetElement<T1, T2, T3>(this VirtualMachine vm, ulong id)
		where T1 : VmElement
		where T2 : VmElement
		where T3 : VmElement {
		var el = GetElementById(vm, id);

		return new(el);
	}

	public static VmEither<T1, T2, T3, T4> GetElement<T1, T2, T3, T4>(this VirtualMachine vm, ulong id)
		where T1 : VmElement
		where T2 : VmElement
		where T3 : VmElement
		where T4 : VmElement {
		var el = GetElementById(vm, id);

		return new(el);
	}

	public static VmEither<T1, T2, T3, T4, T5> GetElement<T1, T2, T3, T4, T5>(this VirtualMachine vm, ulong id)
		where T1 : VmElement
		where T2 : VmElement
		where T3 : VmElement
		where T4 : VmElement
		where T5 : VmElement {
		var el = GetElementById(vm, id);

		return new(el);
	}

	public static VmEither<T1, T2, T3, T4, T5, T6> GetElement<T1, T2, T3, T4, T5, T6>(this VirtualMachine vm, ulong id)
		where T1 : VmElement
		where T2 : VmElement
		where T3 : VmElement
		where T4 : VmElement
		where T5 : VmElement
		where T6 : VmElement {
		var el = GetElementById(vm, id);

		return new(el);
	}

	public static VmEither<T1, T2, T3, T4, T5, T6, T7> GetElement<T1, T2, T3, T4, T5, T6, T7>(this VirtualMachine vm, ulong id)
		where T1 : VmElement
		where T2 : VmElement
		where T3 : VmElement
		where T4 : VmElement
		where T5 : VmElement
		where T6 : VmElement 
		where T7 : VmElement {
		var el = GetElementById(vm, id);

		return new(el);
	}

	/// <summary>
	/// Every parameter holder in the machine, whatever its concrete type.
	///
	/// GetElementsByType reads buckets the reader fills by hand, one Add call per type per
	/// element, so a subclass added later reaches the ParameterHolder bucket only if someone
	/// remembers to write that line. A selector that must not miss a kind of object scans
	/// instead — one pass, and it cannot fall out of date. Placeholders are excluded: they
	/// stand in for elements the data references but does not define.
	/// </summary>
	public static IReadOnlyList<ParameterHolder> AllParameterHolders(this VirtualMachine vm) {
		var holders = new List<ParameterHolder>();
		foreach (var element in vm.AllElements())
			if (element is ParameterHolder holder and not IPlaceholder)
				holders.Add(holder);
		return holders;
	}

	/// <summary>
	/// Every element, as a snapshot.
	///
	/// Deliberately not a lazy view over ElementsById. Reading a declared type can *write* to
	/// the machine — VmTypeHelper registers a placeholder for an "IObjRef%cf_&lt;id&gt;" whose
	/// blueprint is missing from the data — so any caller that filters these by type would be
	/// mutating the dictionary it is walking, and the enumerator throws. Callers building a
	/// candidate list do exactly that, so the copy is taken once, here.
	/// </summary>
	public static IReadOnlyList<VmElement> AllElements(this VirtualMachine vm) =>
		vm.ElementsById.Values.ToList();

	public static VmElement? GetNullableElement(this VirtualMachine vm, ulong id) {
		vm.ElementsById.TryGetValue(id, out var el);

		return el;
	}
	
	public static T? GetNullableElement<T>(this VirtualMachine vm, ulong id) {
		ValidateVmElementType<T>();
		vm.ElementsById.TryGetValue(id, out var el);

		if (el is null)
			return default;
		if (el is not T value)
			throw new ArgumentException(BuildTypeErrorMessage(id, el.GetType(), typeof(T)));
		return value;
	}
	
		public static VmEither<T1, T2>? GetNullableElement<T1, T2>(this VirtualMachine vm, ulong id) where T1 : VmElement where T2 : VmElement {
		vm.ElementsById.TryGetValue(id, out var value);
		if (value == null) {
			return null;
		}
		return new VmEither<T1, T2>(value);
	}
	public static VmEither<T1, T2, T3>? GetNullableElement<T1, T2, T3>(this VirtualMachine vm, ulong id) where T1 : VmElement where T2 : VmElement where T3 : VmElement {
		vm.ElementsById.TryGetValue(id, out var value);
		if (value == null) {
			return null;
		}
		return new VmEither<T1, T2, T3>(value);
	}
	public static VmEither<T1, T2, T3, T4>? GetNullableElement<T1, T2, T3, T4>(this VirtualMachine vm, ulong id)
		where T1 : VmElement
		where T2 : VmElement
		where T3 : VmElement
		where T4 : VmElement {
		vm.ElementsById.TryGetValue(id, out var el);

		if (el is null)
			return default;

		return new(el);
	}

	public static VmEither<T1, T2, T3, T4, T5>? GetNullableElement<T1, T2, T3, T4, T5>(this VirtualMachine vm, ulong id) 
		where T1 : VmElement
		where T2 : VmElement
		where T3 : VmElement
		where T4 : VmElement
		where T5 : VmElement {
		vm.ElementsById.TryGetValue(id, out var el);
		
		return el != null ? new(el) : null;
	}

	
	public static VmEither<T1, T2, T3, T4, T5, T6>? GetNullableElement<T1, T2, T3, T4, T5, T6>(this VirtualMachine vm, ulong id) 
		where T1 : VmElement
		where T2 : VmElement
		where T3 : VmElement
		where T4 : VmElement
		where T5 : VmElement
		where T6 : VmElement{
		vm.ElementsById.TryGetValue(id, out var el);

		if (el is null)
			return default;


		return new(el);
	}
}
