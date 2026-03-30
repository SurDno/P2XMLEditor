using System.Xml.Linq;
using P2XMLEditor.Data;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using static P2XMLEditor.Helper.XmlParsingHelper;

namespace P2XMLEditor.Writing.Element.ReleaseXElementWriters;

public class ReleaseXElementGameObjectWriter : ReleaseXElementParameterHolderWriter<GameObject> {
    public override XElement ToXml(GameObject element, WriterSettings settings) {
        var xElement = base.ToXml(element, settings);
        
        // Reverse order here since we're using AddFirst.
        if (element.Instantiated != null)
           xElement.AddFirst(CreateBoolElement("Instantiated", (bool)element.Instantiated));
        if (element.EngineBaseTemplateId != null)
            xElement.AddFirst(CreateSelfClosingElement("EngineBaseTemplateID", element.EngineBaseTemplateId));
        if (element.EngineTemplateId != null)
            xElement.AddFirst(CreateSelfClosingElement("EngineTemplateID", element.EngineTemplateId));
        if (element.WorldPositionGuid != null)
            xElement.AddFirst(CreateSelfClosingElement("WorldPositionGuid", element.WorldPositionGuid));
        return xElement;
    }
}
