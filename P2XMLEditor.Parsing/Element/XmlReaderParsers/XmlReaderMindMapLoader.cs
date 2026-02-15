using P2XMLEditor.Enums;
using P2XMLEditor.Enums.VirtualMachine;

using P2XMLEditor.Parsing.RawData;
using static P2XMLEditor.Parsing.Helpers.XmlReaderExtensions;

namespace P2XMLEditor.Parsing.Element.XmlReaderParsers;

public class XmlReaderMindMapLoader : IParser<RawMindMapData> {
	public void ProcessFile(string filePath, List<RawMindMapData> raws) {
		using var xr = InitializeFullFileReader(filePath);
		xr.SkipDeclarationAndRoot();

		while (xr.Read()) {
			if (xr.EndOfContainerReached()) break;

			var raw = new RawMindMapData {
				Id = xr.GetIdAndEnter(),
				NodeIds = xr.GetULongListAndAdvance(),
				LinkIds = xr.Name == "Links" ? xr.GetULongListAndAdvance() : null,
				LogicMapType = xr.GetStringValueAndAdvance().Deserialize<LogicMapType>(),
				TitleId = xr.GetULongValueAndAdvance(),
				Name = xr.GetStringValueAndAdvance(),
				ParentId = xr.GetULongValueAndAdvance()
			};

			raws.Add(raw);
		}
	}
}