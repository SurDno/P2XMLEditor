using System.Collections.Generic;
using P2XMLEditor.Logging;
using P2XMLEditor.Parsing.RawData;
using static P2XMLEditor.Parsing.Helpers.XmlReaderExtensions;

namespace P2XMLEditor.Parsing.Element.XmlReaderParsers;

public class XmlReaderReplyLoader : IParser<RawReplyData> {

	[PerformanceLogHook]
	public void ProcessFile(string filePath, List<RawReplyData> raws) {
		using var xr = InitializeFullFileReader(filePath);
		xr.SkipDeclarationAndRoot();

		while (xr.Read()) {
			if (xr.EndOfContainerReached()) break;

			var raw = new RawReplyData {
				Id = xr.GetIdAndEnter(),
				Name = xr.GetStringValueAndAdvance(),
				TextId = xr.GetULongValueAndAdvance(),
				OnlyOnce = xr.Name == "OnlyOnce" ? xr.GetBoolValueAndAdvance() : false,
				OnlyOneReply = xr.Name == "OnlyOneReply" ? xr.GetBoolValueAndAdvance() : false,
				Default = xr.Name == "Default" ? xr.GetBoolValueAndAdvance() : false,
				EnableConditionId = xr.Name == "EnableCondition" ? xr.GetULongValueAndAdvance() : null,
				ActionLineId = xr.Name == "ActionLine" ? xr.GetULongValueAndAdvance() : null,
				OrderIndex = xr.Name == "OrderIndex" ? xr.GetIntValueAndAdvance() : 0,
				ParentId = xr.GetULongValueAndAdvance()
			};

			raws.Add(raw);
		}
	}
}
