using System.Xml.Linq;
using P2XMLEditor.GameData.Templates.InternalTypes.Interfaces;
using static P2XMLEditor.Helper.XmlParsingHelper;

namespace P2XMLEditor.GameData.Templates.InternalTypes.Components;

public struct HerbRootsComponent() : ITemplateComponent {
    public struct TemplateEntry {
        public string TemplateId;
        public int Weight;
    }

    public List<TemplateEntry> Templates { get; } = [];
    public int HerbsBudget { get; set; }
    public int HerbsCountMin { get; set; }
    public int HerbsCountMax { get; set; }
    public int HerbsGrowTimeInMinutesMin { get; set; }
    public int HerbsGrowTimeInMinutesMax { get; set; }

    public void LoadFromXml(XElement element) {
        var templatesEl = element.Element("Templates");
        if (templatesEl != null) {
            foreach (var item in templatesEl.Elements("Item")) {
                var entry = new TemplateEntry {
                    TemplateId = item.Element("Template")!.Element("Id")!.Value,
                    Weight = item.Element("Weight")!.ParseInt()
                };
                Templates.Add(entry);
            }
        }

        HerbsBudget = element.Element("HerbsBudget")!.ParseInt();
        HerbsCountMin = element.Element("HerbsCountMin")!.ParseInt();
        HerbsCountMax = element.Element("HerbsCountMax")!.ParseInt();
        HerbsGrowTimeInMinutesMin = element.Element("HerbsGrowTimeInMinutesMin")!.ParseInt();
        HerbsGrowTimeInMinutesMax = element.Element("HerbsGrowTimeInMinutesMax")!.ParseInt();
    }

    public XElement ToXml(XElement baseElement) {
        var t = new XElement("Templates");
        foreach (var entry in Templates) {
            var it = new XElement("Item");
            it.Add(new XElement("Template", new XElement("Id", entry.TemplateId)));
            it.Add(new XElement("Weight", entry.Weight));
            t.Add(it);
        }

        baseElement.Add(t);

        baseElement.Add(new XElement("HerbsBudget", HerbsBudget));
        baseElement.Add(new XElement("HerbsCountMin", HerbsCountMin));
        baseElement.Add(new XElement("HerbsCountMax", HerbsCountMax));
        baseElement.Add(new XElement("HerbsGrowTimeInMinutesMin", HerbsGrowTimeInMinutesMin));
        baseElement.Add(new XElement("HerbsGrowTimeInMinutesMax", HerbsGrowTimeInMinutesMax));

        return baseElement;
    }
}