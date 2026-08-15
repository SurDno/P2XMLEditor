using System.Xml.Linq;
using P2XMLEditor.Data;
using P2XMLEditor.GameData.VirtualMachineElements;
using static P2XMLEditor.Helper.XmlParsingHelper;

namespace P2XMLEditor.Writing.Element.ReleaseXElementWriters;

public class ReleaseXElementEntryPointWriter : IReleaseXElementWriter<EntryPoint> {
	public XElement ToXml(EntryPoint element, WriterSettings settings) {
		var xElement = CreateBaseElement(element.Id);
		if (!settings.StripNames)
			xElement.Add(new XElement("Name", element.Name));
		if (element.ActionLine != null)
			xElement.Add(new XElement("ActionLine", element.ActionLine.Id));
		if (!settings.StripEditorOnlyTags && element.Parent.HasValue)
			xElement.Add(new XElement("Parent", element.Parent.Value.Id));
		return EnsureFullClosingTag(xElement);
	}
}
