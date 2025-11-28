using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.GameData.VirtualMachineElements.Enums;
using P2XMLEditor.Helper;
using P2XMLEditor.Parsing.RawData;
using static P2XMLEditor.Helper.XmlParsingHelper;

namespace P2XMLEditor.Parsing.Element.XElementParsers;

public class XElementEventLoader : IParser<RawEventData> {
    public void ProcessFile(string filePath, List<RawEventData> raws) {
        using var xr = XmlReaderExtensions.InitializeFullFileReader(filePath);

        xr.MoveToContent();
        xr.ReadStartElement();
		
        while (xr.NodeType == XmlNodeType.Element) {
            var element = (XElement)XNode.ReadFrom(xr);
            var id = ulong.Parse(element.Attribute(XNameCache.IdAttribute)!.Value);

            var messagesInfo = new List<MessageInfo>();
            var messagesElement = element.Element(XNameCache.MessagesInfo);
            if (messagesElement != null) {
                foreach (var item in messagesElement.Elements(XNameCache.Item)) {
                    messagesInfo.Add(new(
                        item.Element(XNameCache.Name)!.Value,
                        item.Element(XNameCache.Type)!.Value
                    ));
                }
            }

            var raw = new RawEventData {
                Id = id,
                EventTime = ParseTimeSpanString(element.Element(XNameCache.EventTime)!.Value),
                Manual = element.Element(XNameCache.Manual)?.Let(ParseBool),
                EventRaisingType = element.Element(XNameCache.EventRaisingType)!.Value.Deserialize<EventRaisingType>(),
                ChangeTo = element.Element(XNameCache.ChangeTo)?.Let(ParseBool),
                Repeated = element.Element(XNameCache.Repeated)?.Let(ParseBool),
                Name = element.Element(XNameCache.Name)!.Value,
                ParentId = ulong.Parse(element.Element(XNameCache.Parent)!.Value),
                EventParameterId = element.Element(XNameCache.EventParameter) != null ?
                    ulong.Parse(element.Element(XNameCache.EventParameter)!.Value) : null,
                ConditionId = element.Element(XNameCache.Condition) != null ?
                    ulong.Parse(element.Element(XNameCache.Condition)!.Value) : null,
                MessagesInfo = messagesInfo.Count > 0 ? messagesInfo : null
            };

            raws.Add(raw);
        }
    }
}