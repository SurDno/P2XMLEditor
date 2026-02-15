using System.Xml;
using System.Xml.Linq;
using P2XMLEditor.Parsing.Helpers;
using P2XMLEditor.Parsing.RawData;
using static P2XMLEditor.Parsing.Helpers.XElementExtensions;

namespace P2XMLEditor.Parsing.Element.XElementParsers;

public class XElementGameObjectLoader : IParser<RawGameObjectData> {
    public void ProcessFile(string filePath, List<RawGameObjectData> raws) {
        using var xr = XmlReaderExtensions.InitializeFullFileReader(filePath);

        xr.MoveToContent();
        xr.ReadStartElement();
		
        while (xr.NodeType == XmlNodeType.Element) {
            var element = (XElement)XNode.ReadFrom(xr);
            var id = ulong.Parse(element.Attribute(XNameCache.IdAttribute)!.Value);

            var raw = new RawGameObjectData {
                Id = id,
                Static = element.Element(XNameCache.Static)?.Let(ParseBool),
                FunctionalComponentIds = ParseListElementAsUlong(element, XNameCache.FunctionalComponents).ToArray(),
                EventGraphId = element.Element(XNameCache.EventGraph) != null ?
                    ulong.Parse(element.Element(XNameCache.EventGraph)!.Value) : null,
                StandartParamIds = ReadDictULong(element.Element(XNameCache.StandartParams)!),
                CustomParamIds = ReadDictULong(element.Element(XNameCache.CustomParams)!),
                GameTimeContext = element.Element(XNameCache.GameTimeContext)?.Value,
                Name = element.Element(XNameCache.Name)!.Value,
                ParentId = ulong.Parse(element.Element(XNameCache.Parent)!.Value),
                InheritanceInfo = ParseListElement(element, XNameCache.InheritanceInfo).ToArray(),
                EventIds = ParseListElementAsUlong(element, XNameCache.Events).ToArray(),
                ChildObjectIds = ParseListElementAsUlong(element, XNameCache.ChildObjects).ToArray(),
                WorldPositionGuid = element.Element(XNameCache.WorldPositionGuid)?.Value,
                EngineTemplateId = element.Element(XNameCache.EngineTemplateId)?.Value,
                EngineBaseTemplateId = element.Element(XNameCache.EngineBaseTemplateId)?.Value,
                Instantiated = element.Element(XNameCache.Instantiated)?.Let(ParseBool)
            };

            raws.Add(raw);
        }
    }
}