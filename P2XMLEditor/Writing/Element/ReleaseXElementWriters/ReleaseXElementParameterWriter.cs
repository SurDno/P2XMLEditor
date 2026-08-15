using System.Xml.Linq;
using P2XMLEditor.Data;
using P2XMLEditor.GameData.VirtualMachineElements;
using static P2XMLEditor.Helper.XmlParsingHelper;

namespace P2XMLEditor.Writing.Element.ReleaseXElementWriters;

public class ReleaseXElementParameterWriter : IReleaseXElementWriter<Parameter> {
	public XElement ToXml(Parameter element, WriterSettings settings) {
		var xElement = CreateBaseElement(element.Id);
		xElement.Add(CreateSelfClosingElement("Name", element.Name));

		var ownerComponent = element.FindOwnerComponent();
		if (ownerComponent != null)
			xElement.Add(new XElement("OwnerComponent", ownerComponent.Id));
		xElement.Add(
			new XElement("Type", element.Type),
			CreateSelfClosingElement("Value", element.SerializedValue)
		);
		if (!settings.RemoveDefaultValueTypes || element.Implicit)
			xElement.Add(CreateBoolElement("Implicit", element.Implicit));
		if (element.Parent != null) xElement.Add(new XElement("Parent", element.Parent.Value.Id)); 
		if (!settings.RemoveDefaultValueTypes || element.IsCustom())
			xElement.Add(CreateBoolElement("Custom", element.IsCustom()));
		return EnsureFullClosingTag(xElement);
	}
}
