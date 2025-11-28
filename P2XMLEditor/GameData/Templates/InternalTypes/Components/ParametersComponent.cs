using System;
using System.Collections.Generic;
using System.Xml.Linq;
using P2XMLEditor.GameData.Enums;
using P2XMLEditor.GameData.Templates.InternalTypes.Interfaces;
using P2XMLEditor.GameData.Types;
using P2XMLEditor.Logging;

namespace P2XMLEditor.GameData.Templates.InternalTypes.Components;

public struct ParametersComponent() : ITemplateComponent {
    public List<IParameter> Parameters { get; } = [];

    public void LoadFromXml(XElement element) {
        var list = element.Element("Parameters");
        if (list == null)
            return;

        foreach (var item in list.Elements("Item")) {
            try {
                var p = CreateParameterInstance(item);
                if (p == null) {
                    Logger.Log(LogLevel.Error, $"Unknown parameter skipped: {item.Attribute("type")!.Value}");
                    continue;
                }

                p.LoadFromXml(item);
                Parameters.Add(p);
            }
            catch (Exception ex) {
                Logger.Log(LogLevel.Error, $"Unknown parameter errored: {item.Attribute("type")!.Value} {ex.Message}");
            }
        }
    }

    public XElement ToXml(XElement baseElement) {
        var list = new XElement("Parameters");

        foreach (var p in Parameters)
            list.Add(p.ToXml());

        baseElement.Add(list);
        return baseElement;
    }
    
    private static IParameter? CreateParameterInstance(XElement item) => item.Attribute("type")!.Value switch {
        "BoolParameter" => new Parameter<bool>(),
        "FractionParameter" => new Parameter<FractionEnum>(),
        "ContainerOpenStateEnumParameter" => new Parameter<ContainerOpenState>(),
        "LiquidTypeParameter" => new Parameter<LiquidType>(),
        "WeaponKindParameter" => new Parameter<WeaponKind>(),
        "BlockTypeParameter" => new Parameter<BlockType>(),
        "StammKindParameter" => new Parameter<StammKind>(),
        "CombatStyleParameter" => new Parameter<CombatStyle>(),
        "FastTravelPointParameter" => new Parameter<FastTravelPoint>(),
        "BoundHealthStateParameter" => new Parameter<BoundHealthState>(),
        "FloatParameter" => new MinMaxParameter<float>(),
        "IntParameter" => new MinMaxParameter<int>(),
        "TimeSpanParameter" => new MinMaxParameter<TimeSpan>(),
        "ResetableBoolParameter" => new ResetableParameter<bool>(),
        "ResetableDetectTypeParameter" => new ResetableParameter<DetectType>(),
        "ResetableFloatParameter" => new ResetableMinMaxParameter<float>(),
        "ResetableIntParameter" => new ResetableMinMaxParameter<int>(),
        "BoolPriorityParameter" => new PriorityParameter<bool>(),
        "LockStatePriorityParameter" => new PriorityParameter<LockState>(),
        _ => null // TODO: implement XML preserving unknown parameter type class
    };
}
