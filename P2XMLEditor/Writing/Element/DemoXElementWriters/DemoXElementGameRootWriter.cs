using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using P2XMLEditor.Data;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.Helper;
using static P2XMLEditor.Helper.DemoXmlParsingHelper;

namespace P2XMLEditor.Writing.Element.DemoXElementWriters;

public class DemoXElementGameRootWriter : IDemoXElementWriter<GameRoot> {
	public XElement ToXml(GameRoot element, WriterSettings settings) {
		var obj = CreateDemoBaseElement(element.Id);

		obj.Add(CreateDemoListElementAsLong("FunctionalComponents", element.FunctionalComponents.Select(c => c.Id)));
		
		if (element.EventGraph != null)
			obj.Add(new XElement("EventGraph", element.EventGraph.Id));

		obj.Add(CreateDemoDictAsLong("StandartParams", element.StandartParams?.ToDictionary(k => k.Key, v => v.Value.Id)));
		obj.Add(CreateDemoDictAsLong("CustomParams", element.CustomParams?.ToDictionary(k => k.Key, v => v.Value.Id)));
		obj.Add(CreateDemoStringElement("Name", element.Name));
		obj.Add(CreateDemoListElementAsLong("Events", element.Events?.Select(e => e.Id) ?? []));
		obj.Add(CreateDemoListElementAsLong("ChildObjects", element.ChildObjects?.Select(c => c.Id) ?? []));
		obj.Add(CreateDemoListElementAsLong("Samples", element.Samples?.Select(s => s.Id) ?? []));
		obj.Add(CreateDemoListElementAsLong("LogicMaps", element.LogicMaps?.Select(l => l.Id) ?? []));
		obj.Add(CreateDemoListElementAsLong("GameModes", element.GameModes?.Select(g => g.Id) ?? []));

		if (element.BaseToEngineGuidsTable != null) {
			var dict = new XElement("BaseToEngineGuidsTable.Dict");
			foreach (var kvp in element.BaseToEngineGuidsTable) {
				dict.Add(new XElement(kvp.Key, kvp.Value));
			}
			obj.Add(dict);
		}

		if (element.HierarchyScenesStructure != null) {
			var structure = new XElement("HierarchyScenesStructure.Dict");
			foreach (var kvp in element.HierarchyScenesStructure) {
				var item = new XElement(kvp.Key.ToString());
				foreach (var container in kvp.Value) {
					var list = new XElement(container.Key + ".List");
					foreach (var childId in container.Value) {
						list.Add(new XElement("object", new XAttribute("id", childId)));
					}
					item.Add(list);
				}
				structure.Add(item);
			}
			obj.Add(structure);
		}

		obj.Add(CreateDemoListElement("HierarchyEngineGuidsTable", element.HierarchyEngineGuidsTable ?? []));

		if (!settings.RemoveDefaultValueTypes || element.WorldObjectSaveOptimizeMode) obj.Add(CreateDemoBoolElement("WorldObjectSaveOptimizeMode", element.WorldObjectSaveOptimizeMode));

		obj.Add(CreateGuidElement(element.Id));
		return obj;
	}

	private XElement CreateDemoDictAsLong(string name, Dictionary<string, ulong>? items) {
		var dictName = name + ".Dict";
		if (items == null || !items.Any()) return new XElement(dictName);
		return new XElement(dictName, items.Select(x => new XElement(x.Key, x.Value)));
	}
}
