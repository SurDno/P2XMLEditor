using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;
using P2XMLEditor.Logging;
using P2XMLEditor.Parsing.Helpers;
using P2XMLEditor.Parsing.RawData;

namespace P2XMLEditor.Parsing.Element.DemoXElementParsers;

public class DemoXElementEntryPointLoader : IParser<RawEntryPointData> {
	
	[PerformanceLogHook]
	public void ProcessFile(string filePath, List<RawEntryPointData> raws) {
		using var xr = XmlReaderExtensions.InitializeFullFileReader(filePath);

		xr.MoveToContent();
		xr.ReadStartElement();
		
		while (xr.NodeType == XmlNodeType.Element) {
			var element = (XElement)XNode.ReadFrom(xr);
			var id = ulong.Parse(element.Attribute("id")!.Value);

			var raw = new RawEntryPointData {
				Id = id,
				ActionLineId = element.Element("ActionLine") != null ?
					ulong.Parse(element.Element("ActionLine")!.Value) : null,
				Name = element.Element("Name")!.Value,
				ParentId = element.Element("Parent") != null && !string.IsNullOrEmpty(element.Element("Parent")!.Value) ?
					ulong.Parse(element.Element("Parent")!.Value) : null
			};

			raws.Add(raw);
		}
	}
}
