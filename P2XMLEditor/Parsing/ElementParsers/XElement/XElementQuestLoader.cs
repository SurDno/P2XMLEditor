using System.Xml.Linq;
using P2XMLEditor.Helper;
using P2XMLEditor.Parsing.RawData;
using static P2XMLEditor.Helper.XmlParsingHelper;

namespace P2XMLEditor.Parsing.ElementParsers.XElement;

public class XElementQuestLoader : IParser<RawQuestData> {
    public void ProcessFile(string filePath, List<RawQuestData> raws) {
        foreach (var element in XDocument.Load(filePath).Root!.Elements()) {
            ulong id = ulong.Parse(element.Attribute(XNameCache.IdAttribute)!.Value);

            var raw = new RawQuestData {
                Id = id,
                Static = element.Element(XNameCache.Static) != null
                    ? bool.Parse(element.Element(XNameCache.Static)!.Value)
                    : null,
                FunctionalComponentIds = ReadULongList(element.Element(XNameCache.FunctionalComponents)!),
                EventGraphId = ulong.Parse(element.Element(XNameCache.EventGraph)!.Value),
                StandartParamIds = ReadDictULong(element.Element(XNameCache.StandartParams)!),
                CustomParamIds = ReadDictULong(element.Element(XNameCache.CustomParams)!),
                GameTimeContext = element.Element(XNameCache.GameTimeContext)?.Value,
                Name = element.Element(XNameCache.Name)!.Value,
                ParentId = ulong.Parse(element.Element(XNameCache.Parent)!.Value),
                InheritanceInfo = element.Element(XNameCache.InheritanceInfo) != null
                    ? ReadStrList(element.Element(XNameCache.InheritanceInfo)!)
                    : null,
                EventIds = element.Element(XNameCache.Events) != null
                    ? ReadULongList(element.Element(XNameCache.Events)!)
                    : null,
                ChildObjectIds = element.Element(XNameCache.ChildObjects) != null
                    ? ReadULongList(element.Element(XNameCache.ChildObjects)!)
                    : null,
                StartEventId = element.Element(XNameCache.StartEvent) != null
                    ? ulong.Parse(element.Element(XNameCache.StartEvent)!.Value)
                    : null
            };

            raws.Add(raw);
        }
    }
}
