using System.Xml;
using System.Xml.Linq;
using P2XMLEditor.Core;
using P2XMLEditor.Data;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.GameData.VirtualMachineElements.Enums;
using P2XMLEditor.GameData.VirtualMachineElements.Interfaces;
using P2XMLEditor.Helper;
using P2XMLEditor.Parsing.RawData;
using static P2XMLEditor.Helper.XmlParsingHelper;
using static P2XMLEditor.Helper.XmlReaderExtensions;

#pragma warning disable CS8618

namespace P2XMLEditor.GameData.VirtualMachineElements;

public class PartCondition(ulong id) : VmElement(id), IFiller<RawPartConditionData>, IVmCreator<PartCondition> {
    protected override HashSet<string> KnownElements { get; } =
        ["Name", "ConditionType", "FirstExpression", "SecondExpression", "OrderIndex"];
    public string? Name { get; set; }
    public ConditionType ConditionType { get; set; }
    public Expression? FirstExpression { get; set; }
    public Expression? SecondExpression { get; set; }
    public int OrderIndex { get; set; }
    
    public override XElement ToXml(WriterSettings settings) {
        var element = CreateBaseElement(Id);
        if (!settings.CleanUpNames) 
            element.Add(CreateSelfClosingElement("Name", Name));
        element.Add(new XElement("ConditionType", ConditionType.Serialize()));
        if (ConditionType is not (ConditionType.ConstTrue or ConditionType.ConstFalse) ||
             !settings.CleanUpUnusedProperties) {
            if (FirstExpression != null)
                element.Add(new XElement("FirstExpression", FirstExpression.Id));
            if (SecondExpression != null && (ConditionType != ConditionType.ValueExpression || 
                                             !settings.CleanUpUnusedProperties))
                element.Add(new XElement("SecondExpression", SecondExpression.Id));
        }

        element.Add(new XElement("OrderIndex", OrderIndex));
        return element;
    }

    public void FillFromRawData(RawPartConditionData data, VirtualMachine vm) {
        Name = data.Name;
        ConditionType = data.ConditionType.Deserialize<ConditionType>();
        OrderIndex = data.OrderIndex;
        FirstExpression = data.FirstExpressionId.HasValue
            ? vm.GetElement<Expression>(data.FirstExpressionId.Value)
            : null;
        SecondExpression = data.SecondExpressionId.HasValue
            ? vm.GetElement<Expression>(data.SecondExpressionId.Value)
            : null;
    }

    public static PartCondition New(VirtualMachine vm, ulong id, VmElement parent) => new PartCondition(id) {
        Name = "Always True",
        ConditionType = ConditionType.ConstTrue
    };

    public void OnDestroy(VirtualMachine vm) {
        vm.RemoveElement(FirstExpression);
        vm.RemoveElement(SecondExpression);
    }
}