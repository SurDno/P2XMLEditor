using System.Collections.Generic;
using P2XMLEditor.Logging;
using P2XMLEditor.Parsing.RawData;
using static P2XMLEditor.Helper.XmlReaderExtensions;

namespace P2XMLEditor.Parsing.Element.XmlReaderParsers;

public class XmlReaderTalkingLoader : IParser<RawTalkingData> {
	[PerformanceLogHook]
	public void ProcessFile(string filePath, List<RawTalkingData> raws) {
		using var xr = InitializeFullFileReader(filePath);
		xr.SkipDeclarationAndRoot();

		while (xr.Read()) {
			if (xr.EndOfContainerReached()) break;

			var raw = new RawTalkingData {
				Id = xr.GetIdAndEnter(),
				StateIds = xr.GetULongListAndAdvance(),
				EventLinkIds = xr.GetULongListAndAdvance()
			};
			xr.SkipFilledElement();
			raw.EntryPointIds = xr.GetULongListAndAdvance();
			raw.IgnoreBlock = xr.Name == "IgnoreBlock" ? xr.GetBoolValueAndAdvance() : null;
			raw.OwnerId = xr.GetULongValueAndAdvance();
			raw.Initial = xr.Name == "Initial" ? xr.GetBoolValueAndAdvance() : null;
			raw.Name = xr.GetOptionalStringValueAndAdvance();
			raw.ParentId = xr.GetULongValueAndAdvance();

			raws.Add(raw);
		}
	}
}