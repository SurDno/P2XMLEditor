using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;

namespace P2XMLEditor.Helper;

public static class VmElementExtensions {
    private static VmElement GetElementById(VirtualMachine vm, ulong id) {
        vm.ElementsById.TryGetValue(id, out var el);
        if (el is null)
            throw new KeyNotFoundException($"Element {id} was not found");
        return el;
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

    public static T GetElement<T>(this VirtualMachine vm, ulong id) {
        ValidateVmElementType<T>();
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

        if (el is null)
            return default;


        return new(el);
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
    
    public static IEnumerable<T> GetElementsByType<T>(this VirtualMachine vm) where T : VmElement {
        return vm.ElementsByType.TryGetValue(typeof(T), out var elements) ? elements.Cast<T>() : [];
    }
}