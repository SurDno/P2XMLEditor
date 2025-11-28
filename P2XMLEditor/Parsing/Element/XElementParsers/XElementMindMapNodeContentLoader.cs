using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;
using P2XMLEditor.GameData.VirtualMachineElements.Enums;
using P2XMLEditor.Helper;
using P2XMLEditor.Parsing.RawData;

namespace P2XMLEditor.Parsing.Element.XElementParsers;

public class XElementMindMapNodeContentLoader : IParser<RawMindMapNodeContentData> {
	public void ProcessFile(string filePath, List<RawMindMapNodeContentData> raws) {
		using var xr = XmlReaderExtensions.InitializeFullFileReader(filePath);

		xr.MoveToContent();
		xr.ReadStartElement();
		
		while (xr.NodeType == XmlNodeType.Element) {
			var element = (XElement)XNode.ReadFrom(xr);
			var id = ulong.Parse(element.Attribute(XNameCache.IdAttribute)!.Value);

			var raw = new RawMindMapNodeContentData {
				Id = id,
				ParentId = ulong.Parse(element.Element(XNameCache.Parent)!.Value),
				ContentType = element.Element(XNameCache.ContentType)!.Value.Deserialize<NodeContentType>(),
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