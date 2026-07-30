using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using P2XMLEditor.GameData.VirtualMachineElements.Enums;
using P2XMLEditor.Helper;
using P2XMLEditor.Logging;
using P2XMLEditor.Parsing.Helpers;
using P2XMLEditor.Parsing.RawData;

namespace P2XMLEditor.Parsing.Element.XElementParsers;
using static XElementExtensions;

public class XElementBranchLoader : IParser<RawBranchData> {
	
	[PerformanceLogHook]
	public void ProcessFile(string filePath, List<RawBranchData> raws) {
		using var xr = XmlReaderExtensions.InitializeFullFileReader(filePath);

		xr.MoveToContent();
		xr.ReadStartElement();
		
		while (xr.NodeType == XmlNodeType.Element) {
			var element = (XElement)XNode.ReadFrom(xr);
			var id = ulong.Parse(element.Attribute(XNameCache.IdAttribute)!.Value);
			
			var branchVariantInfo = element.Element(XNameCache.BranchVariantInfo)?.Elements(XNameCache.Item).
				Select(item => (item.Element(XNameCache.Name)!.Value, item.Element(XNameCache.Type)!.Value)).ToList() 
									?? null;
			
			var raw = new RawBranchData {
				Id = id,
				BranchConditionIds = ParseListElementAsUlong(element, XNameCache.BranchConditions).ToArray(),
				BranchType = element.Element(XNameCache.BranchType)!.Value.Deserialize<BranchType>(),
				BranchVariantInfo = branchVariantInfo?.ToArray(),
				EntryPointIds = ParseListElementAsUlong(element, XNameCache.EntryPoints).ToArray(),
				IgnoreBlock = element.Element(XNameCache.IgnoreBlock)?.Let(ParseBool) ?? false,
				OwnerId = ulong.Parse(element.Element(XNameCache.Owner)!.Value),
				InputLinkIds = ParseListElementAsUlong(element, XNameCache.InputLinks).ToArray(),
				OutputLinkIds = ParseListElementAsUlong(element, XNameCache.OutputLinks).ToArray(),
				Initial = element.Element(XNameCache.Initial)?.Let(ParseBool) ?? false,
				Name = element.Element(XNameCache.Name)!.Value,
				ParentId = ulong.Parse(element.Element(XNameCache.Parent)!.Value)
			};

			raws.Add(raw);
		}
	}
}
