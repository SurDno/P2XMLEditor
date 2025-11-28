using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes;
using P2XMLEditor.Helper;
using P2XMLEditor.Parsing.RawData;
using static P2XMLEditor.Helper.XmlParsingHelper;

namespace P2XMLEditor.Parsing.Element.XElementParsers;

public class XElementGraphLoader : IParser<RawGraphData> {
    public void ProcessFile(string filePath, List<RawGraphData> raws) {
        using var xr = XmlReaderExtensions.InitializeFullFileReader(filePath);

        xr.MoveToContent();
        xr.ReadStartElement();
		
        while (xr.NodeType == XmlNodeType.Element) {
            var element = (XElement)XNode.ReadFrom(xr);
            var id = ulong.Parse(element.Attribute(XNameCache.IdAttribute)!.Value);

            var paramInfos = new List<GraphParamInfo>();
            var inputParamsElement = element.Element(XNameCache.InputParamsInfo);
            if (inputParamsElement != null) {
                foreach (var item in inputParamsElement.Elements(XNameCache.Item)) {
                    paramInfos.Add(new(
                        item.Element(XNameCache.Name)!.Value,
                        item.Element(XNameCache.Type)!.Value
                    ));
                }
            }

            var raw = new RawGraphData {
                Id = id,
                StateIds = ParseListElementAsUlong(element, XNameCache.States),
                EventLinkIds = ParseListElementAsUlong(element, XNameCache.EventLinks),
                GraphType = element.Element(XNameCache.GraphType)!.Value,
                EntryPointIds = ParseListElementAsUlong(element, XNameCache.EntryPoints),
                IgnoreBlock = element.Element(XNameCache.IgnoreBlock)?.Let(ParseBool),
                OwnerId = ulong.Parse(element.Element(XNameCache.Owner)!.Value),
                InputParamsInfo = paramInfos.Count > 0 ? paramInfos : null,
                InputLinkIds = ParseListElementAsUlong(element, XNameCache.InputLinks),
                OutputLinkIds = ParseListElementAsUlong(element, XNameCache.OutputLinks),
                Initial = element.Element(XNameCache.Initial)?.Let(ParseBool),
                Name = element.Element(XNameCache.Name)!.Value,
                ParentId = ulong.Parse(element.Element(XNameCache.Parent)!.Value),
                SubstituteGraphId = element.Element(XNameCache.SubstituteGraph) != null ?
                    ulong.Parse(element.Element(XNameCache.SubstituteGraph)!.Value) : null
            };

            raws.Add(raw);
        }
    }
}