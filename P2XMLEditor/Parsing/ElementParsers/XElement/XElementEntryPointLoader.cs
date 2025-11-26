using System.Xml.Linq;
using P2XMLEditor.Helper;
using P2XMLEditor.Parsing.RawData;
using static P2XMLEditor.Helper.XmlParsingHelper;

namespace P2XMLEditor.Parsing.ElementParsers.XElement;

public class XElementEntryPointLoader : IParser<RawEntryPointData> {
	public void ProcessFile(string filePath, List<RawEntryPointData> raws) {
		foreach (var element in XDocument.Load(filePath).Root!.Elements()) {
			var id = ulong.Parse(element.Attribute(XNameCache.IdAttribute)!.Value);

			var raw = new RawEntryPointData {
				Id = id,
				Name = element.Element(XNameCache.Name)!.Value,
				ActionLineId = element.Element(XNameCache.ActionLine) != null ?
					ulong.Parse(element.Element(XNameCache.ActionLine)!.Value) : null,
				ParentId = ulong.Parse(element.Element(XNameCache.Parent)!.Value)
			};

			raws.Add(raw);
		}
	}
}