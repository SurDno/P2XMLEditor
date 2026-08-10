using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;
using P2XMLEditor.Logging;
using P2XMLEditor.Parsing.Helpers;
using P2XMLEditor.Parsing.RawData;

namespace P2XMLEditor.Parsing.Element.AlphaXElementParsers;

public class AlphaXElementEntryPointLoader : IParser<RawEntryPointData> {
	
	[PerformanceLogHook]
	public void ProcessFile(string filePath, List<RawEntryPointData> raws) {
		using var xr = XmlReaderExtensions.InitializeFullFileReader(filePath);

		xr.MoveToContent();
		xr.ReadStartElement();
		
		while (xr.NodeType == XmlNodeType.Element) {
			var element = (XElement)XNode.ReadFrom(xr);
			var id = ulong.Parse(element.Element("Guid")!.Value);

			var raw = new RawEntryPointData {
				Id = id,
				ActionLineId = element.Element("ActionLine") != null ?
					ulong.Parse(element.Element("ActionLine")!.Value) : null,
				Name = element.Element("Name")!.Value,
				// A few alpha entry points carry no Parent though a node lists them; that owner is
				// put back in AlphaXElementParsingExecutor once every file is read. Left 0 rather
				// than dereferenced, which would throw on those few.
				ParentId = element.Element("Parent") != null ? ulong.Parse(element.Element("Parent")!.Value) : 0
			};

			raws.Add(raw);
		}
	}
}
