using System.Xml;
using P2XMLEditor.GameData.VirtualMachineElements.Enums;
using P2XMLEditor.Helper;
using P2XMLEditor.Logging;
using P2XMLEditor.Parsing.RawData;
using static P2XMLEditor.Helper.XmlReaderExtensions;

namespace P2XMLEditor.Parsing.ElementParsers.Reader;

public class XmlReaderActionLoader : IParser<RawActionData> {
	[PerformanceLogHook]
	public void ProcessFile(string filePath, List<RawActionData> raws) {
		using var xr = InitializeFullFileReader(filePath);
		xr.SkipDeclarationAndRoot();

		while (xr.Read()) {
			if (xr.EndOfContainerReached()) break;

			var raw = new RawActionData {
				Id = xr.GetIdAndEnter(),
				ActionType = xr.GetStringValueAndAdvance().Deserialize<ActionType>(),
				MathOperationType = xr.GetStringValueAndAdvance().Deserialize<MathOperationType>(),
				TargetFuncName = xr.GetOptionalStringValueAndAdvance(),
				SourceExpressionId = xr.Name == "SourceExpression" ? xr.GetULongValueAndAdvance() : null,
				TargetObject = xr.GetStringValueAndAdvance(),
				TargetParam = xr.GetStringValueAndAdvance(),
				SourceParams = xr.Name == "SourceParams" ? xr.GetStringListAndAdvance() : null,
				Name = xr.GetOptionalStringValueAndAdvance(),
				LocalContextId = xr.GetULongValueAndAdvance(),
				OrderIndex = xr.GetIntValueAndAdvance()
			};
			raws.Add(raw);
			
		}
	}
}