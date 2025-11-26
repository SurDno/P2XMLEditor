using System.Xml.Linq;
using P2XMLEditor.Helper;
using P2XMLEditor.Parsing.RawData;
using static P2XMLEditor.Helper.XmlParsingHelper;

namespace P2XMLEditor.Parsing.ElementParsers.XElement;

public class XElementTalkingLoader : IParser<RawTalkingData> {
	public void ProcessFile(string filePath, List<RawTalkingData> raws) {
		foreach (var element in XDocument.Load(filePath).Root!.Elements()) {
			ulong id = ulong.Parse(element.Attribute(XNameCache.IdAttribute)!.Value);

			var raw = new RawTalkingData {
				Id = id,
				StateIds = element.Element(XNameCache.States) != null
					? ReadULongList(element.Element(XNameCache.States)!)
					: [],
				EventLinkIds = element.Element(XNameCache.EventLinks) != null
					? ReadULongList(element.Element(XNameCache.EventLinks)!)
					: [],
				EntryPointIds = element.Element(XNameCache.EntryPoints) != null
					? ReadULongList(element.Element(XNameCache.EntryPoints)!)
					: [],
				IgnoreBlock = element.Element(XNameCache.IgnoreBlock) != null
					? bool.Parse(element.Element(XNameCache.IgnoreBlock)!.Value)
					: null,
				OwnerId = ulong.Parse(element.Element(XNameCache.Owner)!.Value),
				Initial = element.Element(XNameCache.Initial) != null
					? bool.Parse(element.Element(XNameCache.Initial)!.Value)
					: null,
				Name = element.Element(XNameCache.Name)!.Value,
				ParentId = ulong.Parse(element.Element(XNameCache.Parent)!.Value)
			};

			raws.Add(raw);
		}
	}
}