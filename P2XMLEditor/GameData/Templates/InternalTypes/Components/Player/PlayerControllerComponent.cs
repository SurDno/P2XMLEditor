using System.Collections.Generic;
using System.Xml.Linq;
using P2XMLEditor.GameData.Enums;
using P2XMLEditor.GameData.Templates.InternalTypes.Interfaces;
using P2XMLEditor.Helper;
using static P2XMLEditor.Helper.XmlParsingHelper;

namespace P2XMLEditor.GameData.Templates.InternalTypes.Components.Player;

public struct PlayerControllerComponent() : ITemplateComponent {
    public List<ReputationEntry> Reputations { get; } = [];
    public float ThresholdNearRegionsPositive { get; set; }
    public float ThresholdNearRegionsNegative { get; set; }
    public float CoefficientNearRegionsPositive { get; set; }
    public float CoefficientNearRegionsNegative { get; set; }
    public float ThresholdPlayerInfected { get; set; }
    public float ThresholdRegionInfected { get; set; }
    public List<FractionEnum> DangerousFractions { get; } = [];

    public struct ReputationEntry {
        public ActionEnum Action;
        public List<FractionEnum> Fractions;
        public float Visible;
        public float Invisible;
        public bool AffectNearRegions;
    }

    public void LoadFromXml(XElement element) {
        var repsEl = element.Element("Reputations");
        if (repsEl != null) {
            foreach (var item in repsEl.Elements("Item")) {
                var action = item.Element("Action")!.Value.Deserialize<ActionEnum>();

                var fractions = new List<FractionEnum>();
                var frEl = item.Element("Fractions");
                if (frEl != null) {
                    foreach (var f in frEl.Elements("Item")) {
                        fractions.Add(f.Value.Deserialize<FractionEnum>());
                    }
                }

                var rep = new ReputationEntry {
                    Action = action,
                    Fractions = fractions,
                    Visible = item.Element("Visible")!.ParseFloat(),
                    Invisible = (item.Element("Invisible")!).ParseFloat(),
                    AffectNearRegions = ParseBool(item.Element("AffectNearRegions")!)
                };

                Reputations.Add(rep);
            }
        }

        ThresholdNearRegionsPositive = (element.Element("ThresholdNearRegionsPositive")!).ParseFloat();
        ThresholdNearRegionsNegative = (element.Element("ThresholdNearRegionsNegative")!).ParseFloat();
        CoefficientNearRegionsPositive = (element.Element("CoefficientNearRegionsPositive")!).ParseFloat();
        CoefficientNearRegionsNegative = (element.Element("CoefficientNearRegionsNegative")!).ParseFloat();
        ThresholdPlayerInfected = (element.Element("ThresholdPlayerInfected")!).ParseFloat();
        ThresholdRegionInfected = (element.Element("ThresholdRegionInfected")!).ParseFloat();

        var dfEl = element.Element("DangerousFractions");
        if (dfEl == null) return;
        foreach (var f in dfEl.Elements("Item"))
            DangerousFractions.Add(f.Value.Deserialize<FractionEnum>());
    }

    public XElement ToXml(XElement baseElement) {
        var repsEl = new XElement("Reputations");
        foreach (var rep in Reputations) {
            var item = new XElement("Item");
            item.Add(new XElement("Action", rep.Action.Serialize()));
            var frEl = new XElement("Fractions");
            foreach (var f in rep.Fractions) 
                frEl.Add(new XElement("Item", f.Serialize()));
            item.Add(frEl);
            item.Add(new XElement("Visible", rep.Visible));
            item.Add(new XElement("Invisible", rep.Invisible));
            item.Add(new XElement("AffectNearRegions", rep.AffectNearRegions));
            repsEl.Add(item);
        }
        baseElement.Add(repsEl);

        baseElement.Add(new XElement("ThresholdNearRegionsPositive", ThresholdNearRegionsPositive));
        baseElement.Add(new XElement("ThresholdNearRegionsNegative", ThresholdNearRegionsNegative));
        baseElement.Add(new XElement("CoefficientNearRegionsPositive", CoefficientNearRegionsPositive));
        baseElement.Add(new XElement("CoefficientNearRegionsNegative", CoefficientNearRegionsNegative));
        baseElement.Add(new XElement("ThresholdPlayerInfected", ThresholdPlayerInfected));
        baseElement.Add(new XElement("ThresholdRegionInfected", ThresholdRegionInfected));

        var dfEl = new XElement("DangerousFractions");
        foreach (var f in DangerousFractions)
            dfEl.Add(new XElement("Item", f.Serialize()));
        baseElement.Add(dfEl);

        return baseElement;
    }
}
