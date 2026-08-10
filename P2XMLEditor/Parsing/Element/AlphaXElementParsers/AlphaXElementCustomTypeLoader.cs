using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using P2XMLEditor.Logging;
using P2XMLEditor.Parsing.Helpers;
using P2XMLEditor.Parsing.RawData;

namespace P2XMLEditor.Parsing.Element.AlphaXElementParsers;

public class AlphaXElementCustomTypeLoader : IParser<RawCustomTypeData> {
	
	[PerformanceLogHook]
	public void ProcessFile(string filePath, List<RawCustomTypeData> raws) {
		if (!File.Exists(filePath)) return;
		using var xr = XmlReaderExtensions.InitializeFullFileReader(filePath);

		xr.MoveToContent();
		xr.ReadStartElement();
		
		while (xr.NodeType == XmlNodeType.Element) {
			var element = (XElement)XNode.ReadFrom(xr);
			var id = ulong.Parse(element.Element("Guid")!.Value);

			var raw = new RawCustomTypeData {
				Id = id,
				Name = element.Element("Name")!.Value,
				ParentId = ulong.Parse(element.Element("Parent")!.Value)
			};

			raws.Add(raw);
		}
	}
}
