using System.Xml.Linq;
using P2XMLEditor.Data;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.Helper;
using static P2XMLEditor.Helper.DemoXmlParsingHelper;

namespace P2XMLEditor.Writing.Element.DemoXElementWriters;

public class DemoXElementGameStringWriter : IDemoXElementWriter<GameString> {
	public XElement ToXml(GameString element, WriterSettings settings) {
		var obj = CreateDemoBaseElement(element.Id);

		var dict = new XElement("language_strings.Dict");
		foreach (var lang in settings.Languages) {
			dict.Add(new XElement(lang, element.GetText(lang)));
		}
		obj.Add(dict);

		obj.Add(
			new XElement("Parent", element.Parent.Id),
			CreateGuidElement(element.Id)
		);

		return obj;
	}
}
