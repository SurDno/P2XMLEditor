using System.Xml.Linq;
using P2XMLEditor.GameData.VirtualMachineElements.Enums;
using P2XMLEditor.Helper;
using P2XMLEditor.Parsing.RawData;
using static P2XMLEditor.Helper.XmlParsingHelper;

namespace P2XMLEditor.Parsing.ElementParsers.XElement;

public class XElementMindMapNodeLoader : IParser<RawMindMapNodeData> {
	public void ProcessFile(string filePath, List<RawMindMapNodeData> raws) {
		foreach (var element in XDocument.Load(filePath).Root!.Elements()) {
			var id = ulong.Parse(element.Attribute(XNameCache.IdAttribute)!.Value);

			var raw = new RawMindMapNodeData {
				Id = id,
				Name = element.Element(XNameCache.Name)!.Value,
				ParentId = ulong.Parse(element.Element(XNameCache.Parent)!.Value),
				LogicMapNodeType = element.Element(XNameCache.LogicMapNodeType)!.Value,
				ContentIds = ParseListElementAsUlong(element, XNameCache.NodeContent),
				InputLinkIds = ParseListElementAsUlong(element, XNameCache.InputLinks),
				OutputLinkIds = ParseListElementAsUlong(element, XNameCache.OutputLinks),
				GameScreenPosX = float.Parse(element.Element(XNameCache.GameScreenPosX)!.Value),
				GameScreenPosY = float.Parse(element.Element(XNameCache.GameScreenPosY)!.Value)
			};

			raws.Add(raw);
		}
	}
}