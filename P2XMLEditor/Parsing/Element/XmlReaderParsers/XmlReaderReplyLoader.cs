using P2XMLEditor.Helper;
using P2XMLEditor.Logging;
using P2XMLEditor.Parsing.RawData;
using static P2XMLEditor.Helper.XmlReaderExtensions;

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
				OnlyOnce = xr.Name == "OnlyOnce" ? xr.GetBoolValueAndAdvance() : null,
				OnlyOneReply = xr.Name == "OnlyOneReply" ? xr.GetBoolValueAndAdvance() : null,
				Default = xr.Name == "Default" ? xr.GetBoolValueAndAdvance() : null,
				EnableConditionId = xr.Name == "EnableCondition" ? xr.GetULongValueAndAdvance() : null,
				ActionLineId = xr.Name == "ActionLine" ? xr.GetULongValueAndAdvance() : null,
				OrderIndex = xr.GetIntValueAndAdvance(),
				ParentId = xr.GetULongValueAndAdvance()
			};

			raws.Add(raw);
		}
	}
}