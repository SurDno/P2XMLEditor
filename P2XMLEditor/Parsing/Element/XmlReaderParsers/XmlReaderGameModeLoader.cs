using System.Collections.Generic;
using P2XMLEditor.Parsing.RawData;
using static P2XMLEditor.Helper.XmlReaderExtensions;
using static P2XMLEditor.Helper.XmlParsingHelper;

namespace P2XMLEditor.Parsing.Element.XmlReaderParsers;

public class XmlReaderGameModeLoader : IParser<RawGameModeData> {
	public void ProcessFile(string filePath, List<RawGameModeData> raws) {
		using var xr = InitializeFullFileReader(filePath);
		xr.SkipDeclarationAndRoot();

		while (xr.Read()) {
			if (xr.EndOfContainerReached()) break;

			var raw = new RawGameModeData {
				Id = xr.GetIdAndEnter(),
				IsMain = xr.Name == "IsMain" ? xr.GetBoolValueAndAdvance() : null,
				StartGameTime = ParseTimeSpanString(xr.GetStringValueAndAdvance()),
				GameTimeSpeed = xr.GetFloatValueAndAdvance(),
				StartSolarTime = ParseTimeSpanString(xr.GetStringValueAndAdvance()),
				SolarTimeSpeed = xr.GetFloatValueAndAdvance(),
				PlayerRef = xr.GetStringValueAndAdvance(),
				Name = xr.GetStringValueAndAdvance(),
				ParentId = xr.GetULongValueAndAdvance()
			};

			raws.Add(raw);
		}
	}
}