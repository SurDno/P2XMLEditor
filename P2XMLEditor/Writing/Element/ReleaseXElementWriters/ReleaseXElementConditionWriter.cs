using System.Linq;
using System.Xml.Linq;
using P2XMLEditor.Data;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.Helper;
using static P2XMLEditor.Helper.XmlParsingHelper;

namespace P2XMLEditor.Writing.Element.ReleaseXElementWriters;

public class ReleaseXElementConditionWriter : IReleaseXElementWriter<Condition> {
	public XElement ToXml(Condition element, WriterSettings settings) {
		var xElement = CreateBaseElement(element.Id);
		if (element.Predicates.Any())
			xElement.Add(CreateListElement("Predicates", element.Predicates.Select(p => p.Id.ToString())));
		xElement.Add(
			new XElement("Operation", element.Operation.Serialize()),
			CreateSelfClosingElement("Name", element.Name),
			new XElement("OrderIndex", element.OrderIndex)
		);
		return xElement;
	}
}
