using System.Collections.Generic;
using System.IO;
using P2XMLEditor.GameData.VirtualMachineElements.Enums;
using P2XMLEditor.Logging;
using P2XMLEditor.Parsing.RawData;
using static P2XMLEditor.Parsing.Helpers.RawPointerExtensions;

namespace P2XMLEditor.Parsing.Element.RawPointerParsers;

// ASSUMPTIONS:
// * Only known ConditionOperation enum values.
// * Predicates counts stays within 1-99 range.
// * Order index stays within 1-99 range.
// * Name may have both non-ASCII characters (cyrillic) and special symbols ( < > )
public class RawPointerConditionLoader : IParser<RawConditionData> {

	[PerformanceLogHook]
	public unsafe void ProcessFile(string filePath, List<RawConditionData> raws) {
		var data = File.ReadAllBytes(filePath);
		var n = data.Length;

		fixed (byte* ptr = data) {
			var p = ptr + 105;
			var end = ptr + n - 10;
			while (p + 78 < end) {
				p += 12;
				var id = ParseUlong16(p);
				p += 16;

				p += 27;
				var predicatesCount = ParseCount2(p, out var digitCount);
				var predicates = new ulong[predicatesCount];
				p += digitCount + 16;
				for (var i = 0; i < predicatesCount; i++) {
					predicates[i] = ParseUlong16(p);
					p += 16 + 21;
				}
				p += 11;
				
				p += 11;
				var operation = ParseOperation(ref p);
				p += 18;

				string name;
				p += 5;
				if (*p == (byte)' ') {
					name = string.Empty;
				} else {
					p++;
					name = ParseStringUtf8SpecialSymbols(ref p);
					p += 4;
				}
				p += 2;

				p += 19;
				var op = ParseInt2(p, out digitCount);
				p += digitCount;
				p += 26;


				raws.Add(new RawConditionData
					{ Id = id, PredicateIds = predicates, Operation = operation, Name = name, OrderIndex = op });
			}
		}
	}
	
	private static unsafe ConditionOperation ParseOperation(ref byte* p) {
		var start = p;
		while (*p != '<')
			p++;

		var len = (int)(p - start);

		return len switch {
			8 => ConditionOperation.Root,
			6 => ConditionOperation.Or,
			_ => start[4] switch {
				(byte)'A' => ConditionOperation.And,
				_		 => ConditionOperation.Xor
			}
		};
	}
}