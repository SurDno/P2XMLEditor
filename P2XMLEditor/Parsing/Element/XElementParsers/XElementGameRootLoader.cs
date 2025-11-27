using System.Xml.Linq;
using P2XMLEditor.GameData.VirtualMachineElements.Enums;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes;
using P2XMLEditor.Helper;
using P2XMLEditor.Parsing.RawData;
using static P2XMLEditor.Helper.XmlParsingHelper;

namespace P2XMLEditor.Parsing.Element.XElementParsers;

public class XElementGameRootLoader : IParser<RawGameRootData> {
    public void ProcessFile(string filePath, List<RawGameRootData> raws) {
        using var xr = XmlReaderExtensions.InitializeFullFileReader(filePath);

        xr.MoveToContent();
        xr.ReadStartElement();
		
        while (xr.NodeType == System.Xml.XmlNodeType.Element) {
            var element = (System.Xml.Linq.XElement)XNode.ReadFrom(xr);
            var id = ulong.Parse(element.Attribute(XNameCache.IdAttribute)!.Value);

            var scenesStructure = new Dictionary<ulong, SceneStructureEntry>();
            var structureElement = element.Element(XNameCache.HierarchyScenesStructure);
            if (structureElement != null) {
                foreach (var item in structureElement.Elements(XNameCache.Item)) {
                    var key = item.Attribute(XNameCache.KeyAttribute)!.Value;
                    var entry = new SceneStructureEntry();

                    foreach (var containerName in Enum.GetNames(typeof(ChildContainerType))) {
                        var container = item.Element(containerName);
                        if (container == null) continue;
                        var type = Enum.Parse<ChildContainerType>(containerName);
                        var children = container.Elements(XNameCache.Item).Select(e => ulong.Parse(e.Value)).ToList();
                        entry.Containers[type] = children;
                    }

                    scenesStructure[ulong.Parse(key)] = entry;
                }
            }

            var raw = new RawGameRootData {
                Id = id,
                FunctionalComponentIds = ParseListElementAsUlong(element, XNameCache.FunctionalComponents),
                EventGraphId = element.Element(XNameCache.EventGraph) != null ?
                    ulong.Parse(element.Element(XNameCache.EventGraph)!.Value) : null,
                StandartParamIds = ParseDictionaryElementAsUlong(element, XNameCache.StandartParams),
                CustomParamIds = ParseDictionaryElementAsUlong(element, XNameCache.CustomParams),
                Name = element.Element(XNameCache.Name)!.Value,
                EventIds = ParseListElementAsUlong(element, XNameCache.Events),
                ChildObjectIds = ParseListElementAsUlong(element, XNameCache.ChildObjects),
                SampleIds = ParseListElementAsUlong(element, XNameCache.Samples),
                LogicMapIds = ParseListElementAsUlong(element, XNameCache.LogicMaps),
                GameModeIds = ParseListElementAsUlong(element, XNameCache.GameModes),
                BaseToEngineGuidsTable = ParseDictionaryElement(element, XNameCache.BaseToEngineGuidsTable),
                HierarchyScenesStructure = scenesStructure,
                HierarchyEngineGuidsTable = ParseListElement(element, XNameCache.HierarchyEngineGuidsTable),
                WorldObjectSaveOptimizeMode = element.Element(XNameCache.WorldObjectSaveOptimizeMode)?.Let(ParseBool)
            };

            raws.Add(raw);
        }
    }
}