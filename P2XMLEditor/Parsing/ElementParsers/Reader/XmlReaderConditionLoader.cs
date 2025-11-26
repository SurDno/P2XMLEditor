using System.Xml;
using P2XMLEditor.GameData.VirtualMachineElements.Enums;
using P2XMLEditor.Helper;
using P2XMLEditor.Logging;
using P2XMLEditor.Parsing.RawData;
using static P2XMLEditor.Helper.XmlReaderExtensions;

namespace P2XMLEditor.Parsing.ElementParsers.Reader;

public class XmlReaderConditionLoader : IParser<RawConditionData> {
	[PerformanceLogHook]
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