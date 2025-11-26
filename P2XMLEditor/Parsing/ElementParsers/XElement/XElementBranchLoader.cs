using System.Xml.Linq;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.GameData.VirtualMachineElements.Enums;
using P2XMLEditor.Helper;
using P2XMLEditor.Parsing.RawData;
using static P2XMLEditor.Helper.XmlParsingHelper;

namespace P2XMLEditor.Parsing.ElementParsers.XElement;

public class XElementBranchLoader : IParser<RawBranchData> {
	public void ProcessFile(string filePath, List<RawBranchData> raws) {
		foreach (var element in XDocument.Load(filePath).Root!.Elements()) {
			var id = ulong.Parse(element.Attribute(XNameCache.IdAttribute)!.Value);
            
			var branchVariantInfo = element.Element(XNameCache.BranchVariantInfo)?.Elements(XNameCache.Item)
				.Select(item => new BranchVariantInfo {
					Name = item.Element(XNameCache.Name)!.Value,
					Type = item.Element(XNameCache.Type)!.Value
				})
				.ToList() ?? null;
            
			var raw = new RawBranchData {
				Id = id,
				BranchConditionIds = ParseListElementAsUlong(element, XNameCache.BranchConditions),
				BranchType = element.Element(XNameCache.BranchType)!.Value.Deserialize<BranchType>(),
				BranchVariantInfo = branchVariantInfo,
				EntryPointIds = ParseListElementAsUlong(element, XNameCache.EntryPoints),
				IgnoreBlock = element.Element(XNameCache.IgnoreBlock)?.Let(ParseBool),
				OwnerId = ulong.Parse(element.Element(XNameCache.Owner)!.Value),
				InputLinkIds = ParseListElementAsUlong(element, XNameCache.InputLinks),
				OutputLinkIds = ParseListElementAsUlong(element, XNameCache.OutputLinks),
				Initial = element.Element(XNameCache.Initial)?.Let(ParseBool),
				Name = element.Element(XNameCache.Name)!.Value,
				ParentId = ulong.Parse(element.Element(XNameCache.Parent)!.Value)
			};

			raws.Add(raw);
		}
	}
}