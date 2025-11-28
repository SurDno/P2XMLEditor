using System;
using System.Linq;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using ZLinq;

namespace P2XMLEditor.Core;

public readonly struct VmEither<T1, T2> where T1 : VmElement where T2 : VmElement {
    public VmEither(T1 value) => Element = value;
    public VmEither(T2 value) => Element = value;
    public VmEither(VmElement element) {
        Element = element;
    }

    public ulong Id => Element.Id;
    public VmElement Element { get; }

    public static implicit operator VmEither<T1, T2>(T1 value) => new(value);
    public static implicit operator VmEither<T1, T2>(T2 value) => new(value);
}

public readonly struct VmEither<T1, T2, T3> where T1 : VmElement where T2 : VmElement where T3 : VmElement {
    public VmEither(T1 value) => Element = value;
    public VmEither(T2 value) => Element = value;
    public VmEither(T3 value) => Element = value;
    public VmEither(VmElement element) {
        Element = element;
    }

    public ulong Id => Element.Id;
    public VmElement Element { get; }

    public static implicit operator VmEither<T1, T2, T3>(T1 value) => new(value);
    public static implicit operator VmEither<T1, T2, T3>(T2 value) => new(value);
    public static implicit operator VmEither<T1, T2, T3>(T3 value) => new(value);
}

public readonly struct VmEither<T1, T2, T3, T4> where T1 : VmElement where T2 : VmElement where T3 : VmElement 
    where T4 : VmElement {
    public VmEither(T1 value) => Element = value;
    public VmEither(T2 value) => Element = value;
    public VmEither(T3 value) => Element = value;
    public VmEither(T4 value) => Element = value;
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

public readonly struct VmEither<T1, T2, T3, T4, T5> where T1 : VmElement where T2 : VmElement where T3 : VmElement 
    where T4 : VmElement where T5 : VmElement {
    public VmEither(T1 value) => Element = value;
    public VmEither(T2 value) => Element = value;
    public VmEither(T3 value) => Element = value;
    public VmEither(T4 value) => Element = value;
    public VmEither(T5 value) => Element = value;
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

public readonly struct VmEither<T1, T2, T3, T4, T5, T6> where T1 : VmElement where T2 : VmElement where T3 : VmElement 
    where T4 : VmElement where T5 : VmElement where T6 : VmElement {
    public VmEither(T1 value) => Element = value;
    public VmEither(T2 value) => Element = value;
    public VmEither(T3 value) => Element = value;
    public VmEither(T4 value) => Element = value;
    public VmEither(T5 value) => Element = value;
    public VmEither(T6 value) => Element = value;
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

public readonly struct VmEither<T1, T2, T3, T4, T5, T6, T7> where T1 : VmElement where T2 : VmElement where T3 : VmElement 
    where T4 : VmElement where T5 : VmElement where T6 : VmElement where T7 : VmElement {
    public VmEither(T1 value) => Element = value;
    public VmEither(T2 value) => Element = value;
    public VmEither(T3 value) => Element = value;
    public VmEither(T4 value) => Element = value;
    public VmEither(T5 value) => Element = value;
    public VmEither(T6 value) => Element = value;
    public VmEither(T7 value) => Element = value;
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