using System.Xml.Linq;
using P2XMLEditor.Data;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.Helper;
using static P2XMLEditor.Helper.DemoXmlParsingHelper;

namespace P2XMLEditor.Writing.Element.DemoXElementWriters;

public class DemoXElementParameterWriter : IDemoXElementWriter<Parameter> {
	public XElement ToXml(Parameter element, WriterSettings settings) {
		var obj = CreateDemoBaseElement(element.Id);


		var styledType = element.Type;
		if (element.Type.StartsWith("System"))
			styledType += "%";
		obj.Add(CreateDemoStringElement("Type", styledType));
		obj.Add(CreateDemoStringElement("Value", element.SerializedValue));

		if (element.Implicit.HasValue)
			obj.Add(CreateDemoBoolElement("Implicit", element.Implicit.Value));
		
		obj.Add(
			CreateDemoStringElement("Name", element.Name),
			new XElement("Parent", element.Parent.Id),
			CreateGuidElement(element.Id)
		);

		return obj;
	}
}
