using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;
using P2XMLEditor.Logging;
using P2XMLEditor.Parsing.Helpers;
using P2XMLEditor.Parsing.RawData;
using static P2XMLEditor.Parsing.Helpers.XElementExtensions;

namespace P2XMLEditor.Parsing.Element.AlphaXElementParsers;

public class AlphaXElementGraphLinkLoader : IParser<RawGraphLinkData> {
	
	[PerformanceLogHook]
	public void ProcessFile(string filePath, List<RawGraphLinkData> raws) {
		using var xr = AlphaFormat.OpenReader(filePath);

		xr.MoveToContent();
		xr.ReadStartElement();
		
		while (xr.NodeType == XmlNodeType.Element) {
			var element = (XElement)XNode.ReadFrom(xr);
			var id = ulong.Parse(element.Element("Guid")!.Value);

			var raw = new RawGraphLinkData {
				Id = id,
				EventId = element.Element("Event") != null ?
					ulong.Parse(element.Element("Event")!.Value) : null,
				EventObject = element.Element("EventObjectID")!.Value,
				SourceId = element.Element("Source") != null ?
					ulong.Parse(element.Element("Source")!.Value) : null,
				DestinationId = element.Element("Destination") != null ?
					ulong.Parse(element.Element("Destination")!.Value) : null,
				SourceExitPointIndex = int.Parse(element.Element("SourceExitPointIndex")!.Value),
				DestEntryPointIndex = int.Parse(element.Element("DestEntryPointIndex")!.Value),
				SourceParams = ParseDemoList(element, "SourceParamNames").ToArray(),
				Enabled = element.Element("Enabled")?.Let(ParseBool) ?? false,
				Name = element.Element("Name")!.Value,
				ParentId = ulong.Parse(element.Element("Parent")!.Value)
			};

			raws.Add(raw);
		}
	}
}
