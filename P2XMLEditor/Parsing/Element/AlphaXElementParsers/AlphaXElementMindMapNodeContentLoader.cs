using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;
using P2XMLEditor.GameData.VirtualMachineElements.Enums;
using P2XMLEditor.Helper;
using P2XMLEditor.Logging;
using P2XMLEditor.Parsing.Helpers;
using P2XMLEditor.Parsing.RawData;

namespace P2XMLEditor.Parsing.Element.AlphaXElementParsers;

public class AlphaXElementMindMapNodeContentLoader : IParser<RawMindMapNodeContentData> {
	
	[PerformanceLogHook]
	public void ProcessFile(string filePath, List<RawMindMapNodeContentData> raws) {
		using var xr = AlphaFormat.OpenReader(filePath);

		xr.MoveToContent();
		xr.ReadStartElement();
		
		while (xr.NodeType == XmlNodeType.Element) {
			var element = (XElement)XNode.ReadFrom(xr);
			var id = ulong.Parse(element.Element("Guid")!.Value);

			var raw = new RawMindMapNodeContentData {
				Id = id,
				ParentId = ulong.Parse(element.Element("Parent")!.Value),
				ContentType = element.Element("ContentType")!.Value.Deserialize<NodeContentType>(),
				Number = int.Parse(element.Element("Number")!.Value),
				ContentDescriptionTextId = ulong.Parse(element.Element("ContentDescriptionText")!.Value),
				ContentPictureId = element.Element("ContentPicture") != null ?
					ulong.Parse(element.Element("ContentPicture")!.Value) : null,
				ContentConditionId = ulong.Parse(element.Element("ContentCondition")!.Value), 
				Name = element.Element("Name") != null ? element.Element("Name")!.Value : string.Empty
			};

			raws.Add(raw);
		}
	}
}
