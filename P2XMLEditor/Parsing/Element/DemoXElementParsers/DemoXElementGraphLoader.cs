using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using P2XMLEditor.Helper;
using P2XMLEditor.Logging;
using P2XMLEditor.Parsing.Helpers;
using P2XMLEditor.Parsing.RawData;
using static P2XMLEditor.Parsing.Helpers.XElementExtensions;

namespace P2XMLEditor.Parsing.Element.DemoXElementParsers;

public class DemoXElementGraphLoader : IParser<RawGraphData> {
	
	[PerformanceLogHook]
	public void ProcessFile(string filePath, List<RawGraphData> raws) {
		using var xr = XmlReaderExtensions.InitializeFullFileReader(filePath);

		xr.MoveToContent();
		xr.ReadStartElement();
		
		while (xr.NodeType == XmlNodeType.Element) {
			var element = (XElement)XNode.ReadFrom(xr);
			var id = ulong.Parse(element.Attribute("id")!.Value);

			var paramInfos = new List<(string, string)>();
			var inputParamsElement = element.Element("InputParamsInfo.List");
			if (inputParamsElement != null) {
				foreach (var item in inputParamsElement.Elements("object")) {
					paramInfos.Add(new(
						item.Element("Name")!.Value,
						item.Element("Type")!.Value
					));
				}
			}

			var raw = new RawGraphData {
				Id = id,
				StateIds = ParseDemoListAsUlong(element, "States").ToArray(),
				EventLinkIds = ParseDemoListAsUlong(element, "EventLinks").ToArray(),
				GraphType = element.Element("GraphType")!.Value,
				EntryPointIds = ParseDemoListAsUlong(element, "EntryPoints").ToArray(),
				IgnoreBlock = element.Element("IgnoreBlock")?.Let(ParseBool),
				OwnerId = ulong.Parse(element.Element("Owner")!.Value),
				InputParamsInfo = paramInfos.Count > 0 ? paramInfos.ToArray() : null,
				InputLinkIds = ParseDemoListAsUlong(element, "InputLinks").ToArray(),
				OutputLinkIds = ParseDemoListAsUlong(element, "OutputLinks").ToArray(),
				Initial = element.Element("Initial")?.Let(ParseBool),
				Name = element.Element("Name")!.Value,
				ParentId = ulong.Parse(element.Element("Parent")!.Value),
				SubstituteGraphId = element.Element("SubstituteGraph") != null ?
					ulong.Parse(element.Element("SubstituteGraph")!.Value) : null
			};

			raws.Add(raw);
		}
	}
}
