using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;
using P2XMLEditor.Helper;
using P2XMLEditor.Parsing.RawData;
using static P2XMLEditor.Helper.XmlParsingHelper;

namespace P2XMLEditor.Parsing.Element.XElementParsers;

public class XElementGameModeLoader : IParser<RawGameModeData> {
    public void ProcessFile(string filePath, List<RawGameModeData> raws) {
        using var xr = XmlReaderExtensions.InitializeFullFileReader(filePath);

        xr.MoveToContent();
        xr.ReadStartElement();
		
        while (xr.NodeType == XmlNodeType.Element) {
            var element = (XElement)XNode.ReadFrom(xr);
            var id = ulong.Parse(element.Attribute(XNameCache.IdAttribute)!.Value);

            var raw = new RawGameModeData {
                Id = id,
                IsMain = element.Element(XNameCache.IsMain)?.Let(ParseBool),
                StartGameTime = ParseTimeSpanString(element.Element(XNameCache.StartGameTime)!.Value),
                GameTimeSpeed = float.Parse(element.Element(XNameCache.GameTimeSpeed)!.Value),
                StartSolarTime = ParseTimeSpanString(element.Element(XNameCache.StartSolarTime)!.Value),
                SolarTimeSpeed = float.Parse(element.Element(XNameCache.SolarTimeSpeed)!.Value),
                PlayerRef = element.Element(XNameCache.PlayerRef)!.Value,
                Name = element.Element(XNameCache.Name)!.Value,
                ParentId = ulong.Parse(element.Element(XNameCache.Parent)!.Value)
            };

            raws.Add(raw);
        }
    }
}