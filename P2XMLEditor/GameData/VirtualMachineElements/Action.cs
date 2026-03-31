using System.Collections.Generic;
using System.Linq;
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

public class Action(ulong id) : VmElement(id), IFiller<RawActionData>, INamedElement {
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

	public bool? Enabled { get; set; } // Demo-only

	public void FillFromRawData(RawActionData data, VirtualMachine vm) {
		ActionType = data.ActionType;
		MathOperationType = data.MathOperationType;
		TargetFuncName = data.TargetFuncName;
		SourceExpression = data.SourceExpressionId.HasValue ? 
			vm.GetElement<Expression>(data.SourceExpressionId.Value) : null;
		TargetObject = data.TargetObject;
		TargetParam = data.TargetParam;
		SourceParams = data.SourceParams?.ToList();
		Name = data.Name;
		LocalContext = vm.GetElement<State, Graph, Branch, Talking, Speech>(data.LocalContextId);
		OrderIndex = data.OrderIndex;
		Enabled = data.Enabled;
	}
		
	public override void OnDestroy(VirtualMachine vm) {
		if (SourceExpression != null)
			vm.RemoveElement(SourceExpression);
	}
}