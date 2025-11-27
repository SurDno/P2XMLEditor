using System.Xml.Linq;
using P2XMLEditor.Helper;
using P2XMLEditor.Parsing.RawData;
using static P2XMLEditor.Helper.XmlParsingHelper;

namespace P2XMLEditor.Parsing.Element.XElementParsers;

public class XElementSpeechLoader : IParser<RawSpeechData> {
    public void ProcessFile(string filePath, List<RawSpeechData> raws) {
        using var xr = XmlReaderExtensions.InitializeFullFileReader(filePath);

        xr.MoveToContent();
        xr.ReadStartElement();
		
        while (xr.NodeType == System.Xml.XmlNodeType.Element) {
            var element = (System.Xml.Linq.XElement)XNode.ReadFrom(xr);
            ulong id = ulong.Parse(element.Attribute(XNameCache.IdAttribute)!.Value);

            var raw = new RawSpeechData {
                Id = id,
                ReplyIds = element.Element(XNameCache.Replyes) != null
                    ? ReadULongList(element.Element(XNameCache.Replyes)!)
                    : [],
                TextId = ulong.Parse(element.Element(XNameCache.Text)!.Value),
                AuthorGuidId = ulong.Parse(element.Element(XNameCache.AuthorGuid)!.Value),
                OnlyOnce = element.Element(XNameCache.OnlyOnce) != null
                    ? bool.Parse(element.Element(XNameCache.OnlyOnce)!.Value)
                    : null,
                IsTrade = element.Element(XNameCache.IsTrade) != null
                    ? bool.Parse(element.Element(XNameCache.IsTrade)!.Value)
                    : null,
                EntryPointIds = element.Element(XNameCache.EntryPoints) != null
                    ? ReadULongList(element.Element(XNameCache.EntryPoints)!)
                    : [],
                IgnoreBlock = element.Element(XNameCache.IgnoreBlock) != null
                    ? bool.Parse(element.Element(XNameCache.IgnoreBlock)!.Value)
                    : null,
                OwnerId = ulong.Parse(element.Element(XNameCache.Owner)!.Value),
                InputLinkIds = element.Element(XNameCache.InputLinks) != null
                    ? ReadULongList(element.Element(XNameCache.InputLinks)!)
                    : null,
                OutputLinkIds = element.Element(XNameCache.OutputLinks) != null
                    ? ReadULongList(element.Element(XNameCache.OutputLinks)!)
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
