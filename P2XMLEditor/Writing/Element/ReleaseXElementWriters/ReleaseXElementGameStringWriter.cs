using System.Xml.Linq;
using P2XMLEditor.Data;
using P2XMLEditor.GameData.VirtualMachineElements;
using static P2XMLEditor.Helper.XmlParsingHelper;

namespace P2XMLEditor.Writing.Element.ReleaseXElementWriters;

public class ReleaseXElementGameStringWriter : IReleaseXElementWriter<GameString> {
	public XElement ToXml(GameString element, WriterSettings settings) {
		var xElement = CreateBaseElement(element.Id);
		xElement.Add(new XElement("Parent", element.Parent.Id));
		return EnsureFullClosingTag(xElement);
	}
}
