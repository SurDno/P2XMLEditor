using System.Linq;
using System.Xml.Linq;
using P2XMLEditor.Data;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.Helper;
using static P2XMLEditor.Helper.XmlParsingHelper;

namespace P2XMLEditor.Writing.Element.ReleaseXElementWriters;

public class ReleaseXElementActionLineWriter : IReleaseXElementWriter<ActionLine> {
    public XElement ToXml(ActionLine element, WriterSettings settings) {
        var xElement = CreateBaseElement(element.Id);
        if (element.Actions is not null && element.Actions.Count != 0)
            xElement.Add(CreateListElement("Actions", element.Actions.Select(a => a.Id.ToString())));
        
        xElement.Add(new XElement("ActionLineType", element.ActionLineType.Serialize()));
        
        if (element.LoopInfo != null) {
            var actionLineInfo = new XElement("ActionLoopInfo",
                CreateSelfClosingElement("Name", element.LoopInfo.Value.Name),
                new XElement("Start", element.LoopInfo.Value.Start),
                new XElement("End", element.LoopInfo.Value.End)
            );
            if (element.LoopInfo.Value.Random != null)
                actionLineInfo.Add(CreateBoolElement("Random", (bool)element.LoopInfo.Value.Random!));
            xElement.Add(actionLineInfo);
        }
        
        xElement.Add(
            CreateSelfClosingElement("Name", element.Name),
            new XElement("LocalContext", element.LocalContext.Id),
            new XElement("OrderIndex", element.OrderIndex)
        );
        return xElement;
    }
}
