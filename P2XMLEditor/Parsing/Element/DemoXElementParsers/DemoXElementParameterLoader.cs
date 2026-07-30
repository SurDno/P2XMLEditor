using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;
using P2XMLEditor.Logging;
using P2XMLEditor.Parsing.Helpers;
using P2XMLEditor.Parsing.RawData;
using static P2XMLEditor.Parsing.Helpers.XElementExtensions;

namespace P2XMLEditor.Parsing.Element.DemoXElementParsers;

public class DemoXElementParameterLoader : IParser<RawParameterData> {
	
	[PerformanceLogHook]
	public void ProcessFile(string filePath, List<RawParameterData> raws) {
		using var xr = XmlReaderExtensions.InitializeFullFileReader(filePath);

		xr.MoveToContent();
		xr.ReadStartElement();
		
		while (xr.NodeType == XmlNodeType.Element) {
			var element = (XElement)XNode.ReadFrom(xr);
			var id = ulong.Parse(element.Attribute("id")!.Value);

			var raw = new RawParameterData();
			raw.Id = id;
			raw.OwnerComponentId = element.Element("OwnerComponent") != null ?
				ulong.Parse(element.Element("OwnerComponent")!.Value) : null;
			raw.Type = element.Element("Type")!.Value;
			if (raw.Type.EndsWith('%'))
				raw.Type = raw.Type[..^1];
			raw.Value = element.Element("Value") != null ? element.Element("Value")!.Value : string.Empty;
			raw.Implicit = element.Element("Implicit")?.Let(ParseBool) ?? false;
			raw.Name = element.Element("Name") != null ? element.Element("Name")!.Value : string.Empty;
			raw.ParentId = ulong.Parse(element.Element("Parent")!.Value);

			raws.Add(raw);
		}
	}
}
