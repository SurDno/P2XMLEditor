using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace P2XMLEditor.Collections;

public unsafe struct NativeList<T> : IDisposable where T : unmanaged {
    private T* ptr;       
    private T* end;       
    private nuint capacity;

    public NativeList(int initialCapacity) {
        capacity = (nuint)initialCapacity;
        ptr = (T*)NativeMemory.Alloc(capacity, (nuint)sizeof(T));  
        end = ptr;
    }

    public int Length {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => (int)(end - ptr);
    }

    public T* RawPtr {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => ptr;
    }

    public ref T this[int index] {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get {
#if DEBUG
            if ((uint)index >= (uint)Length)
                throw new IndexOutOfRangeException();
#endif
            return ref *(ptr + index);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void EnsureCapacity(nuint needed) {
        if (needed <= capacity) return;

        nuint newCap;

        if (capacity < 256) {
            newCap = capacity * 2;
        } else {
            newCap = capacity + capacity / 2; 
        }

        if (newCap < needed)
            newCap = needed;

        ptr = (T*)NativeMemory.Realloc(ptr,  (nuint)sizeof(T));
        end = ptr + needed;
        capacity = newCap;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(T value) {
        nuint len = (nuint)(end - ptr);
        if (len == capacity) {
            EnsureCapacity(len + 1);
        }
        *end = value;
        end++;
    }

    public Enumerator GetEnumerator() {
        return new Enumerator(ptr, end);
    }

    public void Dispose() {
        if (ptr == null) return;
        NativeMemory.Free(ptr);
        ptr = null;
        end = null;
        capacity = 0;
    }

    public struct Enumerator {
        private T* cur;
        private readonly T* endPtr;
        private T* currentValue;

        public Enumerator(T* start, T* end) {
            cur = start;
            endPtr = end;
            currentValue = null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool MoveNext() {
            if (cur >= endPtr)
                return false;
            currentValue = cur;
            cur++;
            return true;
        }

        public T Current {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => *currentValue;
        }
    }
}
