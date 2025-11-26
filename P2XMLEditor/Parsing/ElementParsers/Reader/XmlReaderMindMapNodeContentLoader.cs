using System.Xml;
using P2XMLEditor.Helper;
using P2XMLEditor.Logging;
using P2XMLEditor.Parsing.RawData;
using static P2XMLEditor.Helper.XmlReaderExtensions;

namespace P2XMLEditor.Parsing.ElementParsers.Reader;

public class XmlReaderMindMapNodeContentLoader : IParser<RawMindMapNodeContentData> {
	[PerformanceLogHook]
	public void ProcessFile(string filePath, List<RawMindMapNodeContentData> raws) {
		using var xr = InitializeFullFileReader(filePath);
		xr.SkipDeclarationAndRoot();

		while (xr.Read()) {
			if (xr.EndOfContainerReached()) break;

			var raw = new RawMindMapNodeContentData {
				Id = xr.GetIdAndEnter(),
				ContentType = xr.GetStringValueAndAdvance(),
				Number = xr.GetIntValueAndAdvance(),
				ContentDescriptionTextId = xr.GetULongValueAndAdvance(),
				ContentPictureId = xr.Name == "ContentPicture" ? xr.GetULongValueAndAdvance() : null,
				ContentConditionId = xr.GetULongValueAndAdvance(),
				Name = xr.GetOptionalStringValueAndAdvance(),
				ParentId = xr.GetULongValueAndAdvance()
			};

			raws.Add(raw);
		}
	}
}