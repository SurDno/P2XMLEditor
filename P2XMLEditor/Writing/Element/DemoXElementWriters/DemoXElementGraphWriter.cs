using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using P2XMLEditor.Data;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.Helper;
using static P2XMLEditor.Helper.DemoXmlParsingHelper;

namespace P2XMLEditor.Writing.Element.DemoXElementWriters;

public class DemoXElementGraphWriter : IDemoXElementWriter<Graph> {
	public XElement ToXml(Graph element, WriterSettings settings) {
		var obj = CreateDemoBaseElement(element.Id);

		obj.Add(CreateDemoListElementAsLong("States", element.States.Select(s => s.Id)));
		obj.Add(CreateDemoListElementAsLong("EventLinks", element.EventLinks.Select(l => l.Id)));
		
		obj.Add(new XElement("GraphType", element.GraphType));
		obj.Add(CreateDemoListElementAsLong("EntryPoints", element.EntryPoints.Select(e => e.Id)));

		if (element.IgnoreBlock.HasValue)
			obj.Add(CreateDemoBoolElement("IgnoreBlock", element.IgnoreBlock.Value));

		obj.Add(new XElement("Owner", element.Owner.Id));

		if (element.InputParamsInfo?.Any() == true) {
			var list = new XElement("InputParamsInfo.List");
			for (var i = 0; i < element.InputParamsInfo.Count; i++) {
				var info = element.InputParamsInfo[i];
				var itemObj = new XElement("object", new XAttribute("id", i));
				itemObj.Add(
					new XElement("Name", info.Name),
					new XElement("Type", info.Type),
					CreateGuidElement((ulong)i)
				);
				list.Add(itemObj);
			}
			obj.Add(list);
		}

		obj.Add(
			CreateDemoListElementAsLong("InputLinks", element.InputLinks?.Select(l => l.Id) ?? []),
			CreateDemoListElementAsLong("OutputLinks", element.OutputLinks?.Select(l => l.Id) ?? [])
		);

		if (element.Initial.HasValue)
			obj.Add(CreateDemoBoolElement("Initial", element.Initial.Value));

		obj.Add(
			CreateDemoStringElement("Name", element.Name),
			new XElement("Parent", element.Parent.Id)
		);

		if (element.SubstituteGraph != null)
			obj.Add(new XElement("SubstituteGraph", element.SubstituteGraph.Value.Id));

		obj.Add(CreateGuidElement(element.Id));
		return obj;
	}
}
