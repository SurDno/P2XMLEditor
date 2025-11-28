using System.Collections.Generic;
using P2XMLEditor.Logging;
using P2XMLEditor.Parsing.RawData;
using static P2XMLEditor.Helper.XmlReaderExtensions;

namespace P2XMLEditor.Parsing.Element.XmlReaderParsers;

public class XmlReaderQuestLoader : IParser<RawQuestData> {
	[PerformanceLogHook]
	public void ProcessFile(string filePath, List<RawQuestData> raws) {
		using var xr = InitializeFullFileReader(filePath);
		xr.SkipDeclarationAndRoot();

		while (xr.Read()) {
			if (xr.EndOfContainerReached()) break;

			var raw = new RawQuestData {
				Id = xr.GetIdAndEnter(),
				StartEventId = xr.GetULongValueAndAdvance(),
				Static = xr.Name == "Static" ? xr.GetBoolValueAndAdvance() : null,
				InheritanceInfo = xr.Name == "InheritanceInfo" ? xr.GetStringListAndAdvance() : null,
				FunctionalComponentIds = xr.GetULongListAndAdvance(),
				EventGraphId = xr.GetULongValueAndAdvance(),
				ChildObjectIds = xr.Name == "ChildObjects" ? xr.GetULongListAndAdvance() : null,
				EventIds = xr.Name == "Events" ? xr.GetULongListAndAdvance() : null,
				CustomParamIds = xr.GetULongDictAndAdvance(),
				StandartParamIds = xr.GetULongDictAndAdvance(),
				GameTimeContext = xr.Name == "GameTimeContext" ? xr.GetOptionalStringValueAndAdvance() : null,
				Name = xr.GetStringValueAndAdvance(),
				ParentId = xr.GetULongValueAndAdvance()
			};

			raws.Add(raw);
		}
	}
}