using System.Xml.Linq;
using P2XMLEditor.GameData.VirtualMachineElements.Enums;
using P2XMLEditor.Helper;
using P2XMLEditor.Parsing.RawData;

namespace P2XMLEditor.Parsing.ElementParsers.XElement;

public class XElementSampleLoader : IParser<RawSampleData> {
	public void ProcessFile(string filePath, List<RawSampleData> raws) {
		foreach (var element in XDocument.Load(filePath).Root!.Elements()) {
			ulong id = ulong.Parse(element.Attribute(XNameCache.IdAttribute)!.Value);

			var raw = new RawSampleData {
				Id = id,
				SampleType = element.Element(XNameCache.SampleType)!.Value.Deserialize<SampleType>(),
				EngineId = element.Element(XNameCache.EngineID)!.Value
			};

			raws.Add(raw);
		}
	}
}