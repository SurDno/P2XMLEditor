using System.Linq;
using System.Xml.Linq;
using P2XMLEditor.Data;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.Helper;
using static P2XMLEditor.Helper.XmlParsingHelper;

namespace P2XMLEditor.Writing.Element.ReleaseXElementWriters;

public class ReleaseXElementActionLineWriter : IReleaseXElementWriter<ActionLine> {
	public XElement ToXml(ActionLine element, WriterSettings settings) {
		var xElement = CreateBaseElement(element.Id);
		if (element.Actions is not null && element.Actions.Count != 0)
			xElement.Add(CreateListElementUnsorted("Actions", element.Actions.Select(a => a.Id.ToString())));
		
		xElement.Add(new XElement("ActionLineType", element.ActionLineType.Serialize()));
		
		if (element.LoopInfo != null) {
			var actionLineInfo = new XElement("ActionLoopInfo",
				CreateSelfClosingElement("Name", element.LoopInfo.Name.Write()),
				new XElement("Start", element.LoopInfo.Start.Write()),
				new XElement("End", element.LoopInfo.End.Write())
			);
			if (!settings.RemoveDefaultValueTypes || element.LoopInfo.Random)
				actionLineInfo.Add(CreateBoolElement("Random", element.LoopInfo.Random));
			xElement.Add(actionLineInfo);
		}
		
		if (!settings.StripNames)
			xElement.Add(CreateSelfClosingElement("Name", element.Name));
		xElement.Add(
			new XElement("LocalContext", element.LocalContext.Id)
		);
		
		if (!settings.RemoveDefaultValueTypes || element.OrderIndex != 0)
			xElement.Add(new XElement("OrderIndex", element.OrderIndex));
		return EnsureFullClosingTag(xElement);
	}
}
