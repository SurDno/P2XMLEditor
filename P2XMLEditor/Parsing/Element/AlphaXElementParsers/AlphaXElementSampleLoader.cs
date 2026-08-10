using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;
using P2XMLEditor.GameData.VirtualMachineElements.Enums;
using P2XMLEditor.Helper;
using P2XMLEditor.Logging;
using P2XMLEditor.Parsing.Helpers;
using P2XMLEditor.Parsing.RawData;

namespace P2XMLEditor.Parsing.Element.AlphaXElementParsers;

public class AlphaXElementSampleLoader : IParser<RawSampleData> {
	[PerformanceLogHook]
	public void ProcessFile(string filePath, List<RawSampleData> raws) {
		using var xr = XmlReaderExtensions.InitializeFullFileReader(filePath);

		xr.MoveToContent();
		xr.ReadStartElement();
		
		while (xr.NodeType == XmlNodeType.Element) {
			var element = (XElement)XNode.ReadFrom(xr);
			var id = ulong.Parse(element.Element("Guid")!.Value);

			var raw = new RawSampleData {
				Id = id,
				SampleType = element.Element("SampleType")!.Value.Deserialize<SampleType>(),
				EngineId = element.Element("EngineID")!.Value
			};

			raws.Add(raw);
		}
	}
}
