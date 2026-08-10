using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;
using P2XMLEditor.Logging;
using P2XMLEditor.Parsing.Helpers;
using P2XMLEditor.Parsing.RawData;
using static P2XMLEditor.Parsing.Helpers.XElementExtensions;

namespace P2XMLEditor.Parsing.Element.AlphaXElementParsers;

public class AlphaXElementReplyLoader : IParser<RawReplyData> {
	[PerformanceLogHook]
	public void ProcessFile(string filePath, List<RawReplyData> raws) {
		using var xr = XmlReaderExtensions.InitializeFullFileReader(filePath);

		xr.MoveToContent();
		xr.ReadStartElement();
		
		while (xr.NodeType == XmlNodeType.Element) {
			var element = (XElement)XNode.ReadFrom(xr);
			var id = ulong.Parse(element.Element("Guid")!.Value);

			var raw = new RawReplyData {
				Id = id,
				Name = element.Element("Name")!.Value,
				TextId = ulong.Parse(element.Element("Text")!.Value),
				OnlyOnce = element.Element("OnlyOnce") != null
					? bool.Parse(element.Element("OnlyOnce")!.Value)
					: false,
				OnlyOneReply = element.Element("OnlyOneReply") != null
					? bool.Parse(element.Element("OnlyOneReply")!.Value)
					: false,
				Default = element.Element("Default") != null
					? bool.Parse(element.Element("Default")!.Value)
					: false,
				EnableConditionId = element.Element("EnableCondition") != null
					? ulong.Parse(element.Element("EnableCondition")!.Value)
					: null,
				ActionLineId = element.Element("ActionLine") != null
					? ulong.Parse(element.Element("ActionLine")!.Value)
					: null,
				OrderIndex = int.Parse(element.Element("OrderIndex")!.Value),
				ParentId = ulong.Parse(element.Element("Parent")!.Value)
			};

			raws.Add(raw);
		}
	}
}
