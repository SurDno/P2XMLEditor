using System.Xml.Linq;
using P2XMLEditor.Helper;
using P2XMLEditor.Parsing.RawData;

namespace P2XMLEditor.Parsing.Element.XElementParsers;

public class XElementCustomTypeLoader : IParser<RawCustomTypeData> {
	public void ProcessFile(string filePath, List<RawCustomTypeData> raws) {
		if (!File.Exists(filePath)) return;
		using var xr = XmlReaderExtensions.InitializeFullFileReader(filePath);

		xr.MoveToContent();
		xr.ReadStartElement();
		
		while (xr.NodeType == System.Xml.XmlNodeType.Element) {
			var element = (System.Xml.Linq.XElement)XNode.ReadFrom(xr);
			var id = ulong.Parse(element.Attribute(XNameCache.IdAttribute)!.Value);

			var raw = new RawCustomTypeData {
				Id = id,
				Name = element.Element(XNameCache.Name)!.Value,
				ParentId = ulong.Parse(element.Element(XNameCache.Parent)!.Value)
			};

			raws.Add(raw);
		}
	}
}