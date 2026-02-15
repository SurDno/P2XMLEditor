using P2XMLEditor.Enums;
using P2XMLEditor.Enums.VirtualMachine;

using P2XMLEditor.Parsing.RawData;
using static P2XMLEditor.Parsing.Helpers.XmlReaderExtensions;

namespace P2XMLEditor.Parsing.Element.XmlReaderParsers;

public class XmlReaderConditionLoader : IParser<RawConditionData> {

	public void ProcessFile(string filePath, List<RawConditionData> raws) {
		using var xr = InitializeFullFileReader(filePath);
		xr.SkipDeclarationAndRoot();

		while (xr.Read()) {
			if (xr.EndOfContainerReached()) break;

			var raw = new RawConditionData {
				Id = xr.GetIdAndEnter(),
				PredicateIds = xr.GetULongListAndAdvance(),
				Operation = xr.GetStringValueAndAdvance().Deserialize<ConditionOperation>(),
				Name = xr.GetOptionalStringValueAndAdvance(),
				OrderIndex = xr.GetIntValueAndAdvance()
			};

			raws.Add(raw);
		}
	}
}