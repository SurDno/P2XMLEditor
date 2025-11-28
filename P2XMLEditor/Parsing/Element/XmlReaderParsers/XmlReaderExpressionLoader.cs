using System.Collections.Generic;
using P2XMLEditor.Logging;
using P2XMLEditor.Parsing.RawData;
using static P2XMLEditor.Helper.XmlReaderExtensions;

namespace P2XMLEditor.Parsing.Element.XmlReaderParsers;

public class XmlReaderExpressionLoader : IParser<RawExpressionData> {
	[PerformanceLogHook]
	public void ProcessFile(string filePath, List<RawExpressionData> raws) {
		using var xr = InitializeFullFileReader(filePath);
		xr.SkipDeclarationAndRoot();

		while (xr.Read()) {
			if (xr.EndOfContainerReached()) break;

			var raw = new RawExpressionData {
				Id = xr.GetIdAndEnter(),
				ExpressionType = xr.GetStringValueAndAdvance(),
				TargetFunctionName = xr.GetOptionalStringValueAndAdvance(),
				TargetObject = xr.GetStringValueAndAdvance(),
				TargetParam = xr.Name == "TargetParam" ? xr.GetStringValueAndAdvance() : null,
				SourceParams = xr.Name == "SourceParams" ? xr.GetStringListAndAdvance() : null,
				ConstId = xr.Name == "Const" ? xr.GetULongValueAndAdvance() : null,
				LocalContextId = xr.GetULongValueAndAdvance(),
				FormulaChilds = xr.Name == "FormulaChilds" ? xr.GetULongListAndAdvance() : null,
				FormulaOperations = xr.Name == "FormulaOperations" ? xr.GetStringListAndAdvance() : null,
				Inversion = xr.Name == "Inversion" ? xr.GetBoolValueAndAdvance() : null
			};
			raws.Add(raw);
		}
	}
}