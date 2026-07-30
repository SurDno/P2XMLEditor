using System.Collections.Generic;
using System.IO;
using P2XMLEditor.GameData.VirtualMachineElements.Enums;
using P2XMLEditor.Logging;
using P2XMLEditor.Parsing.RawData;
using static P2XMLEditor.Parsing.Helpers.RawPointerExtensions;

namespace P2XMLEditor.Parsing.Element.RawPointerParsers;

public class RawPointerActionLoader : IParser<RawActionData>  {
	[PerformanceLogHook]
	public unsafe void ProcessFile(string filePath, List<RawActionData> raws) {
		var data = File.ReadAllBytes(filePath);
		var n = data.Length;

		fixed (byte* ptr = data) {
			var p = ptr + 102;
			var end = ptr + n - 10;
			while (p + 78 < end) {
				p += 12;
				var id = ParseUlong16(p);
				p += 16;
				
				
				p += 32;
				var actionType = ActionType.None;
				switch (*p) {
					case (byte)'N':
						actionType = ActionType.None;
						p += 64;
						break;
					case (byte)'S':
						if (*(p + 4) == (byte)'P') {
							actionType = ActionType.SetParam;
							p += 69;
						} else {
							actionType = ActionType.SetExpression;
							p += 74;
						}
						break;
 					case (byte)'M':
						actionType = ActionType.Math;
						p += 64;
						break;
					case (byte)'D':
						actionType = ActionType.DoFunction;
						p += 71;
						break;
					case (byte)'R':
						actionType = ActionType.RaiseEvent;
						p += 71;
						break;
				}
				
				var mathOperationType = MathOperationType.None;
				switch (*p) {
					case (byte)'N':
						mathOperationType = MathOperationType.None;
						p += 45;
						break;
					case (byte)'A':
						mathOperationType = MathOperationType.Addition;
						p += 50;
						break;
					case (byte)'S':
						mathOperationType = MathOperationType.Subtraction;
						p += 52;
						break;
					case (byte)'M':
						mathOperationType = MathOperationType.Multiply;
						p += 49;
						break;
					case (byte)'D':
						mathOperationType = MathOperationType.Division;
						p += 49;
						break;
				}
				
				var targetFuncName = string.Empty;
				
				if (*p != (byte)' ') {
					p++;
					targetFuncName = ParseStringAsciiNoSpecialSymbols(ref p);
					p += 14;
				}
				p += 16;

				ulong? sourceExpressionId = null;
				ulong? sourceConstId = null;
				if (*p == (byte)'E') {
					p += 11;
					sourceExpressionId = ParseUlong16(p);
					p += 16 + 32;
				} else if (*p == (byte)'C') {
					p += 6;
					sourceConstId = ParseUlong16(p);
					p += 16 + 27;
				}
				
				p += 7;
				var targetObject = ParseStringUtf8NoSpecialSymbols(ref p);
				
				p += 34;
				var targetParam = ParseStringAsciiSpecialSymbols(ref p);

				p += 21;
				string[]? sourceParams;
				if (*p == (byte)'S') {
					p += 20;
					var sourceParamCount = ParseCount1(p);
					sourceParams = new string[sourceParamCount];
					p += 17;
					for (var i = 0; i < sourceParamCount; i++) {
						sourceParams[i] = ParseStringUtf8SpecialSymbols(ref p);
						p += 21;
					}
					p += 14;
				} else {
					sourceParams = [];
				}

				var name = string.Empty;
				p += 4;
				if (*p != (byte)' ') {
					p++;
					name = ParseStringAsciiNoSpecialSymbols(ref p);
					p += 4;
				}
				p += 23;

				var localContenxtId = ParseUlong16(p);
				p += 49;
				
				var index = ParseInt3(p, out var digitCount);
				p += digitCount + 26;

				raws.Add(new RawActionData {
					Id = id, ActionType = actionType, MathOperationType = mathOperationType, 
					TargetFuncName = targetFuncName, SourceExpressionId = sourceExpressionId,
					SourceConstId = sourceConstId, TargetObject = targetObject, TargetParam = targetParam, 
					SourceParams = sourceParams, Name = name, LocalContextId = localContenxtId, OrderIndex = index });
			}
		}
	}
}
