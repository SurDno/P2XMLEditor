using System.Diagnostics.CodeAnalysis;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;

namespace P2XMLEditor.Core;

public readonly struct VmEither<T1, T2>(VmElement element) where T1 : VmElement where T2 : VmElement {
    public ulong Id => Element.Id;
    public VmElement Element { get; } = element;

    public static implicit operator VmEither<T1, T2>(T1 value) => new(value);
    public static implicit operator VmEither<T1, T2>(T2 value) => new(value);
}

[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public readonly struct VmEither<T1, T2, T3> where T1 : VmElement where T2 : VmElement where T3 : VmElement {
    public VmEither(VmElement element) {
        Element = element;
    }

    public ulong Id => Element.Id;
    public VmElement Element { get; }

    public static implicit operator VmEither<T1, T2, T3>(T1 value) => new(value);
    public static implicit operator VmEither<T1, T2, T3>(T2 value) => new(value);
    public static implicit operator VmEither<T1, T2, T3>(T3 value) => new(value);
}

[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public readonly struct VmEither<T1, T2, T3, T4> where T1 : VmElement where T2 : VmElement where T3 : VmElement 
    where T4 : VmElement {
    public VmEither(VmElement element) {
        Element = element;
    }

    public ulong Id => Element.Id;
    public VmElement Element { get; }

    public static implicit operator VmEither<T1, T2, T3, T4>(T1 value) => new(value);
    public static implicit operator VmEither<T1, T2, T3, T4>(T2 value) => new(value);
    public static implicit operator VmEither<T1, T2, T3, T4>(T3 value) => new(value);
    public static implicit operator VmEither<T1, T2, T3, T4>(T4 value) => new(value);
}

[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public readonly struct VmEither<T1, T2, T3, T4, T5> where T1 : VmElement where T2 : VmElement where T3 : VmElement 
    where T4 : VmElement where T5 : VmElement {
    public VmEither(VmElement element) {
        Element = element;
    }

    public ulong Id => Element.Id;
    public VmElement Element { get; }

    public static implicit operator VmEither<T1, T2, T3, T4, T5>(T1 value) => new(value);
    public static implicit operator VmEither<T1, T2, T3, T4, T5>(T2 value) => new(value);
    public static implicit operator VmEither<T1, T2, T3, T4, T5>(T3 value) => new(value);
    public static implicit operator VmEither<T1, T2, T3, T4, T5>(T4 value) => new(value);
    public static implicit operator VmEither<T1, T2, T3, T4, T5>(T5 value) => new(value);
}

[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public readonly struct VmEither<T1, T2, T3, T4, T5, T6> where T1 : VmElement where T2 : VmElement where T3 : VmElement 
    where T4 : VmElement where T5 : VmElement where T6 : VmElement {
    public VmEither(VmElement element) {
        Element = element;
    }

    public ulong Id => Element.Id;
    public VmElement Element { get; }

    public static implicit operator VmEither<T1, T2, T3, T4, T5, T6>(T1 value) => new(value);
    public static implicit operator VmEither<T1, T2, T3, T4, T5, T6>(T2 value) => new(value);
    public static implicit operator VmEither<T1, T2, T3, T4, T5, T6>(T3 value) => new(value);
    public static implicit operator VmEither<T1, T2, T3, T4, T5, T6>(T4 value) => new(value);
    public static implicit operator VmEither<T1, T2, T3, T4, T5, T6>(T5 value) => new(value);
    public static implicit operator VmEither<T1, T2, T3, T4, T5, T6>(T6 value) => new(value);
}

[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public readonly struct VmEither<T1, T2, T3, T4, T5, T6, T7> where T1 : VmElement where T2 : VmElement where T3 : VmElement 
    where T4 : VmElement where T5 : VmElement where T6 : VmElement where T7 : VmElement {
    public VmEither(VmElement element) {
        Element = element;
    }

    public ulong Id => Element.Id;
    public VmElement Element { get; }

    public static implicit operator VmEither<T1, T2, T3, T4, T5, T6, T7>(T1 value) => new(value);
    public static implicit operator VmEither<T1, T2, T3, T4, T5, T6, T7>(T2 value) => new(value);
    public static implicit operator VmEither<T1, T2, T3, T4, T5, T6, T7>(T3 value) => new(value);
    public static implicit operator VmEither<T1, T2, T3, T4, T5, T6, T7>(T4 value) => new(value);
    public static implicit operator VmEither<T1, T2, T3, T4, T5, T6, T7>(T5 value) => new(value);
    public static implicit operator VmEither<T1, T2, T3, T4, T5, T6, T7>(T6 value) => new(value);
    public static implicit operator VmEither<T1, T2, T3, T4, T5, T6, T7>(T7 value) => new(value);
}