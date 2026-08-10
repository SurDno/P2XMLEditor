using System.Xml.Linq;
using P2XMLEditor.Data;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.Helper;
using static P2XMLEditor.Helper.DemoXmlParsingHelper;

namespace P2XMLEditor.Writing.Element.AlphaXElementWriters;

public class AlphaXElementParameterWriter : IAlphaXElementWriter<Parameter> {
	public XElement ToXml(Parameter element, WriterSettings settings) {
		var obj = new XElement("object");

		obj.Add(CreateDemoStringElement("Name", element.Name));
		// The owner component by name, which is how the demo records it — the id form the release
		// uses has no place here. Absent for a custom parameter, exactly as FindOwnerComponent
		// answers null for one, matching the real files.
		if (!settings.StripNames)
			obj.Add(CreateDemoStringElement("ComponentName", element.FindOwnerComponent()?.Name));

		var styledType = element.Type;
		if (element.Type.StartsWith("System"))
			styledType += "%";
		obj.Add(CreateDemoStringElement("Type", styledType));
		obj.Add(CreateDemoStringElement("Value", element.SerializedValue));

		if (!settings.RemoveDefaultValueTypes || element.Implicit) obj.Add(CreateDemoBoolElement("Implicit", element.Implicit));

		obj.Add(new XElement("Parent", element.Parent.Id));
		// ParamType is the demo's Custom flag. It has to be written for the value to survive a
		// reload — the loader reads custom-ness back from here — so it is not gated on StripNames.
		obj.Add(new XElement("ParamType", element.IsCustom() ? "PARAM_TYPE_CUSTOM" : "PARAM_TYPE_STANDART"));
		obj.Add(CreateGuidElement(element.Id));

		return obj;
	}
}
