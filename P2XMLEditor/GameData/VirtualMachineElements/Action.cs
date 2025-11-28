using System.Collections.Generic;
using System.Xml.Linq;
using P2XMLEditor.Core;
using P2XMLEditor.Data;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.GameData.VirtualMachineElements.Enums;
using P2XMLEditor.GameData.VirtualMachineElements.Interfaces;
using P2XMLEditor.Helper;
using P2XMLEditor.Parsing.RawData;
using static P2XMLEditor.Helper.XmlParsingHelper;

#pragma warning disable CS8618

namespace P2XMLEditor.GameData.VirtualMachineElements;

public class Action(ulong id) : VmElement(id), IFiller<RawActionData> {
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

    public void FillFromRawData(RawActionData data, VirtualMachine vm) {
        ActionType = data.ActionType;
        MathOperationType = data.MathOperationType;
        TargetFuncName = data.TargetFuncName;
        SourceExpression = data.SourceExpressionId.HasValue ? 
            vm.GetElement<Expression>(data.SourceExpressionId.Value) : null;
        TargetObject = data.TargetObject;
        TargetParam = data.TargetParam;
        SourceParams = data.SourceParams;
        Name = data.Name;
        LocalContext = vm.GetElement<State, Graph, Branch, Talking, Speech>(data.LocalContextId);
        OrderIndex = data.OrderIndex;
    }
        
    public void OnDestroy(VirtualMachine vm) {
        if (SourceExpression != null)
            vm.RemoveElement(SourceExpression);
    }
}