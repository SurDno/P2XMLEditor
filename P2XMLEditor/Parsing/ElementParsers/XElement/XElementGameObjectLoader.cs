using System.Xml.Linq;
using P2XMLEditor.Helper;
using P2XMLEditor.Parsing.RawData;
using static P2XMLEditor.Helper.XmlParsingHelper;

namespace P2XMLEditor.Parsing.ElementParsers.XElement;

public class XElementGameObjectLoader : IParser<RawGameObjectData> {
    public void ProcessFile(string filePath, List<RawGameObjectData> raws) {
        foreach (var element in XDocument.Load(filePath).Root!.Elements()) {
            var id = ulong.Parse(element.Attribute(XNameCache.IdAttribute)!.Value);

            var raw = new RawGameObjectData {
                Id = id,
                Static = element.Element(XNameCache.Static)?.Let(ParseBool),
                FunctionalComponentIds = ParseListElementAsUlong(element, XNameCache.FunctionalComponents),
                EventGraphId = element.Element(XNameCache.EventGraph) != null ?
                    ulong.Parse(element.Element(XNameCache.EventGraph)!.Value) : null,
                StandartParamIds = ParseDictionaryElementAsUlong(element, XNameCache.StandartParams),
                CustomParamIds = ParseDictionaryElementAsUlong(element, XNameCache.CustomParams),
                GameTimeContext = element.Element(XNameCache.GameTimeContext)?.Value,
                Name = element.Element(XNameCache.Name)!.Value,
                ParentId = ulong.Parse(element.Element(XNameCache.Parent)!.Value),
                InheritanceInfo = ParseListElement(element, XNameCache.InheritanceInfo),
                EventIds = ParseListElementAsUlong(element, XNameCache.Events),
                ChildObjectIds = ParseListElementAsUlong(element, XNameCache.ChildObjects),
                WorldPositionGuid = element.Element(XNameCache.WorldPositionGuid)?.Value,
                EngineTemplateId = element.Element(XNameCache.EngineTemplateId)?.Value,
                EngineBaseTemplateId = element.Element(XNameCache.EngineBaseTemplateId)?.Value,
                Instantiated = element.Element(XNameCache.Instantiated)?.Let(ParseBool)
            };

            raws.Add(raw);
        }
    }
}