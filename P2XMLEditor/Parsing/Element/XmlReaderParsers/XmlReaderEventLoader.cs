using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Xml;
using P2XMLEditor.GameData.VirtualMachineElements.Enums;
using P2XMLEditor.Helper;
using P2XMLEditor.Logging;
using P2XMLEditor.Parsing.RawData;
using static P2XMLEditor.Parsing.Helpers.XmlReaderExtensions;
using static P2XMLEditor.Parsing.Helpers.XElementExtensions;

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

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static (string, string)[] ReadMessages(XmlReader xr) {
		var length = int.Parse(xr.GetAttribute("count")!);
		xr.Read();

		var list = new (string, string)[length];
		for (var i = 0; i < length; i++) {
			xr.Read();
			list[i] = new(xr.GetStringValueAndAdvance(), xr.GetStringValueAndAdvance());
			xr.Read();
		}

		xr.Read();
		return list;
	}
}