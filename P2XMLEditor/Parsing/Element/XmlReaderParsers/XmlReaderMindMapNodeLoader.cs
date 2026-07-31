using System.Collections.Generic;
using P2XMLEditor.GameData.VirtualMachineElements.Enums;
using P2XMLEditor.Helper;
using P2XMLEditor.Logging;
using P2XMLEditor.Parsing.RawData;
using static P2XMLEditor.Parsing.Helpers.XmlReaderExtensions;

namespace P2XMLEditor.Parsing.Element.XmlReaderParsers;

public class XmlReaderMindMapNodeLoader : IParser<RawMindMapNodeData> {
	[PerformanceLogHook]
	public void ProcessFile(string filePath, List<RawMindMapNodeData> raws) {
		using var xr = InitializeFullFileReader(filePath);
		xr.SkipDeclarationAndRoot();

		while (xr.Read()) {
			if (xr.EndOfContainerReached()) break;

			var raw = new RawMindMapNodeData {
				Id = xr.GetIdAndEnter(),
				LogicMapNodeType = xr.Name == "LogicMapNodeType" ? xr.GetStringValueAndAdvance().Deserialize<LogicMapNodeType>() : LogicMapNodeType.Common,
				ContentIds = xr.Name == "NodeContent" ? xr.GetULongListAndAdvance() : null,
				GameScreenPosX = xr.Name == "GameScreenPosX" ? xr.GetFloatValueAndAdvance() : 0f,
				GameScreenPosY = xr.Name == "GameScreenPosY" ? xr.GetFloatValueAndAdvance() : 0f,
				InputLinkIds = xr.Name == "InputLinks" ? xr.GetULongListAndAdvance() : null,
				OutputLinkIds = xr.Name == "OutputLinks" ? xr.GetULongListAndAdvance() : null,
				Name = xr.GetStringValueAndAdvance(),
				ParentId = xr.GetULongValueAndAdvance()
			};

			raws.Add(raw);
		}
	}
}
