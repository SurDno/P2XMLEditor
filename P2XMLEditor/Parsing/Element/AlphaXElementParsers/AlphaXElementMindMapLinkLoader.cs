using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using P2XMLEditor.Logging;
using P2XMLEditor.Parsing.Helpers;
using P2XMLEditor.Parsing.RawData;

namespace P2XMLEditor.Parsing.Element.AlphaXElementParsers;

public class AlphaXElementMindMapLinkLoader : IParser<RawMindMapLinkData> {
	
	[PerformanceLogHook]
	public void ProcessFile(string filePath, List<RawMindMapLinkData> raws) {
		if (!File.Exists(filePath)) return;
		using var xr = AlphaFormat.OpenReader(filePath);

		xr.MoveToContent();
		xr.ReadStartElement();
		
		while (xr.NodeType == XmlNodeType.Element) {
			var element = (XElement)XNode.ReadFrom(xr);
			var id = ulong.Parse(element.Element("Guid")!.Value);

			var raw = new RawMindMapLinkData {
				Id = id,
				ParentId = ulong.Parse(element.Element("Parent")!.Value),
				SourceId = ulong.Parse(element.Element("Source")!.Value),
				DestinationId = ulong.Parse(element.Element("Destination")!.Value)
			};

			raws.Add(raw);
		}
	}
}
