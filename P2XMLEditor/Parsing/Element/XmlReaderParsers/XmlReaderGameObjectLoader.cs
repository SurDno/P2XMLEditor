using P2XMLEditor.Helper;
using P2XMLEditor.Logging;
using P2XMLEditor.Parsing.RawData;
using static P2XMLEditor.Helper.XmlReaderExtensions;

namespace P2XMLEditor.Parsing.Element.XmlReaderParsers;

public class XmlReaderGameObjectLoader : IParser<RawGameObjectData> {
	[PerformanceLogHook]
	public void ProcessFile(string filePath, List<RawGameObjectData> raws) {
		using var xr = InitializeFullFileReader(filePath);
		xr.SkipDeclarationAndRoot();

		while (xr.Read()) {
			if (xr.EndOfContainerReached()) break;

			var raw = new RawGameObjectData {
				Id = xr.GetIdAndEnter(),
				WorldPositionGuid = xr.Name == "WorldPositionGuid" ? xr.GetOptionalStringValueAndAdvance() : null,
				EngineTemplateId = xr.Name == "EngineTemplateID" ? xr.GetStringValueAndAdvance() : null,
				EngineBaseTemplateId = xr.Name == "EngineBaseTemplateID" ? xr.GetStringValueAndAdvance() : null,
				Instantiated = xr.Name == "Instantiated" ? xr.GetBoolValueAndAdvance() : null,
				Static = xr.Name == "Static" ? xr.GetBoolValueAndAdvance() : null,
				InheritanceInfo = xr.Name == "InheritanceInfo" ? xr.GetStringListAndAdvance() : null,
				FunctionalComponentIds = xr.Name == "FunctionalComponents" ? xr.GetULongListAndAdvance() : null,
				EventGraphId = xr.Name == "EventGraph" ? xr.GetULongValueAndAdvance() : null,
				ChildObjectIds = xr.Name == "ChildObjects" ? xr.GetULongListAndAdvance() : null,
				EventIds = xr.Name == "Events" ? xr.GetULongListAndAdvance() : null,
				CustomParamIds = xr.Name == "CustomParams" ? xr.GetULongDictAndAdvance() : null,
				StandartParamIds = xr.Name == "StandartParams" ? xr.GetULongDictAndAdvance() : null,
				GameTimeContext = xr.Name == "GameTimeContext" ? xr.GetStringValueAndAdvance() : null,
				Name = xr.GetStringValueAndAdvance(),
				ParentId = xr.GetULongValueAndAdvance()
			};

			raws.Add(raw);
		}
	}
}