using System.Xml.Linq;
using P2XMLEditor.Core;
using P2XMLEditor.Data;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.GameData.VirtualMachineElements.Enums;
using P2XMLEditor.Helper;
using static P2XMLEditor.Helper.XmlParsingHelper;

#pragma warning disable CS8618

namespace P2XMLEditor.GameData.VirtualMachineElements;

public class PartCondition(string id) : VmElement(id) {
    protected override HashSet<string> KnownElements { get; } =
        ["Name", "ConditionType", "FirstExpression", "SecondExpression", "OrderIndex"];

    public string? Name { get; set; }
    public ConditionType ConditionType { get; set; }
    public Expression? FirstExpression { get; set; }
    public Expression? SecondExpression { get; set; }
    public int OrderIndex { get; set; }
    
    private record RawPartConditionData(string Id, string? Name, string ConditionType, string? FirstExpressionId,
        string? SecondExpressionId, int OrderIndex) : RawData(Id);

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

    protected override RawData CreateRawData(XElement element) {
        return new RawPartConditionData(
            element.Attribute("id")?.Value ?? throw new ArgumentException("Id missing"),
            element.Element("Name")?.Value,
            GetRequiredElement(element, "ConditionType").Value,
            element.Element("FirstExpression")?.Value,
            element.Element("SecondExpression")?.Value,
            GetRequiredElement(element, "OrderIndex").ParseInt()
        );
    }

    public override void FillFromRawData(RawData rawData, VirtualMachine vm) {
        if (rawData is not RawPartConditionData data)
            throw new ArgumentException($"Expected RawPartConditionData but got {rawData.GetType()}");

        Name = data.Name;
        ConditionType = data.ConditionType.Deserialize<ConditionType>();
        OrderIndex = data.OrderIndex;
        FirstExpression = data.FirstExpressionId != null ? vm.GetElement<Expression>(data.FirstExpressionId) : null;
        SecondExpression = data.SecondExpressionId != null ? vm.GetElement<Expression>(data.SecondExpressionId) : null;
    }
    
    protected override VmElement New(VirtualMachine vm, string id, VmElement parent) => new PartCondition(id) {
        Name = "Always True",
        ConditionType = ConditionType.ConstTrue
    };

    public override void OnDestroy(VirtualMachine vm) {
        vm.RemoveElement(FirstExpression);
        vm.RemoveElement(SecondExpression);
    }
}