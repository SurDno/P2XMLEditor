using System.Xml;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.GameData.VirtualMachineElements.Enums;
using P2XMLEditor.Helper;
using P2XMLEditor.Logging;
using P2XMLEditor.Parsing.RawData;
using static P2XMLEditor.Helper.XmlReaderExtensions;
using static P2XMLEditor.Helper.XmlParsingHelper;

namespace P2XMLEditor.Parsing.Element.XmlReaderParsers;

public class XmlReaderEventLoader : IParser<RawEventData> {
	[PerformanceLogHook]
	public void ProcessFile(string filePath, List<RawEventData> raws) {
		using var xr = InitializeFullFileReader(filePath);
		xr.SkipDeclarationAndRoot();

		while (xr.Read()) {
			if (xr.EndOfContainerReached()) break;

			var raw = new RawEventData {
				Id = xr.GetIdAndEnter(),
				EventParameterId = xr.Name == "EventParameter" ? xr.GetULongValueAndAdvance() : null,
				EventTime = ParseTimeSpanString(xr.GetStringValueAndAdvance()),
				Manual = xr.Name == "Manual" ? xr.GetBoolValueAndAdvance() : null,
				EventRaisingType = xr.GetStringValueAndAdvance().Deserialize<EventRaisingType>(),
				ConditionId = xr.Name == "Condition" ? xr.GetULongValueAndAdvance() : null,
				ChangeTo = xr.Name == "ChangeTo" ? xr.GetBoolValueAndAdvance() : null,
				Repeated = xr.Name == "Repeated" ? xr.GetBoolValueAndAdvance() : null,
				MessagesInfo = xr.Name == "MessagesInfo" ? ReadMessages(xr) : null,
				Name = xr.GetStringValueAndAdvance(),
				ParentId = xr.GetULongValueAndAdvance()
			};
			raws.Add(raw);
		}
	}

	private static List<MessageInfo>? ReadMessages(XmlReader xr) {
		xr.Read();
		if (xr.EndOfContainerReached()) {
			xr.Read();
			return null;
		}

		var list = new List<MessageInfo>();
		while (!xr.EndOfContainerReached()) {
			xr.Read();
			var name = xr.GetStringValueAndAdvance();
			var type = xr.GetStringValueAndAdvance();
			list.Add(new MessageInfo(name, type));
			xr.Read();
		}

		xr.Read();
		return list;
	}
}