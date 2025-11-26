using System.Xml.Linq;
using P2XMLEditor.GameData.VirtualMachineElements.Enums;
using P2XMLEditor.Helper;
using P2XMLEditor.Parsing.RawData;
using static P2XMLEditor.Helper.XmlParsingHelper;

namespace P2XMLEditor.Parsing.ElementParsers.XElement;

public class XElementMindMapNodeContentLoader : IParser<RawMindMapNodeContentData> {
	public void ProcessFile(string filePath, List<RawMindMapNodeContentData> raws) {
		foreach (var element in XDocument.Load(filePath).Root!.Elements()) {
			var id = ulong.Parse(element.Attribute(XNameCache.IdAttribute)!.Value);

			var raw = new RawMindMapNodeContentData {
				Id = id,
				ParentId = ulong.Parse(element.Element(XNameCache.Parent)!.Value),
				ContentType = element.Element(XNameCache.ContentType)!.Value,
				Number = int.Parse(element.Element(XNameCache.Number)!.Value),
				ContentDescriptionTextId = ulong.Parse(element.Element(XNameCache.ContentDescriptionText)!.Value),
				ContentPictureId = element.Element(XNameCache.ContentPicture) != null ?
					ulong.Parse(element.Element(XNameCache.ContentPicture)!.Value) : null,
				ContentConditionId = ulong.Parse(element.Element(XNameCache.ContentCondition)!.Value),
				Name = element.Element(XNameCache.Name)!.Value
			};

			raws.Add(raw);
		}
	}
}