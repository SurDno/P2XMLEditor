using P2XMLEditor.Enums;
using P2XMLEditor.Enums.VirtualMachine;

using P2XMLEditor.Parsing.RawData;
using static P2XMLEditor.Parsing.Helpers.XmlReaderExtensions;

namespace P2XMLEditor.Parsing.Element.XmlReaderParsers;

public class XmlReaderMindMapNodeContentLoader : IParser<RawMindMapNodeContentData> {
	public void ProcessFile(string filePath, List<RawMindMapNodeContentData> raws) {
		using var xr = InitializeFullFileReader(filePath);
		xr.SkipDeclarationAndRoot();

		while (xr.Read()) {
			if (xr.EndOfContainerReached()) break;

			var raw = new RawMindMapNodeContentData {
				Id = xr.GetIdAndEnter(),
				ContentType = xr.GetStringValueAndAdvance().Deserialize<NodeContentType>(),
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