using System.Collections.Generic;
using P2XMLEditor.GameData.VirtualMachineElements.Enums;
using P2XMLEditor.Helper;
using P2XMLEditor.Logging;
using P2XMLEditor.Parsing.RawData;
using static P2XMLEditor.Parsing.Helpers.XmlReaderExtensions;

namespace P2XMLEditor.Parsing.Element.XmlReaderParsers;

public class XmlReaderActionLoader : IParser<RawActionData> {

	[PerformanceLogHook]
	public void ProcessFile(string filePath, List<RawActionData> raws) {
		using var xr = InitializeFullFileReader(filePath);
		xr.SkipDeclarationAndRoot();

		while (xr.Read()) {
			if (xr.EndOfContainerReached()) break;

			var raw = new RawActionData {
				Id = xr.GetIdAndEnter(),
				ActionType = xr.Name == "ActionType" ? xr.GetStringValueAndAdvance().Deserialize<ActionType>() : ActionType.None,
				MathOperationType = xr.Name == "MathOperationType" ? xr.GetStringValueAndAdvance().Deserialize<MathOperationType>() : MathOperationType.None,
				TargetFuncName = xr.GetOptionalStringValueAndAdvance(),
				SourceExpressionId = xr.Name == "SourceExpression" ? xr.GetULongValueAndAdvance() : null,
				SourceConstId = xr.Name == "SourceConst" ? xr.GetULongValueAndAdvance() : null,
				TargetObject = xr.GetStringValueAndAdvance(),
				TargetParam = xr.GetStringValueAndAdvance(),
				SourceParams = xr.Name == "SourceParams" ? xr.GetStringListAndAdvance() : null,
				Name = xr.GetOptionalStringValueAndAdvance(),
				LocalContextId = xr.GetULongValueAndAdvance(),
				OrderIndex = xr.Name == "OrderIndex" ? xr.GetIntValueAndAdvance() : 0
			};
			raws.Add(raw);
			
		}
	}
}
