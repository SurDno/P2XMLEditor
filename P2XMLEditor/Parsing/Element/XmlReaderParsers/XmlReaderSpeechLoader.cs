using P2XMLEditor.Helper;
using P2XMLEditor.Logging;
using P2XMLEditor.Parsing.RawData;
using static P2XMLEditor.Helper.XmlReaderExtensions;

namespace P2XMLEditor.Parsing.Element.XmlReaderParsers;

public class XmlReaderSpeechLoader : IParser<RawSpeechData> {
	[PerformanceLogHook]
	public void ProcessFile(string filePath, List<RawSpeechData> raws) {
		using var xr = InitializeFullFileReader(filePath);
		xr.SkipDeclarationAndRoot();

		while (xr.Read()) {
			if (xr.EndOfContainerReached()) break;

			var raw = new RawSpeechData {
				Id = xr.GetIdAndEnter(),
				ReplyIds = xr.GetULongListAndAdvance(),
				TextId = xr.GetULongValueAndAdvance(),
				AuthorGuidId = xr.GetULongValueAndAdvance(),
				OnlyOnce = xr.Name == "OnlyOnce" ? xr.GetBoolValueAndAdvance() : null,
				IsTrade = xr.Name == "IsTrade" ? xr.GetBoolValueAndAdvance() : null,
				EntryPointIds = xr.GetULongListAndAdvance(),
				IgnoreBlock = xr.Name == "IgnoreBlock" ? xr.GetBoolValueAndAdvance() : null,
				OwnerId = xr.GetULongValueAndAdvance(),
				InputLinkIds = xr.Name == "InputLinks" ? xr.GetULongListAndAdvance() : null,
				OutputLinkIds = xr.Name == "OutputLinks" ? xr.GetULongListAndAdvance() : null,
				Initial = xr.Name == "Initial" ? xr.GetBoolValueAndAdvance() : null,
				Name = xr.GetOptionalStringValueAndAdvance(),
				ParentId = xr.GetULongValueAndAdvance()
			};

			raws.Add(raw);
		}
	}
}