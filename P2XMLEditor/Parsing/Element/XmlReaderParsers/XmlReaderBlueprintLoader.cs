using System.Collections.Generic;
using P2XMLEditor.Parsing.RawData;
using static P2XMLEditor.Helper.XmlReaderExtensions;

namespace P2XMLEditor.Parsing.Element.XmlReaderParsers;

public class XmlReaderBlueprintLoader : IParser<RawBlueprintData> {
	public void ProcessFile(string filePath, List<RawBlueprintData> raws) {
		using var xr = InitializeFullFileReader(filePath);
		xr.SkipDeclarationAndRoot();

		while (xr.Read()) {
			if (xr.EndOfContainerReached()) break;
			var raw = new RawBlueprintData {
				Id = xr.GetIdAndEnter(),
				Static = xr.Name == "Static" ? xr.GetBoolValueAndAdvance() : null,
				InheritanceInfo = xr.Name == "InheritanceInfo" ? xr.GetStringListAndAdvance() : null,
				FunctionalComponentIds = xr.GetULongListAndAdvance(),
				EventGraphId = xr.Name == "EventGraph" ? xr.GetULongValueAndAdvance() : null,
				EventIds = xr.Name == "Events" ? xr.GetULongListAndAdvance() : null,
				CustomParamIds = xr.GetULongDictAndAdvance(),
				StandartParamIds = xr.GetULongDictAndAdvance(),
				GameTimeContext = xr.Name == "GameTimeContext" ? xr.GetStringValueAndAdvance() : null,
				Name = xr.GetStringValueAndAdvance(),
				ParentId = xr.Name == "Parent" ? xr.GetULongValueAndAdvance() : null,
				ChildObjectIds = xr.Name == "ChildObjectIds" ? xr.GetULongListAndAdvance() : null
			};
			raws.Add(raw);
		}
	}
}