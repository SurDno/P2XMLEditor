using System.Xml.Linq;
using P2XMLEditor.Core;
using P2XMLEditor.Data;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.GameData.VirtualMachineElements.Enums;
using P2XMLEditor.Helper;
using static P2XMLEditor.Helper.XmlParsingHelper;

#pragma warning disable CS8618

namespace P2XMLEditor.GameData.VirtualMachineElements;

public class Action(string id) : VmElement(id) {
    protected override HashSet<string> KnownElements { get; } = [
        "ActionType", "MathOperationType", "TargetFuncName", "TargetObject", "TargetParam",
        "SourceParams", "Name", "LocalContext", "OrderIndex", "SourceExpression"
    ];

    public ActionType ActionType { get; set; }
    public MathOperationType MathOperationType { get; set; }
    public string TargetFuncName { get; set; }
    public Expression? SourceExpression { get; set; }
    public string TargetObject { get; set; }
    public string TargetParam { get; set; }
    public List<string>? SourceParams { get; set; }
    public string Name { get; set; }
    public VmEither<State, Graph, Branch, Talking, Speech> LocalContext { get; set; }
    public int OrderIndex { get; set; }

    private record RawActionData(string Id, string ActionType, string MathOperationType, string TargetFuncName,
        string? SourceExpressionId, string TargetObject, string TargetParam, List<string>? SourceParams, string Name, 
        string LocalContextId, int OrderIndex) : RawData(Id);

    public override XElement ToXml(WriterSettings settings) {
        var element = CreateBaseElement(Id);
        
        element.Add(
            new XElement("ActionType", ActionType.Serialize()),
            new XElement("MathOperationType", MathOperationType.Serialize()),
            CreateSelfClosingElement("TargetFuncName", TargetFuncName)
        );
        if (SourceExpression != null) 
            element.Add(new XElement("SourceExpression", SourceExpression.Id));
        element.Add(
            new XElement("TargetObject", TargetObject),
            new XElement("TargetParam", TargetParam)
        );
        if (SourceParams?.Count > 0)
            element.Add(CreateListElement("SourceParams", SourceParams));
        element.Add(
            CreateSelfClosingElement("Name", Name),
            new XElement("LocalContext", LocalContext.Id),
            new XElement("OrderIndex", OrderIndex)
        );
        return element;
    }

    protected override RawData CreateRawData(XElement element) {
        return new RawActionData(
            element.Attribute("id")?.Value ?? throw new ArgumentException("Id missing"),
            GetRequiredElement(element, "ActionType").Value,
            GetRequiredElement(element, "MathOperationType").Value,
            GetRequiredElement(element, "TargetFuncName").Value,
            element.Element("SourceExpression")?.Value,
            GetRequiredElement(element, "TargetObject").Value,
            GetRequiredElement(element, "TargetParam").Value,
            ParseListElement(element, "SourceParams"),
            GetRequiredElement(element, "Name").Value,
            GetRequiredElement(element, "LocalContext").Value,
            GetRequiredElement(element, "OrderIndex").ParseInt()
        );
    }

    public override void FillFromRawData(RawData rawData, VirtualMachine vm) {
        if (rawData is not RawActionData data)
            throw new ArgumentException($"Expected RawActionData but got {rawData.GetType()}");

        ActionType = data.ActionType.Deserialize<ActionType>();
        MathOperationType = data.MathOperationType.Deserialize<MathOperationType>();
        TargetFuncName = data.TargetFuncName;
        SourceExpression = data.SourceExpressionId != null ? vm.GetElement<Expression>(data.SourceExpressionId): null;
        TargetObject = data.TargetObject;
        TargetParam = data.TargetParam;
        SourceParams = data.SourceParams;
        Name = data.Name;
        LocalContext = vm.GetElement<State, Graph, Branch, Talking, Speech>(data.LocalContextId);
        OrderIndex = data.OrderIndex;
    }
    
    protected override VmElement New(VirtualMachine vm, string id, VmElement parent) => throw new NotImplementedException();
        
    public override void OnDestroy(VirtualMachine vm) {
        if (SourceExpression != null)
            vm.RemoveElement(SourceExpression);
    }
}