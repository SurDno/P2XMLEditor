using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using System.Text;

namespace P2XMLEditor.Parsing.Helpers;

public static unsafe class RawPointerExtensions {
    private static readonly Vector128<byte> V16Lt = Vector128.Create((byte)'<');
    private static readonly ushort[] Lut2 = BuildLut2();
    private static readonly Encoding Utf8 = Encoding.UTF8, Ascii = Encoding.ASCII; 
        
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ushort[] BuildLut2() {
        var t = new ushort[100];
        for (var i = 0; i < 100; i++) t[i] = (ushort)i;
        return t;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint ParseUint8(byte* p) {
        return Lut2[(p[6] - 48) * 10 + (p[7] - 48)] +
               Lut2[(p[4] - 48) * 10 + (p[5] - 48)] * 100U +
               Lut2[(p[2] - 48) * 10 + (p[3] - 48)] * 10_000U +
               Lut2[(p[0] - 48) * 10 + (p[1] - 48)] * 1_000_000U;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong ParseUlong15Known7(ulong KPmul, byte* p){
        ulong lo=ParseUint8(p+7);
        return KPmul+lo;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong ParseUlong16Known9(ulong P9mul, byte* p) {
        var q = p + 9;

        ulong a = Lut2[(q[0] - 48) * 10 + (q[1] - 48)]; 
        ulong b = Lut2[(q[2] - 48) * 10 + (q[3] - 48)]; 
        ulong c = Lut2[(q[4] - 48) * 10 + (q[5] - 48)];
        var d = (ulong)(q[6] - 48);

        var v = a * 100000UL + b * 1000UL + c * 10UL + d;

        return P9mul + v;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong ParseUlong16(byte* p) {
        var hi = ParseUint8(p);
        var lo = ParseUint8(p + 8);
        return hi * 100_000_000UL + lo;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong ParseUlong15(byte* p) {
        var hi = (ulong)(p[0] - 48) * 1_000_000UL +
                 (ulong)(p[1] - 48) * 100_000UL +
                 (ulong)(p[2] - 48) * 10_000UL +
                 (ulong)(p[3] - 48) * 1_000UL +
                 (ulong)(p[4] - 48) * 100UL +
                 (ulong)(p[5] - 48) * 10UL +
                 (ulong)(p[6] - 48);

        var lo = ParseUint8(p + 7);

        return hi * 100_000_000UL + lo;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong ParseUlong17(byte* p) {
        var x = ParseUlong16(p);
        return x * 10UL + (ulong)(p[16] - 48);
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool ParseBool(ref byte* p) {
        var result = *p == (byte)'T';
        p += result ? 4 : 5;
        return result;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ParseCount1(byte* p) => *p - '0';
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ParseCount2(byte* p, out int digitCount) {
        var d0 = p[0] - '0';
        if (p[1] != '"') {
            digitCount = 2;
            return d0 * 10 + (p[1] - '0');
        }

        digitCount = 1;
        return d0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ParseCount3(byte* p, out int digitCount) {
        var d0 = p[0] - '0';
        if (p[1] == '"') {
            digitCount = 1;
            return d0;
        }

        var d1 = p[1] - '0';
        if (p[2] == '"') {
            digitCount = 2;
            return d0 * 10 + d1;
        }

        digitCount = 3;
        return d0 * 100 + d1 * 10 + (p[2] - '0');
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ParseInt2(byte* p, out int digitCount) {
        var d0 = p[0] - '0';
        if (p[1] != '<') {
            digitCount = 2;
            return d0 * 10 + (p[1] - '0');
        }

        digitCount = 1;
        return d0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ParseInt3(byte* p, out int digitCount) {
        var d0 = p[0] - '0';
        if (p[1] == '<') {
            digitCount = 1;
            return d0;
        }

        var d1 = p[1] - '0';
        if (p[2] == '<') {
            digitCount = 2;
            return d0 * 10 + d1;
        }

        digitCount = 3;
        return d0 * 100 + d1 * 10 + (p[2] - '0');
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int FindTagContentLength(byte* p) {
        byte* start = p;

        for (;;) {
            var v0 = Sse2.LoadVector128(p);
            int m0 = Sse2.MoveMask(Sse2.CompareEqual(v0, V16Lt));
            if (m0 != 0)
                return (int)(p - start) + BitOperations.TrailingZeroCount(m0);

            var v1 = Sse2.LoadVector128(p + 16);
            int m1 = Sse2.MoveMask(Sse2.CompareEqual(v1, V16Lt));
            if (m1 != 0)
                return (int)(p - start) + 16 + BitOperations.TrailingZeroCount(m1);

            p += 32;
        }
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ParseStringAsciiNoSpecialSymbols(ref byte* p) {
        var start = p;
        var len = FindTagContentLength(p);
        p += len;
        return Ascii.GetString(start, len);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ParseStringAsciiSpecialSymbols(ref byte* p) {
        var start = p;
        var len = FindTagContentLength(p);

        var buf = new char[len];
        var o = 0;

        for (var i = 0; i < len; i++) {
            var b = start[i];

            if (b == '&') {
                switch (start[i + 1]) {
                    case (byte)'l':
                        buf[o++] = '<';
                        i += 3; 
                        continue;
                    case (byte)'g':
                        buf[o++] = '>';
                        i += 3; 
                        continue;
                    case (byte)'a':
                        buf[o++] = '&';
                        i += 4; 
                        continue;
                }
            }

            buf[o++] = (char)b;
        }

        p = start + len;

        return new string(buf, 0, o);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ParseStringUtf8NoSpecialSymbols(ref byte* p) {
        var start = p;
        var len = FindTagContentLength(p);
        p += len;
        return Utf8.GetString(start, len);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ParseStringUtf8SpecialSymbols(ref byte* p) {
        var start = p;
        var len = FindTagContentLength(p);
        Span<byte> tmp = stackalloc byte[len];
        var o = 0;

        for (var i = 0; i < len; i++) {
            var b = start[i];

            if (b == '&') {
                switch (start[i + 1]) {
                    case (byte)'l':
                        tmp[o++] = (byte)'<';
                        i += 3;
                        continue;
                    case (byte)'g':
                        tmp[o++] = (byte)'<';
                        i += 3;
                        continue;
                    case (byte)'a':
                        tmp[o++] = (byte)'&';
                        i += 4;
                        continue;
                }
            }

            tmp[o++] = b;
        }

        p = start + len;
        return Utf8.GetString(tmp[..o]);
    }
}
