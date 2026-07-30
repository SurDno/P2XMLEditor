using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;
using P2XMLEditor.Logging;
using P2XMLEditor.Parsing.Helpers;
using P2XMLEditor.Parsing.RawData;
using static P2XMLEditor.Parsing.Helpers.XElementExtensions;

namespace P2XMLEditor.Parsing.Element.XElementParsers;

public class XElementSpeechLoader : IParser<RawSpeechData> {
	[PerformanceLogHook]
	public void ProcessFile(string filePath, List<RawSpeechData> raws) {
		using var xr = XmlReaderExtensions.InitializeFullFileReader(filePath);

		xr.MoveToContent();
		xr.ReadStartElement();
		
		while (xr.NodeType == XmlNodeType.Element) {
			var element = (XElement)XNode.ReadFrom(xr);
			var id = ulong.Parse(element.Attribute(XNameCache.IdAttribute)!.Value);

			var raw = new RawSpeechData {
				Id = id,
				ReplyIds = element.Element(XNameCache.Replyes) != null
					? ReadULongList(element.Element(XNameCache.Replyes)!).ToArray()
					: [],
				TextId = ulong.Parse(element.Element(XNameCache.Text)!.Value),
				AuthorGuidId = ulong.Parse(element.Element(XNameCache.AuthorGuid)!.Value),
				OnlyOnce = element.Element(XNameCache.OnlyOnce) != null
					? bool.Parse(element.Element(XNameCache.OnlyOnce)!.Value)
					: false,
				IsTrade = element.Element(XNameCache.IsTrade) != null
					? bool.Parse(element.Element(XNameCache.IsTrade)!.Value)
					: false,
				EntryPointIds = element.Element(XNameCache.EntryPoints) != null
					? ReadULongList(element.Element(XNameCache.EntryPoints)!).ToArray()
					: [],
				IgnoreBlock = element.Element(XNameCache.IgnoreBlock) != null
					? bool.Parse(element.Element(XNameCache.IgnoreBlock)!.Value)
					: false,
				OwnerId = ulong.Parse(element.Element(XNameCache.Owner)!.Value),
				InputLinkIds = element.Element(XNameCache.InputLinks) != null
					? ReadULongList(element.Element(XNameCache.InputLinks)!).ToArray()
					: null,
				OutputLinkIds = element.Element(XNameCache.OutputLinks) != null
					? ReadULongList(element.Element(XNameCache.OutputLinks)!).ToArray()
					: null,
				Initial = element.Element(XNameCache.Initial) != null
					? bool.Parse(element.Element(XNameCache.Initial)!.Value)
					: false,
				Name = element.Element(XNameCache.Name)!.Value,
				ParentId = ulong.Parse(element.Element(XNameCache.Parent)!.Value)
			};

			raws.Add(raw);
		}
	}
}
