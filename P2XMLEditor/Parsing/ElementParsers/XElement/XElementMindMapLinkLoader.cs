using System.Xml.Linq;
using P2XMLEditor.Helper;
using P2XMLEditor.Parsing.RawData;
using static P2XMLEditor.Helper.XmlParsingHelper;

namespace P2XMLEditor.Parsing.ElementParsers.XElement;

public class XElementMindMapLinkLoader : IParser<RawMindMapLinkData> {
	public void ProcessFile(string filePath, List<RawMindMapLinkData> raws) {
		foreach (var element in XDocument.Load(filePath).Root!.Elements()) {
			var id = ulong.Parse(element.Attribute(XNameCache.IdAttribute)!.Value);

			var raw = new RawMindMapLinkData {
				Id = id,
				ParentId = ulong.Parse(element.Element(XNameCache.Parent)!.Value),
				SourceId = ulong.Parse(element.Element(XNameCache.Source)!.Value),
				DestinationId = ulong.Parse(element.Element(XNameCache.Destination)!.Value)
			};

			raws.Add(raw);
		}
	}
}