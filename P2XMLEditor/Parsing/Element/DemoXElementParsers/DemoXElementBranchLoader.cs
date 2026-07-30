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

namespace P2XMLEditor.Parsing.Element.DemoXElementParsers;

public class DemoXElementBranchLoader : IParser<RawBranchData> {
	
	[PerformanceLogHook]
	public void ProcessFile(string filePath, List<RawBranchData> raws) {
		using var xr = XmlReaderExtensions.InitializeFullFileReader(filePath);

		xr.MoveToContent();
		xr.ReadStartElement();
		
		while (xr.NodeType == XmlNodeType.Element) {
			var element = (XElement)XNode.ReadFrom(xr);
			var id = ulong.Parse(element.Attribute("id")!.Value);
			
			var branchVariantInfo = element.Element("BranchVariantInfo.List")?.Elements("object")
				.Select(item => (item.Element("Name")!.Value, item.Element("Type")!.Value)).ToList() 
									?? null;
			
			var raw = new RawBranchData {
				Id = id,
				BranchConditionIds = ParseDemoListAsUlong(element, "BranchConditions").ToArray(),
				BranchType = element.Element("BranchType")!.Value.Deserialize<BranchType>(),
				BranchVariantInfo = branchVariantInfo?.ToArray(),
				EntryPointIds = ParseDemoListAsUlong(element, "EntryPoints").ToArray(),
				IgnoreBlock = element.Element("IgnoreBlock")?.Let(ParseBool) ?? false,
				OwnerId = ulong.Parse(element.Element("Owner")!.Value),
				InputLinkIds = ParseDemoListAsUlong(element, "InputLinks").ToArray(),
				OutputLinkIds = ParseDemoListAsUlong(element, "OutputLinks").ToArray(),
				Initial = element.Element("Initial")?.Let(ParseBool) ?? false,
				Name = element.Element("Name")!.Value,
				ParentId = ulong.Parse(element.Element("Parent")!.Value)
			};

			raws.Add(raw);
		}
	}
}
