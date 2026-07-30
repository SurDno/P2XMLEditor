using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using P2XMLEditor.Data;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.Helper;
using static P2XMLEditor.Helper.DemoXmlParsingHelper;

namespace P2XMLEditor.Writing.Element.DemoXElementWriters;

public class DemoXElementConditionWriter : IDemoXElementWriter<Condition> {
	public XElement ToXml(Condition element, WriterSettings settings) {
		var obj = CreateDemoBaseElement(element.Id);

		obj.Add(CreateDemoListElementAsLong("Predicates", element.Predicates.Select(p => p.Id)));
		
		obj.Add(
			new XElement("Operation", element.Operation.Serialize()),
			settings.StripNames ? null : CreateDemoStringElement("Name", element.Name),
			new XElement("OrderIndex", element.OrderIndex),
			CreateGuidElement(element.Id)
		);

		return obj;
	}
}
