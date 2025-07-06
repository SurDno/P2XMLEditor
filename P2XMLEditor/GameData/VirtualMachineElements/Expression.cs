using System.Xml.Linq;
using P2XMLEditor.Core;
using P2XMLEditor.Data;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.GameData.VirtualMachineElements.Enums;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;
using P2XMLEditor.Helper;
using static P2XMLEditor.Helper.XmlParsingHelper;

#pragma warning disable CS8618

namespace P2XMLEditor.GameData.VirtualMachineElements;

public class Expression(string id) : VmElement(id) {
    protected override HashSet<string> KnownElements { get; } = [
        "ExpressionType", "TargetFunctionName", "TargetObject", "TargetParam",
        "Const", "SourceParams", "LocalContext", "Inversion", "FormulaChilds", "FormulaOperations"
    ];

    public ExpressionType ExpressionType { get; set; }
    public CommonVariable TargetObject;
    public CommonVariable? TargetParam;
    
    public Parameter? Const { get; set; }
    public VmEither<Branch, Event, MindMapNode, Speech, State> LocalContext { get; set; }
    public bool? Inversion { get; set; }
    public List<Expression>? FormulaChilds { get; set; } 
    public List<FormulaOperation>? FormulaOperations { get; set; }

    public VmFunction? Function;

    private record RawExpressionData(string Id, string ExpressionType, string? TargetFunctionName, string TargetObject,
        string? TargetParam, string? Const, List<string>? SourceParams, string LocalContext, bool? Inversion,
        List<string>? FormulaChilds, List<string>? FormulaOperations) : RawData(Id);

    public override XElement ToXml(WriterSettings settings) {
        var element = CreateBaseElement(Id);

        element.Add(new XElement("ExpressionType", ExpressionType.Serialize()));
        if (Function != null)
            element.Add(CreateSelfClosingElement("TargetFunctionName", Function.Name));
        element.Add( new XElement("TargetObject", TargetObject.Write()));
        
        if (TargetParam != null)
            element.Add(new XElement("TargetParam", TargetParam.Write()));
        if (Function?.GetParamStrings() is { Count: > 0 } sourceParams) 
            element.Add(CreateListElement("SourceParams", sourceParams));
        if (Const != null)
            element.Add(new XElement("Const", Const.Id));
        
        element.Add(new XElement("LocalContext", LocalContext.Id));
        if (FormulaChilds?.Count > 0) {
            element.Add(CreateListElement("FormulaChilds", FormulaChilds.Select(c => c.Id)));
            element.Add(CreateListElement("FormulaOperations", FormulaOperations!.Select(o => o.Serialize())));
        }
        if (Inversion != null)
            element.Add(CreateBoolElement("Inversion", (bool)Inversion));
        return element;
    }

    protected override RawData CreateRawData(XElement element) {
        return new RawExpressionData(
            element.Attribute("id")?.Value ?? throw new ArgumentException("Id missing"),
            GetRequiredElement(element, "ExpressionType").Value,
            GetRequiredElement(element, "TargetFunctionName").Value,
            GetRequiredElement(element, "TargetObject").Value,
            GetRequiredElement(element, "TargetParam").Value,
            element.Element("Const")?.Value,
            ParseListElement(element, "SourceParams"),
            GetRequiredElement(element, "LocalContext").Value,
            element.Element("Inversion")?.Let(ParseBool),
            ParseListElement(element, "FormulaChilds"),
            ParseListElement(element, "FormulaOperations")
        );
    }

    public override void FillFromRawData(RawData rawData, VirtualMachine vm) {
        if (rawData is not RawExpressionData data)
            throw new ArgumentException($"Expected RawExpressionData but got {rawData.GetType()}");

        ExpressionType = data.ExpressionType.Deserialize<ExpressionType>();
        
        LocalContext = vm.GetElement<Branch, Event, MindMapNode, Speech, State>(data.LocalContext);
        Inversion = data.Inversion;
        
        TargetObject = CommonVariable.Read(data.TargetObject, vm);
        Const = data.Const != null ? vm.GetElement<Parameter>(data.Const) : null;
        if (data.TargetFunctionName != null)
            Function = VmFunction.GetFunction(data.TargetFunctionName, vm, data.SourceParams ?? []);
        if (data.TargetParam != null)
            TargetParam = CommonVariable.Read(data.TargetParam, vm);
        if (data.FormulaChilds != null)
            FormulaChilds = data.FormulaChilds.Select(vm.GetElement<Expression>).ToList();
        FormulaOperations = data.FormulaOperations?.Select(e => e.Deserialize<FormulaOperation>()).ToList();
    }

    public override bool IsOrphaned() {
        return LocalContext.Element switch {
            Event e => !ConditionValid(e.Condition),
            MindMapNode mm => !mm.Content.Any(c => InCondition(c.ContentCondition)),
            Branch b => !b.BranchConditions.Any(HasValidBranchCondition),
            State st => !st.EntryPoints.Where(e => e.ActionLine != null).Any(e => InActionLine(e.ActionLine!)),
            Speech sp => !sp.Replies.Any(r => r.EnableCondition != null && InCondition(r.EnableCondition)),
            _ => true
        };

        bool InExpression(Expression? e) => (e?.FormulaChilds?.Contains(this) ?? false) || e == this;
        bool InPartCondition(PartCondition pc) => 
            pc.ConditionType is not (ConditionType.ConstFalse or ConditionType.ConstTrue) && 
            (InExpression(pc.FirstExpression) || (pc.ConditionType is not ConditionType.ValueExpression && 
                                                  InExpression(pc.SecondExpression)));
        bool InCondition(Condition cond) => cond.Predicates.Any(predicate => predicate.Element switch {
            PartCondition pc => InPartCondition(pc),
            Condition c => InCondition(c),
            _ => false
        });
        bool InActionLine(ActionLine al) => al.Actions?.Any(a => a.Element switch {
            ActionLine actionLine => InActionLine(actionLine),
            Action action => InExpression(action.SourceExpression),
            _ => false 
        }) ?? false;
        bool ConditionValid(Condition? condition) => condition != null && InCondition(condition);
        bool HasValidBranchCondition(VmEither<Condition, PartCondition> bCond) => bCond.Element switch {
            Condition cond => InCondition(cond),
            PartCondition pc => InPartCondition(pc),
            _ => false
        };
    }

    protected override VmElement New(VirtualMachine vm, string id, VmElement parent) {
        var expr = new Expression(id) {
            ExpressionType = ExpressionType.Const,
            LocalContext = new(parent)
        };
        expr.Const = CreateDefault<Parameter>(vm, expr);
        return expr;
    }

    public override void OnDestroy(VirtualMachine vm) {
        if (Const is not null) 
            vm.RemoveElement(Const);

        foreach (var pc in vm.GetElementsByType<PartCondition>()) {
            if (pc.FirstExpression == this)
                pc.FirstExpression = null!;
            if (pc.SecondExpression == this)
                pc.SecondExpression = null!;
        }

        if (FormulaChilds is null) return;
        foreach (var childExpression in FormulaChilds)
            vm.RemoveElement(childExpression);
    }
}