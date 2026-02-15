using P2XMLEditor.Enums;
using P2XMLEditor.Enums.VirtualMachine;

using P2XMLEditor.Parsing.RawData;
using static P2XMLEditor.Parsing.Helpers.XmlReaderExtensions;

namespace P2XMLEditor.Parsing.Element.XmlReaderParsers;

public class XmlReaderSampleLoader : IParser<RawSampleData> {

	public void ProcessFile(string filePath, List<RawSampleData> raws) {
		using var xr = InitializeFullFileReader(filePath);
		xr.SkipDeclarationAndRoot();

		while (xr.Read()) {
			if (xr.EndOfContainerReached()) break;
			var raw = new RawSampleData {
				Id = xr.GetIdAndEnter(),
				SampleType = xr.GetStringValueAndAdvance().Deserialize<SampleType>(),
				EngineId = xr.GetStringValueAndAdvance()
			};
			raws.Add(raw);
		}
	}
}