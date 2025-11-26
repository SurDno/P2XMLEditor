using System.Xml.Linq;
using P2XMLEditor.Helper;
using P2XMLEditor.Parsing.RawData;
using static P2XMLEditor.Helper.XmlParsingHelper;

namespace P2XMLEditor.Parsing.ElementParsers.XElement;

public class XElementGraphLinkLoader : IParser<RawGraphLinkData> {
	public void ProcessFile(string filePath, List<RawGraphLinkData> raws) {
		foreach (var element in XDocument.Load(filePath).Root!.Elements()) {
			var id = ulong.Parse(element.Attribute(XNameCache.IdAttribute)!.Value);

			var raw = new RawGraphLinkData {
				Id = id,
				EventId = element.Element(XNameCache.Event) != null ?
					ulong.Parse(element.Element(XNameCache.Event)!.Value) : null,
				EventObject = element.Element(XNameCache.EventObject)!.Value,
				SourceExitPointIndex = int.Parse(element.Element(XNameCache.SourceExitPointIndex)!.Value),
				DestEntryPointIndex = int.Parse(element.Element(XNameCache.DestEntryPointIndex)!.Value),
				SourceParams = ParseListElement(element, XNameCache.SourceParams),
				SourceId = element.Element(XNameCache.Source) != null ?
					ulong.Parse(element.Element(XNameCache.Source)!.Value) : null,
				DestinationId = element.Element(XNameCache.Destination) != null ?
					ulong.Parse(element.Element(XNameCache.Destination)!.Value) : null,
				Enabled = element.Element(XNameCache.Enabled)?.Let(ParseBool),
				Name = element.Element(XNameCache.Name)!.Value,
				ParentId = ulong.Parse(element.Element(XNameCache.Parent)!.Value)
			};

			raws.Add(raw);
		}
	}
}