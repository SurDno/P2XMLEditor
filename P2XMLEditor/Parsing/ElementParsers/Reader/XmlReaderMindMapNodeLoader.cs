using System.Xml;
using P2XMLEditor.Helper;
using P2XMLEditor.Logging;
using P2XMLEditor.Parsing.RawData;
using static P2XMLEditor.Helper.XmlReaderExtensions;

namespace P2XMLEditor.Parsing.ElementParsers.Reader;

public class XmlReaderMindMapNodeLoader : IParser<RawMindMapNodeData> {
	[PerformanceLogHook]
	public void ProcessFile(string filePath, List<RawMindMapNodeData> raws) {
		using var xr = InitializeFullFileReader(filePath);
		xr.SkipDeclarationAndRoot();

		while (xr.Read()) {
			if (xr.EndOfContainerReached()) break;

			var raw = new RawMindMapNodeData {
				Id = xr.GetIdAndEnter(),
				LogicMapNodeType = xr.GetStringValueAndAdvance(),
				ContentIds = xr.Name == "NodeContent" ? xr.GetULongListAndAdvance() : null,
				GameScreenPosX = xr.GetFloatValueAndAdvance(),
				GameScreenPosY = xr.GetFloatValueAndAdvance(),
				InputLinkIds = xr.Name == "InputLinks" ? xr.GetULongListAndAdvance() : null,
				OutputLinkIds = xr.Name == "OutputLinks" ? xr.GetULongListAndAdvance() : null,
				Name = xr.GetStringValueAndAdvance(),
				ParentId = xr.GetULongValueAndAdvance()
			};

			raws.Add(raw);
		}
	}
}