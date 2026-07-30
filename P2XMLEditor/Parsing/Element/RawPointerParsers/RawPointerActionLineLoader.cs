using System.Collections.Generic;
using System.IO;
using P2XMLEditor.GameData.VirtualMachineElements.Enums;
using P2XMLEditor.Logging;
using P2XMLEditor.Parsing.Element;
using P2XMLEditor.Parsing.RawData;
using static P2XMLEditor.Parsing.Helpers.RawPointerExtensions;

public class RawPointerActionLineLoader : IParser<RawActionLineData> {
	[PerformanceLogHook]
	public unsafe void ProcessFile(string filePath, List<RawActionLineData> raws) {
		var data = File.ReadAllBytes(filePath);
		var n = data.Length;

		fixed (byte* ptr = data) {
			var p = ptr + 108;
			var end = ptr + n - 50;
			while (p < end) {
				p += 10;
				var id = ParseUlong16(p);
				p += 31;
				
				
				ulong[] actionIds = null;
				if (*p == (byte)'s') {
					p += 9;
					var count = ParseCount3(p, out var digitCount);
					actionIds = new ulong[count];
					p += digitCount;
					for (var i = 0; i < count; i++) {
						p += 16;
						actionIds[i] = ParseUlong16(p);
						p += 21;
					}

					p += 31;
				} 

				p += 28;
				ActionLineType actionLineType;
				string? loopInfoName = null;
				string? loopInfoStart = null; 
				string? loopInfoEnd = null;
				bool loopInfoRandom = false;
				
				switch (*p) {
					case (byte)'M':
						actionLineType = ActionLineType.Common;
						p += 4 + 28;
						break;
					case (byte)'S':
						actionLineType = ActionLineType.CustomGroup;
						p += 10 + 28;
						break;
					case (byte)'O':
						actionLineType = ActionLineType.Loop;
						p += 55;
						loopInfoName = ParseStringUtf8SpecialSymbols(ref p);
						p += 22;
						loopInfoStart = ParseStringUtf8SpecialSymbols(ref p);
						p += 21;
						loopInfoEnd = ParseStringUtf8SpecialSymbols(ref p);
						p += 21;
						if (*p == (byte)'>') {
							loopInfoRandom = ParseBool(ref p);
							p += 44;
						} else
							p += 20;
						break;
					case (byte)'V':
						actionLineType = ActionLineType.Inventory;
						p += 7 + 28;
						break;
					case (byte)'R':
						actionLineType = ActionLineType.Market;
						p += 4 + 28;
						break;
					default:
						actionLineType = ActionLineType.GateSystem;
						p += 9 + 28;
						break;
				}

				var name = string.Empty;
				if (*p == (byte)'>') {
					p++;
					name = ParseStringUtf8NoSpecialSymbols(ref p);
					p += 4;
				} 
				p += 23;
				

				var localContextId = ParseUlong16(p);
				p += 16 + 33;

				var orderIndex = p[0] - '0';
				p += 29;
				
				raws.Add(new RawActionLineData {
					Id = id,
					ActionIds = actionIds,
					ActionLineType = actionLineType,
					LoopInfoName = loopInfoName,
					LoopInfoEnd = loopInfoEnd,
					LoopInfoStart = loopInfoStart,
					LoopInfoRandom = loopInfoRandom,
					Name = name,
					LocalContextId = localContextId,
					OrderIndex = orderIndex
				});
			}
		}
	}
}
