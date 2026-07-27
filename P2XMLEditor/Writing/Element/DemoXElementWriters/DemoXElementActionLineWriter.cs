using System.Linq;
using System.Xml.Linq;
using P2XMLEditor.Data;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.Helper;
using static P2XMLEditor.Helper.DemoXmlParsingHelper;

namespace P2XMLEditor.Writing.Element.DemoXElementWriters;

public class DemoXElementActionLineWriter : IDemoXElementWriter<ActionLine> {
	public XElement ToXml(ActionLine element, WriterSettings settings) {
		var obj = CreateDemoBaseElement(element.Id);

		// 1. Actions.List goes FIRST
		obj.Add(CreateDemoListElementAsLong("Actions", element.Actions?.Select(a => a.Id) ?? []));

		// 2. Comments.List is always present (self-closing) between Actions.List and ActionLineType
		obj.Add(new XElement("Comments.List"));

		obj.Add(new XElement("ActionLineType", element.ActionLineType.Serialize()));

		if (element.LoopInfo != null) {
			var loop = new XElement("ActionLoopInfo",
				CreateDemoStringElement("Name", element.LoopInfo.Name.Write()),
				new XElement("Start", element.LoopInfo.Start.Write()),
				new XElement("End", element.LoopInfo.End.Write())
			);
			if (element.LoopInfo.Random.HasValue)
				loop.Add(CreateDemoBoolElement("Random", element.LoopInfo.Random.Value));
			obj.Add(loop);
		}

		obj.Add(
			CreateDemoStringElement("Name", element.Name),
			new XElement("LocalContext", element.LocalContext.Id),
			new XElement("OrderIndex", element.OrderIndex),
			// Always present per Demo format
			new XElement("Enabled", "True"),
			CreateGuidElement(element.Id)
		);

		return obj;
	}
}
