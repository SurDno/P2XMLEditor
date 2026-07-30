using System.Collections.Generic;
using P2XMLEditor.GameData.VirtualMachineElements.Enums;
using P2XMLEditor.Helper;
using P2XMLEditor.Logging;
using P2XMLEditor.Parsing.RawData;
using static P2XMLEditor.Parsing.Helpers.XmlReaderExtensions;

namespace P2XMLEditor.Parsing.Element.XmlReaderParsers;

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
				Operation = xr.Name == "Operation" ? xr.GetStringValueAndAdvance().Deserialize<ConditionOperation>() : ConditionOperation.And,
				Name = xr.GetOptionalStringValueAndAdvance(),
				OrderIndex = xr.Name == "OrderIndex" ? xr.GetIntValueAndAdvance() : 0
			};

			raws.Add(raw);
		}
	}
}
