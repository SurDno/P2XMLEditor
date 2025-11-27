using System.Xml.Linq;
using P2XMLEditor.Helper;
using P2XMLEditor.Parsing.RawData;

namespace P2XMLEditor.Parsing.Element.XElementParsers;

public class XElementGameStringLoader : IParser<RawGameStringData> {
	public void ProcessFile(string filePath, List<RawGameStringData> raws) {
		using var xr = XmlReaderExtensions.InitializeFullFileReader(filePath);

		xr.MoveToContent();
		xr.ReadStartElement();
		
		while (xr.NodeType == System.Xml.XmlNodeType.Element) {
			var element = (System.Xml.Linq.XElement)XNode.ReadFrom(xr);
			var id = ulong.Parse(element.Attribute(XNameCache.IdAttribute)!.Value);

			var raw = new RawGameStringData {
				Id = id,
				ParentId = ulong.Parse(element.Element(XNameCache.Parent)!.Value)
			};

			raws.Add(raw);
		}
	}
}