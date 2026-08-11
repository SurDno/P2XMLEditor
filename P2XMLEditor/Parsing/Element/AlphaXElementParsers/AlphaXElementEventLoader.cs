using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using P2XMLEditor.GameData.VirtualMachineElements.Enums;
using P2XMLEditor.Helper;
using P2XMLEditor.Parsing.Helpers;
using P2XMLEditor.Parsing.RawData;
using static P2XMLEditor.Parsing.Helpers.XElementExtensions;

namespace P2XMLEditor.Parsing.Element.AlphaXElementParsers;

public class AlphaXElementEventLoader : IParser<RawEventData> {
	public void ProcessFile(string filePath, List<RawEventData> raws) {
		using var xr = AlphaFormat.OpenReader(filePath);

		xr.MoveToContent();
		xr.ReadStartElement();
		
		while (xr.NodeType == XmlNodeType.Element) {
			var element = (XElement)XNode.ReadFrom(xr);
			var id = ulong.Parse(element.Element("Guid")!.Value);

			var messagesInfo = new List<(string, string)>();
			var messagesElement = element.Element("MessagesInfo");
			if (messagesElement != null && !string.IsNullOrEmpty(messagesElement.Value)) {
				var entries = messagesElement.Value.Split(["MESS&PARAM"], StringSplitOptions.RemoveEmptyEntries);
				foreach (var entry in entries) {
					var parts = entry.Split(["M&P&PM"], StringSplitOptions.None);
					if (parts.Length >= 2) {
						messagesInfo.Add(new(parts[0], parts[1].TrimEnd('%')));
					}
				}
			}

			var raw = new RawEventData {
				Id = id,
				EventTime = ParseTimeSpanString(element.Element("EventTime")!.Value),
				Manual = element.Element("Procedural")?.Let(ParseBool) ?? false,
				EventRaisingType = element.Element("EventRaisingType")!.Value.Deserialize<EventRaisingType>(),
				ChangeTo = element.Element("ChangeTo")?.Let(ParseBool) ?? false,
				Repeated = element.Element("Repeated")?.Let(ParseBool) ?? false,
				Name = element.Element("Name")!.Value,
				ParentId = ulong.Parse(element.Element("Parent")!.Value),
				EventParameterId = element.Element("EventParameter") != null ?
					ulong.Parse(element.Element("EventParameter")!.Value) : null,
				ConditionId = element.Element("Condition") != null ?
					ulong.Parse(element.Element("Condition")!.Value) : null,
				MessagesInfo = messagesInfo.Count > 0 ? messagesInfo.ToArray() : null
			};

			raws.Add(raw);
		}
	}
}
