using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using P2XMLEditor.Data;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.Helper;
using static P2XMLEditor.Helper.DemoXmlParsingHelper;

namespace P2XMLEditor.Writing.Element.AlphaXElementWriters;

public class AlphaXElementGraphWriter : IAlphaXElementWriter<Graph> {
	public XElement ToXml(Graph element, WriterSettings settings) {
		var obj = new XElement("object");

		obj.Add(CreateDemoListElementAsLong("States", element.States.Select(s => s.Id)));
		obj.Add(CreateDemoListElementAsLong("EventLinks", element.EventLinks.Select(l => l.Id)));
		
		obj.Add(new XElement("GraphType", element.GraphType));
		obj.Add(CreateDemoListElementAsLong("EntryPoints", element.EntryPoints.Select(e => e.Id)));

		if (!settings.RemoveDefaultValueTypes || element.IgnoreBlock) obj.Add(CreateDemoBoolElement("IgnoreBlock", element.IgnoreBlock));

		obj.Add(new XElement("Owner", element.Owner.Id));

		if (element.InputParams?.Any() == true) {
			var list = new XElement("InputParamsInfo.List");
			for (var i = 0; i < element.InputParams.Count; i++) {
				var info = element.InputParams[i];
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

		if (!settings.RemoveDefaultValueTypes || element.Initial) obj.Add(CreateDemoBoolElement("Initial", element.Initial));

		obj.Add(
			settings.StripNames ? null : CreateDemoStringElement("Name", element.Name),
			new XElement("Parent", element.Parent.Id)
		);

		if (element.SubstituteGraph != null)
			obj.Add(new XElement("SubstituteGraph", element.SubstituteGraph.Value.Id));

		obj.Add(CreateGuidElement(element.Id));
		return obj;
	}
}
