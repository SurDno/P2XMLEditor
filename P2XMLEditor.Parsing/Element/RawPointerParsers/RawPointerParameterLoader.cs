using System.Runtime.CompilerServices;
using P2XMLEditor.Parsing.RawData;
using static P2XMLEditor.Parsing.Helpers.RawPointerExtensions;

namespace P2XMLEditor.Parsing.Element.RawPointerParsers;

// ASSUMPTIONS:
// * All parameters have ID of 5629413XXXXXXXX.
// * Name may have non-ASCII symbols (such as cyrillic) but no special symbols ( & < > )
// * Owner components have ID of 788117909XXXXXXX.
// * Type and value may have special symbols (such as &) but no cyrillic.

public class RawPointerParameterLoader : IParser<RawParameterData> {
    public unsafe void ProcessFile(string filePath, List<RawParameterData> raws) {
        var data = File.ReadAllBytes(filePath);
        var n = data.Length;

        fixed (byte* ptr = data) {
            var p = ptr + 105;
            var end = ptr + n - 10;
            while (p + 78 < end) {
                p += 12;
                var id = ParseUlong15Known7(5629413UL * 100_000_000UL, p);
                p += 15;
                p += 9;

                string name;
                p += 4;
                if (*p == (byte)' ') {
                    name = string.Empty;
                } else {
                    p++;
                    name = ParseStringUtf8NoSpecialSymbols(ref p);
                    p += 4;
                }

                p += 10;
                ulong? ownerComponent = null;
                if (*p == (byte)'O') {
                    p += 15;
                    ownerComponent = ParseUlong16Known9(788117909UL * 10_000_000UL, p);
                    p += 16 + 24;
                }

                p += 5;
                var type = ParseStringAsciiSpecialSymbols(ref p);
                p += 19;

                string value;
                if (*p == (byte)' ') {
                    value = string.Empty;
                } else {
                    p++;
                    value = ParseStringAsciiSpecialSymbols(ref p);
                    p += 5;
                }

                p += 19;
                var impl = ParseBool(ref p);
                p += 25;

                var parent = ParseParent(p, out var len);
                p += len;

                p += 23;
                var custom = ParseBool(ref p);
                p += 22;

                raws.Add(new RawParameterData {
                    Id = id, Name = name, OwnerComponentId = ownerComponent, Type = type, Value = value,
                    Implicit = impl, ParentId = parent, Custom = custom
                });
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe ulong ParseParent(byte* p, out int len) {
        switch ((char)p[0]) {
            case '2':
                len = 15;
                return ParseUlong15(p);
            case '1':
                switch ((char)p[1]) {
                    case '1':
                        len = 16;
                        return ParseUlong16(p);
                    default:
                        len = 17;
                        return ParseUlong17(p);
                }
            default:
                len = 16;
                return ParseUlong16(p);
        }
    }
}