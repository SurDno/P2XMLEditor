using System.Collections.Generic;
using P2XMLEditor.Logging;
using P2XMLEditor.Parsing.RawData;
using static P2XMLEditor.Helper.XmlReaderExtensions;

namespace P2XMLEditor.Parsing.Element.XmlReaderParsers;

public class XmlReaderParameterLoader : IParser<RawParameterData> {
	[PerformanceLogHook]
	public void ProcessFile(string filePath, List<RawParameterData> raws) {
		using var xr = InitializeFullFileReader(filePath, 65536);
		xr.SkipDeclarationAndRoot();

		while (xr.Read()) {
			if (xr.EndOfContainerReached()) break;

			var raw = new RawParameterData {
				Id = xr.GetIdAndEnter(),
				Name = xr.GetOptionalStringValueAndAdvance(),
				OwnerComponentId = xr.Name == "OwnerComponent" ? xr.GetULongValueAndAdvance() : null,
				Type = xr.GetStringValueAndAdvance(),
				Value = xr.GetOptionalStringValueAndAdvance(),
				Implicit = xr.Name == "Implicit" ? xr.GetBoolValueAndAdvance() : null,
				ParentId = xr.GetULongValueAndAdvance(),
				Custom = xr.Name == "Custom" ? xr.GetBoolValueAndAdvance() : null
			};

			raws.Add(raw);
		}
	}
}