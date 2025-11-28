using System.Collections.Generic;
using System.Xml.Linq;
using P2XMLEditor.GameData.Enums;
using P2XMLEditor.GameData.Templates.InternalTypes.Interfaces;
using P2XMLEditor.Helper;

namespace P2XMLEditor.GameData.Templates.InternalTypes.Components.Crowds;

public struct IndoorCrowdComponent() : ITemplateComponent {
    public string? RegionId { get; set; }
    public List<CrowdAreaInfo> Areas { get; } = [];

    public struct CrowdAreaInfo {
        public Area Area;
        public List<CrowdTemplateInfo> TemplateSets;
    }

    public struct CrowdTemplateInfo {
        public List<string> Templates;
        public int Min;
        public int Max;
        public float Chance;
    }

    public void LoadFromXml(XElement element) {
        var region = element.Element("Region");
        if (region != null) {
            var id = region.Element("Id")?.Value;
            RegionId = string.IsNullOrEmpty(id) ? null : id;
        }

        var areasEl = element.Element("Areas");
        if (areasEl != null) {
            foreach (var areaItem in areasEl.Elements("Item")) {
                var areaName = areaItem.Element("Area")!.Value;

                var setsEl = areaItem.Element("Templates");
                var sets = new List<CrowdTemplateInfo>();

                if (setsEl != null) {
                    foreach (var setItem in setsEl.Elements("Item")) {
                        var templatesEl = setItem.Element("Templates");
                        var templates = new List<string>();

                        if (templatesEl != null) {
                            foreach (var t in templatesEl.Elements("Item")) {
                                templates.Add(t.Element("Id")!.Value);
                            }
                        }

                        sets.Add(new CrowdTemplateInfo {
                            Templates = templates,
                            Min = setItem.Element("Min")!.ParseInt(),
                            Max = setItem.Element("Max")!.ParseInt(),
                            Chance = setItem.Element("Chance")!.ParseFloat()
                        });
                    }
                }

                Areas.Add(new CrowdAreaInfo {
                    Area = areaName.Deserialize<Area>(),
                    TemplateSets = sets
                });
            }
        }
    }

    public XElement ToXml(XElement baseElement) {
        var region = new XElement("Region");
        region.Add(new XElement("Id", RegionId ?? string.Empty));
        baseElement.Add(region);

        var areasEl = new XElement("Areas");
        foreach (var area in Areas) {
            var areaItem = new XElement("Item");
            areaItem.Add(new XElement("Area", area.Area.Serialize()));

            var setsEl = new XElement("Templates");
            foreach (var set in area.TemplateSets) {
                var setItem = new XElement("Item");

                var templatesEl = new XElement("Templates");
                foreach (var t in set.Templates) {
                    templatesEl.Add(new XElement("Item",
                        new XElement("Id", t)
                    ));
                }

                setItem.Add(templatesEl);
                setItem.Add(new XElement("Min", set.Min));
                setItem.Add(new XElement("Max", set.Max));
                setItem.Add(new XElement("Chance", set.Chance));

                setsEl.Add(setItem);
            }

            areaItem.Add(setsEl);
            areasEl.Add(areaItem);
        }

        baseElement.Add(areasEl);
        return baseElement;
    }
}
