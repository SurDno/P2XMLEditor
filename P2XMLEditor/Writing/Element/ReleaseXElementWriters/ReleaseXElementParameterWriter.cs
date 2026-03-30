using System.Xml.Linq;
using P2XMLEditor.Data;
using P2XMLEditor.GameData.VirtualMachineElements;
using static P2XMLEditor.Helper.XmlParsingHelper;

namespace P2XMLEditor.Writing.Element.ReleaseXElementWriters;

public class ReleaseXElementParameterWriter : IReleaseXElementWriter<Parameter> {
	public XElement ToXml(Parameter element, WriterSettings settings) {
		var xElement = CreateBaseElement(element.Id);
		xElement.Add(
			CreateSelfClosingElement("Name", element.Name)
		);

		var ownerComponent = element.FindOwnerComponent();
		if (ownerComponent != null)
			xElement.Add(new XElement("OwnerComponent", ownerComponent.Id));
		xElement.Add(
			new XElement("Type", element.Type),
			CreateSelfClosingElement("Value", element.Value)
		);
		if (element.Implicit != null)
			xElement.Add(CreateBoolElement("Implicit", (bool)element.Implicit));
		xElement.Add(new XElement("Parent", element.Parent.Id)); 
		xElement.Add(CreateBoolElement("Custom", element.IsCustom()));
		return xElement;
	}
}
