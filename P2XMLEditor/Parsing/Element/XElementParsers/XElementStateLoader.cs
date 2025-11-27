using System.Xml.Linq;
using P2XMLEditor.Helper;
using P2XMLEditor.Parsing.RawData;

namespace P2XMLEditor.Parsing.Element.XElementParsers;

public class XElementStateLoader : IParser<RawStateData> {
	public void ProcessFile(string filePath, List<RawStateData> raws) {
		using var xr = XmlReaderExtensions.InitializeFullFileReader(filePath);

		xr.MoveToContent();
		xr.ReadStartElement();
		
		while (xr.NodeType == System.Xml.XmlNodeType.Element) {
			var element = (System.Xml.Linq.XElement)XNode.ReadFrom(xr);
			ulong id = ulong.Parse(element.Attribute(XNameCache.IdAttribute)!.Value);

			var raw = new RawStateData {
				Id = id,
				EntryPointIds = element.Element(XNameCache.EntryPoints) != null
					? XmlParsingHelper.ReadULongList(element.Element(XNameCache.EntryPoints)!)
					: new List<ulong>(),
				IgnoreBlock = element.Element(XNameCache.IgnoreBlock) != null
					? bool.Parse(element.Element(XNameCache.IgnoreBlock)!.Value)
					: null,
				OwnerId = ulong.Parse(element.Element(XNameCache.Owner)!.Value),
				InputLinkIds = element.Element(XNameCache.InputLinks) != null
					? XmlParsingHelper.ReadULongList(element.Element(XNameCache.InputLinks)!)
					: null,
				OutputLinkIds = element.Element(XNameCache.OutputLinks) != null
					? XmlParsingHelper.ReadULongList(element.Element(XNameCache.OutputLinks)!)
					: null,
				Initial = element.Element(XNameCache.Initial) != null
					? bool.Parse(element.Element(XNameCache.Initial)!.Value)
					: null,
				Name = element.Element(XNameCache.Name)!.Value,
				ParentId = ulong.Parse(element.Element(XNameCache.Parent)!.Value)
			};

			raws.Add(raw);
		}
	}
}