using System.Xml;
using P2XMLEditor.GameData.VirtualMachineElements.Enums;
using P2XMLEditor.Helper;
using P2XMLEditor.Logging;
using P2XMLEditor.Parsing.RawData;
using static P2XMLEditor.Helper.XmlReaderExtensions;

namespace P2XMLEditor.Parsing.ElementParsers.Reader;

public class XmlReaderMindMapLoader : IParser<RawMindMapData> {
	[PerformanceLogHook]
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