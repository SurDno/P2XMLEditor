using System.Xml.Linq;
using P2XMLEditor.Helper;
using P2XMLEditor.Parsing.RawData;
using static P2XMLEditor.Helper.XmlParsingHelper;

namespace P2XMLEditor.Parsing.Element.XElementParsers;

public class XElementBlueprintLoader : IParser<RawBlueprintData> {
	public void ProcessFile(string filePath, List<RawBlueprintData> raws) {
		using var xr = XmlReaderExtensions.InitializeFullFileReader(filePath);

		xr.MoveToContent();
		xr.ReadStartElement();
		
		while (xr.NodeType == System.Xml.XmlNodeType.Element) {
			var element = (System.Xml.Linq.XElement)XNode.ReadFrom(xr);
			var id = ulong.Parse(element.Attribute(XNameCache.IdAttribute)!.Value);

			var raw = new RawBlueprintData {
				Id = id,
				Static = element.Element(XNameCache.Static)?.Let(ParseBool),
				FunctionalComponentIds = ParseListElementAsUlong(element, XNameCache.FunctionalComponents),
				EventGraphId = element.Element(XNameCache.EventGraph) != null ?
					ulong.Parse(element.Element(XNameCache.EventGraph)!.Value) : null,
				StandartParamIds = ParseDictionaryElementAsUlong(element, XNameCache.StandartParams),
				CustomParamIds = ParseDictionaryElementAsUlong(element, XNameCache.CustomParams),
				GameTimeContext = element.Element(XNameCache.GameTimeContext)?.Value,
				Name = element.Element(XNameCache.Name)!.Value,
				ParentId = element.Element(XNameCache.Parent) != null ?
					ulong.Parse(element.Element(XNameCache.Parent)!.Value) : null,
				InheritanceInfo = ParseListElement(element, XNameCache.InheritanceInfo),
				EventIds = ParseListElementAsUlong(element, XNameCache.Events),
				ChildObjectIds = ParseListElementAsUlong(element, XNameCache.ChildObjects)
			};

			raws.Add(raw);
		}
	}
}