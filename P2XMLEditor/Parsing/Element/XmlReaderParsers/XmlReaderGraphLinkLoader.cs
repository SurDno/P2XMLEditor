using System.Collections.Generic;
using P2XMLEditor.Logging;
using P2XMLEditor.Parsing.RawData;
using static P2XMLEditor.Helper.XmlReaderExtensions;

namespace P2XMLEditor.Parsing.Element.XmlReaderParsers;

public class XmlReaderGraphLinkLoader : IParser<RawGraphLinkData> {
	[PerformanceLogHook]
	public void ProcessFile(string filePath, List<RawGraphLinkData> raws) {
		using var xr = InitializeFullFileReader(filePath);
		xr.SkipDeclarationAndRoot();

		while (xr.Read()) {
			if (xr.EndOfContainerReached()) break;

			var raw = new RawGraphLinkData {
				Id = xr.GetIdAndEnter(),
				EventId = xr.Name == "Event" ? xr.GetULongValueAndAdvance() : null,
				EventObject = xr.GetStringValueAndAdvance(),
				SourceExitPointIndex = xr.GetIntValueAndAdvance(),
				DestEntryPointIndex = xr.GetIntValueAndAdvance(),
				SourceParams = xr.Name == "SourceParams" ? xr.GetStringListAndAdvance() : null,
				SourceId = xr.Name == "Source" ? xr.GetULongValueAndAdvance() : null,
				DestinationId = xr.Name == "Destination" ? xr.GetULongValueAndAdvance() : null,
				Enabled = xr.Name == "Enabled" ? xr.GetBoolValueAndAdvance() : null,
				Name = xr.GetStringValueAndAdvance(),
				ParentId = xr.GetULongValueAndAdvance()
			};

			raws.Add(raw);
		}
	}
}