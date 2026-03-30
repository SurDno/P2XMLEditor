using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using P2XMLEditor.Data;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.Helper;
using static P2XMLEditor.Helper.DemoXmlParsingHelper;

namespace P2XMLEditor.Writing.Element.DemoXElementWriters;

public class DemoXElementBlueprintWriter : IDemoXElementWriter<Blueprint> {
	public XElement ToXml(Blueprint element, WriterSettings settings) {
		var obj = CreateDemoBaseElement(element.Id);

		if (element.Static.HasValue)
			obj.Add(CreateDemoBoolElement("Static", element.Static.Value));

		obj.Add(CreateDemoListElementAsLong("FunctionalComponents", element.FunctionalComponents?.Select(c => c.Id) ?? []));
		
		if (element.EventGraph != null)
			obj.Add(new XElement("EventGraph", element.EventGraph.Id));

		obj.Add(CreateDemoDictAsLong("StandartParams", element.StandartParams?.ToDictionary(k => k.Key, v => v.Value.Id)));
		obj.Add(CreateDemoDictAsLong("CustomParams", element.CustomParams?.ToDictionary(k => k.Key, v => v.Value.Id)));
		
		obj.Add(CreateDemoStringElement("GameTimeContext", element.GameTimeContext));
		obj.Add(CreateDemoStringElement("Name", element.Name));
		
		if (element.Parent != null)
			obj.Add(new XElement("Parent", element.Parent.Id));

		obj.Add(CreateDemoListElement("InheritanceInfo", element.InheritanceInfo ?? []));
		obj.Add(CreateDemoListElementAsLong("Events", element.Events?.Select(e => e.Id) ?? []));
		obj.Add(CreateDemoListElementAsLong("ChildObjects", element.ChildObjects?.Select(c => c.Id) ?? []));
		
		obj.Add(CreateGuidElement(element.Id));
		return obj;
	}

	private XElement CreateDemoDictAsLong(string name, Dictionary<string, ulong>? items) {
		var dictName = name + ".Dict";
		if (items == null || !items.Any()) return new XElement(dictName);
		return new XElement(dictName, items.Select(x => new XElement(x.Key, x.Value)));
	}
}
