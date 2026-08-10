using System.Xml.Linq;
using P2XMLEditor.Data;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.Helper;
using static P2XMLEditor.Helper.DemoXmlParsingHelper;

namespace P2XMLEditor.Writing.Element.AlphaXElementWriters;

public class AlphaXElementSampleWriter : IAlphaXElementWriter<Sample> {
	public XElement ToXml(Sample element, WriterSettings settings) {
		var obj = new XElement("object");

		obj.Add(
			new XElement("SampleType", element.SampleType.Serialize()),
			new XElement("EngineID", element.EngineId),
			CreateGuidElement(element.Id)
		);

		return obj;
	}
}
