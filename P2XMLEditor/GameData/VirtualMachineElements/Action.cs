using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.GameData.VirtualMachineElements.Enums;
using P2XMLEditor.GameData.VirtualMachineElements.Interfaces;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;
using P2XMLEditor.Helper;
using P2XMLEditor.Parsing.RawData;
using P2XMLEditor.Services;

namespace P2XMLEditor.GameData.VirtualMachineElements;

public class Action(ulong id) : VmElement(id), IFiller<RawActionData>, INamedElement {
	private string _rawTargetFuncName;
	public ActionType ActionType { get; set; }
	public MathOperationType MathOperationType { get; set; }
	public Event? EventToRaise { get; set; }
	public VmFunction? Function { get; set; }
	public Expression? SourceExpression { get; set; }
	public CommonVariable TargetObject { get; set; }
	public CommonVariable? TargetParam { get; set; }
	public ParameterSource? Source { get; set; }
	public List<CommonVariable>? EventParams { get; set; }
	public string Name { get; set; }
	public VmEither<State, Graph, Branch, Talking, Speech> LocalContext { get; set; }

	public int OrderIndex { get; set; }

	public string TargetFuncName => ActionType switch {
		ActionType.RaiseEvent => EventToRaise?.Id.ToString() ?? _rawTargetFuncName,
		ActionType.DoFunction => Function?.Name ?? _rawTargetFuncName,
		_ => _rawTargetFuncName,
	};

	public bool? Enabled { get; set; }


	public List<string>? GetParamStrings() {
		if (Function != null) 
			return Function.GetParamStrings();
		

		ActionType actionType = ActionType;
		if ((uint)(actionType - 1) <= 2u) {
			string text = Source?.Write();
			if (text == null) {
				return null;
			}

			int num = 1;
			List<string> list = new List<string>(num);
			CollectionsMarshal.SetCount(list, num);
			Span<string> span = CollectionsMarshal.AsSpan(list);
			int num2 = 0;
			span[num2] = text;
			num2++;
			return list;
		}

		if (ActionType == ActionType.RaiseEvent) {
			return EventParams?.Select(p => p.Write()).ToList();
		}

		return null;
	}

	public void FillFromRawData(RawActionData data, VirtualMachine vm) {
		ActionType = data.ActionType;
		MathOperationType = data.MathOperationType;
		_rawTargetFuncName = data.TargetFuncName;
		Name = data.Name;
		LocalContext = vm.GetElement<State, Graph, Branch, Talking, Speech>(data.LocalContextId);
		OrderIndex = data.OrderIndex;
		Enabled = data.Enabled;
		switch (ActionType) {
			case ActionType.RaiseEvent when ulong.TryParse(data.TargetFuncName, out var result):
				EventToRaise = vm.GetElement<Event>(result);
				break;
			case ActionType.DoFunction when !string.IsNullOrEmpty(data.TargetFuncName): {
				Function = VmFunction.GetFunction(data.TargetFuncName, vm, data.SourceParams ?? []);
				break;
			}
		}

		SourceExpression = (data.SourceExpressionId.HasValue
			? vm.GetElement<Expression>(data.SourceExpressionId.Value)
			: null);
		TargetObject = CommonVariable.Read(data.TargetObject, vm);
		TargetParam = ((!string.IsNullOrEmpty(data.TargetParam)) ? CommonVariable.Read(data.TargetParam, vm) : null);
		try {
			ActionType actionType = ActionType;
			if ((uint)(actionType - 1) <= 2u) {
				string[]? sourceParams = data.SourceParams;
				if (sourceParams != null && sourceParams.Length != 0) {
					Source = ParameterSource.Create(data.SourceParams[0], vm, TargetParam);
				}
			} else if (ActionType == ActionType.RaiseEvent) {
				string[]? sourceParams2 = data.SourceParams;
				if (sourceParams2 != null && sourceParams2.Length != 0) {
					EventParams = data.SourceParams.Select((string ps) => CommonVariable.Read(ps, vm)).ToList();
				}
			}
		} catch {
			Console.WriteLine(base.Id);
			throw;
		}
	}
		
	public override void OnDestroy(VirtualMachine vm) {
		if (SourceExpression != null)
			vm.RemoveElement(SourceExpression);
	}
}