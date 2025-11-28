using System.Collections.Generic;
using P2XMLEditor.Logging;
using P2XMLEditor.Parsing.RawData;
using static P2XMLEditor.Helper.XmlReaderExtensions;

namespace P2XMLEditor.Parsing.Element.XmlReaderParsers;

public class XmlReaderPartConditionLoader : IParser<RawPartConditionData> {
	[PerformanceLogHook]
	public void ProcessFile(string filePath, List<RawPartConditionData> raws) {
		using var xr = InitializeFullFileReader(filePath);
		xr.SkipDeclarationAndRoot();

		while (xr.Read()) {
			if (xr.EndOfContainerReached()) break;

			var raw = new RawPartConditionData {
				Id = xr.GetIdAndEnter(),
				Name = xr.GetOptionalStringValueAndAdvance(),
				ConditionType = xr.GetStringValueAndAdvance(),
				FirstExpressionId = xr.Name == "FirstExpression" ? xr.GetULongValueAndAdvance() : null,
				SecondExpressionId = xr.Name == "SecondExpression" ? xr.GetULongValueAndAdvance() : null,
				OrderIndex = xr.GetIntValueAndAdvance()
			};

			raws.Add(raw);
		}
	}
}