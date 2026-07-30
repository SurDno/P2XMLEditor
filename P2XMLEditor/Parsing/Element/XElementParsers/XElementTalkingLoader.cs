using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;
using P2XMLEditor.Logging;
using P2XMLEditor.Parsing.Helpers;
using P2XMLEditor.Parsing.RawData;
using static P2XMLEditor.Parsing.Helpers.XElementExtensions;

namespace P2XMLEditor.Parsing.Element.XElementParsers;

public class XElementTalkingLoader : IParser<RawTalkingData> {
	[PerformanceLogHook]
	public void ProcessFile(string filePath, List<RawTalkingData> raws) {
		using var xr = XmlReaderExtensions.InitializeFullFileReader(filePath);

		xr.MoveToContent();
		xr.ReadStartElement();
		
		while (xr.NodeType == XmlNodeType.Element) {
			var element = (XElement)XNode.ReadFrom(xr);
			var id = ulong.Parse(element.Attribute(XNameCache.IdAttribute)!.Value);

			var raw = new RawTalkingData {
				Id = id,
				StateIds = element.Element(XNameCache.States) != null
					? ReadULongList(element.Element(XNameCache.States)!).ToArray()
					: [],
				EventLinkIds = element.Element(XNameCache.EventLinks) != null
					? ReadULongList(element.Element(XNameCache.EventLinks)!).ToArray()
					: [],
				EntryPointIds = element.Element(XNameCache.EntryPoints) != null
					? ReadULongList(element.Element(XNameCache.EntryPoints)!).ToArray()
					: [],
				IgnoreBlock = element.Element(XNameCache.IgnoreBlock) != null
					? bool.Parse(element.Element(XNameCache.IgnoreBlock)!.Value)
					: false,
				OwnerId = ulong.Parse(element.Element(XNameCache.Owner)!.Value),
				InputLinkIds = element.Element(XNameCache.InputLinks) != null
					? ReadULongList(element.Element(XNameCache.InputLinks)!).ToArray()
					: [],
				Initial = element.Element(XNameCache.Initial) != null
					? bool.Parse(element.Element(XNameCache.Initial)!.Value)
					: false,
				Name = element.Element(XNameCache.Name)!.Value,
				ParentId = ulong.Parse(element.Element(XNameCache.Parent)!.Value)
			};

			raws.Add(raw);
		}
	}
}
