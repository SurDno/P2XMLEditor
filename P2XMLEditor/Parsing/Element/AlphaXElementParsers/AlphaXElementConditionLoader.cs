using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using P2XMLEditor.GameData.VirtualMachineElements.Enums;
using P2XMLEditor.Helper;
using P2XMLEditor.Logging;
using P2XMLEditor.Parsing.Helpers;
using P2XMLEditor.Parsing.RawData;
using static P2XMLEditor.Parsing.Helpers.XElementExtensions;

namespace P2XMLEditor.Parsing.Element.AlphaXElementParsers;

public class AlphaXElementConditionLoader : IParser<RawConditionData> {
	
	[PerformanceLogHook]
	public void ProcessFile(string filePath, List<RawConditionData> raws) {
		using var xr = AlphaFormat.OpenReader(filePath);

		xr.MoveToContent();
		xr.ReadStartElement();
		
		while (xr.NodeType == XmlNodeType.Element) {
			var element = (XElement)XNode.ReadFrom(xr);
			var id = ulong.Parse(element.Element("Guid")!.Value);

			var raw = new RawConditionData {
				Id = id,
				PredicateIds = ParseDemoListAsUlong(element, "Predicates").ToArray(),
				Operation = element.Element("Operation")!.Value.Deserialize<ConditionOperation>(),
				Name = element.Element("Name") != null ? element.Element("Name")!.Value : string.Empty,
				OrderIndex = int.Parse(element.Element("OrderIndex")!.Value)
			};

			raws.Add(raw);
		}
	}
}
