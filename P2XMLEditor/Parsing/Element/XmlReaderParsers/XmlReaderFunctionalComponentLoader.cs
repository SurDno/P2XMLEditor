using P2XMLEditor.Helper;
using P2XMLEditor.Logging;
using P2XMLEditor.Parsing.RawData;
using static P2XMLEditor.Helper.XmlReaderExtensions;

namespace P2XMLEditor.Parsing.Element.XmlReaderParsers;

public class XmlReaderFunctionalComponentLoader : IParser<RawFunctionalComponentData> {
	[PerformanceLogHook]
	public void ProcessFile(string filePath, List<RawFunctionalComponentData> raws) {
		using var xr = InitializeFullFileReader(filePath);
		xr.SkipDeclarationAndRoot();

		while (xr.Read()) {
			if (xr.EndOfContainerReached()) break;

			var raw = new RawFunctionalComponentData {
				Id = xr.GetIdAndEnter(),
				EventIds = xr.Name == "Events" ? xr.GetULongListAndAdvance() : null,
				Main = xr.Name == "Main" ? xr.GetBoolValueAndAdvance() : null,
				LoadPriority = xr.GetLongValueAndAdvance(),
				Name = xr.GetStringValueAndAdvance(),
				ParentId = xr.GetULongValueAndAdvance()
			};
			raws.Add(raw);
		}
	}
}