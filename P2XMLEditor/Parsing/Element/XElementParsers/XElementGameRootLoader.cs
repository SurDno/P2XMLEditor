using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using P2XMLEditor.GameData.VirtualMachineElements.Enums;
using P2XMLEditor.Logging;
using P2XMLEditor.Parsing.Helpers;
using P2XMLEditor.Parsing.RawData;
using static P2XMLEditor.Parsing.Helpers.XElementExtensions;

namespace P2XMLEditor.Parsing.Element.XElementParsers;

public class XElementGameRootLoader : IParser<RawGameRootData> {
	[PerformanceLogHook]
	public void ProcessFile(string filePath, List<RawGameRootData> raws) {
		using var xr = XmlReaderExtensions.InitializeFullFileReader(filePath);

		xr.MoveToContent();
		xr.ReadStartElement();
		
		while (xr.NodeType == XmlNodeType.Element) {
			var element = (XElement)XNode.ReadFrom(xr);
			var id = ulong.Parse(element.Attribute(XNameCache.IdAttribute)!.Value);

			var scenesStructure = new Dictionary<ulong, Dictionary<ChildContainerType, ulong[]>>();
			var structureElement = element.Element(XNameCache.HierarchyScenesStructure);

			if (structureElement != null) {
				foreach (var item in structureElement.Elements(XNameCache.Item)) {
					var key = ulong.Parse(item.Attribute(XNameCache.KeyAttribute)!.Value);
					var containers = new Dictionary<ChildContainerType, ulong[]>();
					foreach (var container in item.Elements()) {
						var type = Enum.Parse<ChildContainerType>(container.Name.LocalName);
						var countAttr = container.Attribute(XNameCache.CountAttribute);
						var childrenElements = container.Elements(XNameCache.Item).ToArray();
						ulong[] array;
						if (countAttr != null) {
							var count = int.Parse(countAttr.Value);
							array = new ulong[count];
							for (var i = 0; i < count; i++) 
								array[i] = ulong.Parse(childrenElements[i].Value);
						} else {

							array = childrenElements
								.Select(e => ulong.Parse(e.Value))
								.ToArray();
						}

						containers[type] = array;
					}

					scenesStructure[key] = containers;
				}
			}

			var raw = new RawGameRootData {
				Id = id,
				FunctionalComponentIds = ParseListElementAsUlong(element, XNameCache.FunctionalComponents).ToArray(),
				EventGraphId = element.Element(XNameCache.EventGraph) != null ?
					ulong.Parse(element.Element(XNameCache.EventGraph)!.Value) : null,
				StandartParamIds = ReadDictULong(element.Element(XNameCache.StandartParams)!),
				CustomParamIds = ReadDictULong(element.Element(XNameCache.CustomParams)!),
				Name = element.Element(XNameCache.Name)!.Value,
				EventIds = ParseListElementAsUlong(element, XNameCache.Events).ToArray(),
				ChildObjectIds = ParseListElementAsUlong(element, XNameCache.ChildObjects).ToArray(),
				SampleIds = ParseListElementAsUlong(element, XNameCache.Samples).ToArray(),
				LogicMapIds = ParseListElementAsUlong(element, XNameCache.LogicMaps).ToArray(),
				GameModeIds = ParseListElementAsUlong(element, XNameCache.GameModes).ToArray(),
				BaseToEngineGuidsTable = ParseDictionaryElement(element, XNameCache.BaseToEngineGuidsTable),
				HierarchyScenesStructure = scenesStructure,
				HierarchyEngineGuidsTable = ParseListElement(element, XNameCache.HierarchyEngineGuidsTable).ToArray(),
				WorldObjectSaveOptimizeMode = element.Element(XNameCache.WorldObjectSaveOptimizeMode)?.Let(ParseBool) ?? false
			};

			raws.Add(raw);
		}
	}
}
