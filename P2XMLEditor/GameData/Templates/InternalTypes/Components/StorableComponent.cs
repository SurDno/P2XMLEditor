using System.Collections.Generic;
using System.Xml.Linq;
using P2XMLEditor.GameData.Enums;
using P2XMLEditor.GameData.Templates.InternalTypes.Interfaces;
using P2XMLEditor.Helper;
using static P2XMLEditor.Helper.XmlParsingHelper;

// TODO: store PlaceholderId as GUID or direct class ref
namespace P2XMLEditor.GameData.Templates.InternalTypes.Components;

public struct StorableComponent() : ITemplateComponent {

    public bool IsEnabled { get; set; }
    public string PlaceholderId { get; set; }

    public List<StorableGroup> Groups { get; } = [];

    public List<DurabilityTooltip> DurabilityTooltips { get; } = [];
    public List<SimpleTooltip> SimpleTooltips { get; } = [];

    public struct DurabilityTooltip {
        public bool IsEnabled;
        public TooltipName Name;
        public ParameterName Parameter;
        public bool IsFood;
    }

    public struct SimpleTooltip {
        public bool IsEnabled;
        public TooltipName Name;
        public string Value;
        public string Color;
    }

    public void LoadFromXml(XElement element) {
        IsEnabled = ParseBool(element.Element("IsEnabled")!);
        PlaceholderId = element.Element("Placeholder")!.Element("Id")!.Value;

        var groupsEl = element.Element("Groups");
        if (groupsEl != null) {
            foreach (var item in groupsEl.Elements("Item")) {
                Groups.Add(item.Value.Deserialize<StorableGroup>());
            }
        }

        var tooltipsEl = element.Element("Tooltips");
        if (tooltipsEl != null) {
            foreach (var item in tooltipsEl.Elements("Item")) {
                var type = item.Attribute("type")!.Value;

                if (type == "StorableTooltipItemDurability") {
                    var tt = new DurabilityTooltip {
                        IsEnabled = ParseBool(item.Element("IsEnabled")),
                        Name = item.Element("Name")!.Value.Deserialize<TooltipName>(),
                        Parameter = item.Element("Parameter")!.Value.Deserialize<ParameterName>(),
                        IsFood = ParseBool(item.Element("IsFood"))
                    };
                    DurabilityTooltips.Add(tt);
                }
                else if (type == "StorableTooltipSimple") {
                    var info = item.Element("Info")!;
                    var tt = new SimpleTooltip {
                        IsEnabled = ParseBool(item.Element("IsEnabled")),
                        Name = info.Element("Name")!.Value.Deserialize<TooltipName>(),
                        Value = info.Element("Value")!.Value,
                        Color = info.Element("Color")!.Value
                    };
                    SimpleTooltips.Add(tt);
                }
            }
        }
    }


    public XElement ToXml(XElement baseElement) {
        baseElement.Add(CreateBoolElement("IsEnabled", IsEnabled));
        baseElement.Add(new XElement("Placeholder", new XElement("Id", PlaceholderId)));

        var g = new XElement("Groups");
        foreach (var grp in Groups) g.Add(new XElement("Item", grp.Serialize()));
        baseElement.Add(g);

        var t = new XElement("Tooltips");

        foreach (var tip in DurabilityTooltips) {
            var it = new XElement("Item");
            it.SetAttributeValue("type", "StorableTooltipItemDurability");
            it.Add(CreateBoolElement("IsEnabled", tip.IsEnabled));
            it.Add(new XElement("Name", tip.Name.Serialize()));
            it.Add(new XElement("Parameter", tip.Parameter.Serialize()));
            it.Add(CreateBoolElement("IsFood", tip.IsFood));
            t.Add(it);
        }

        foreach (var tip in SimpleTooltips) {
            var it = new XElement("Item");
            it.SetAttributeValue("type", "StorableTooltipSimple");
            it.Add(CreateBoolElement("IsEnabled", tip.IsEnabled));

            it.Add(new XElement("Info",
                new XElement("Name", tip.Name.Serialize()),
                new XElement("Value", tip.Value),
                new XElement("Color", tip.Color)
            ));

            t.Add(it);
        }

        baseElement.Add(t);
        return baseElement;
    }
}