using System.Xml.Linq;
using P2XMLEditor.Helper;
using P2XMLEditor.Parsing.RawData;

namespace P2XMLEditor.Parsing.ElementParsers.XElement;

public class XElementReplyLoader : IParser<RawReplyData> {
	public void ProcessFile(string filePath, List<RawReplyData> raws) {
		foreach (var element in XDocument.Load(filePath).Root!.Elements()) {
			ulong id = ulong.Parse(element.Attribute(XNameCache.IdAttribute)!.Value);

			var raw = new RawReplyData {
				Id = id,
				Name = element.Element(XNameCache.Name)!.Value,
				TextId = ulong.Parse(element.Element(XNameCache.Text)!.Value),
				OnlyOnce = element.Element(XNameCache.OnlyOnce) != null
					? bool.Parse(element.Element(XNameCache.OnlyOnce)!.Value)
					: null,
				OnlyOneReply = element.Element(XNameCache.OnlyOneReply) != null
					? bool.Parse(element.Element(XNameCache.OnlyOneReply)!.Value)
					: null,
				Default = element.Element(XNameCache.Default) != null
					? bool.Parse(element.Element(XNameCache.Default)!.Value)
					: null,
				EnableConditionId = element.Element(XNameCache.EnableCondition) != null
					? ulong.Parse(element.Element(XNameCache.EnableCondition)!.Value)
					: null,
				ActionLineId = element.Element(XNameCache.ActionLine) != null
					? ulong.Parse(element.Element(XNameCache.ActionLine)!.Value)
					: null,
				OrderIndex = int.Parse(element.Element(XNameCache.OrderIndex)!.Value),
				ParentId = ulong.Parse(element.Element(XNameCache.Parent)!.Value)
			};

			raws.Add(raw);
		}
	}
}