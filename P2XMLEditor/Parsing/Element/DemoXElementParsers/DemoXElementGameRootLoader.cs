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

namespace P2XMLEditor.Parsing.Element.DemoXElementParsers;

public class DemoXElementGameRootLoader : IParser<RawGameRootData> {
	[PerformanceLogHook]
	public void ProcessFile(string filePath, List<RawGameRootData> raws) {
		using var xr = XmlReaderExtensions.InitializeFullFileReader(filePath);

		xr.MoveToContent();
		xr.ReadStartElement();
		
		while (xr.NodeType == XmlNodeType.Element) {
			var element = (XElement)XNode.ReadFrom(xr);
			var id = ulong.Parse(element.Attribute("id")!.Value);

			var scenesStructure = new Dictionary<ulong, Dictionary<ChildContainerType, ulong[]>>();
			var structureElement = element.Element("HierarchyScenesStructure.Dict");

			if (structureElement != null) {
				foreach (var item in structureElement.Elements()) {
					var key = ulong.Parse(item.Name.LocalName);
					var containers = new Dictionary<ChildContainerType, ulong[]>();
					foreach (var container in item.Elements()) {
						var typeName = container.Name.LocalName;
						if (typeName.EndsWith(".List")) {
							var type = Enum.Parse<ChildContainerType>(typeName[..^5]);
							var children = container.Elements("object")
								.Select(e => ulong.Parse(e.Attribute("id")!.Value))
								.ToArray();
							containers[type] = children;
						}
					}
					scenesStructure[key] = containers;
				}
			}

			var raw = new RawGameRootData {
				Id = id,
				FunctionalComponentIds = ParseDemoListAsUlong(element, "FunctionalComponents").ToArray(),
				EventGraphId = element.Element("EventGraph") != null ?
					ulong.Parse(element.Element("EventGraph")!.Value) : null,
				StandartParamIds = ParseDemoDictAsUlong(element, "StandartParams"),
				CustomParamIds = ParseDemoDictAsUlong(element, "CustomParams"),
				Name = element.Element("Name")!.Value,
				EventIds = ParseDemoListAsUlong(element, "Events").ToArray(),
				ChildObjectIds = ParseDemoListAsUlong(element, "ChildObjects").ToArray(),
				SampleIds = ParseDemoListAsUlong(element, "Samples").ToArray(),
				LogicMapIds = ParseDemoListAsUlong(element, "LogicMaps").ToArray(),
				GameModeIds = ParseDemoListAsUlong(element, "GameModes").ToArray(),
				BaseToEngineGuidsTable = ParseDemoDict(element, "BaseToEngineGuidsTable"),
				HierarchyScenesStructure = scenesStructure,
				HierarchyEngineGuidsTable = ParseDemoList(element, "HierarchyEngineGuidsTable").ToArray(),
				WorldObjectSaveOptimizeMode = element.Element("WorldObjectSaveOptimizeMode")?.Let(ParseBool) ?? false
			};

			raws.Add(raw);
		}
	}
}
