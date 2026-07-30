using System.Xml.Linq;
using P2XMLEditor.Data;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.Helper;
using static P2XMLEditor.Helper.XmlParsingHelper;

namespace P2XMLEditor.Writing.Element.ReleaseXElementWriters;

public class ReleaseXElementSampleWriter : IReleaseXElementWriter<Sample> {
	public XElement ToXml(Sample element, WriterSettings settings) {
		var xElement = CreateBaseElement(element.Id);
		xElement.Add(
			new XElement("SampleType", element.SampleType.Serialize()),
			new XElement("EngineID", element.EngineId)
		);
		return EnsureFullClosingTag(xElement);
	}
}
