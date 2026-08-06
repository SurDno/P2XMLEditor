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
	public ulong? SourceConst { get; set; }
	public TargetObject TargetObject { get; set; }
	public ParamTarget TargetParam { get; set; }
	public ParameterSource? Source { get; set; }
	public List<ParameterSource>? EventParams { get; set; }
	public string Name { get; set; }
	public VmEither<State, Graph, Branch, Talking, Speech> LocalContext { get; set; }

	public int OrderIndex { get; set; }

	public string TargetFuncName => ActionType switch {
		ActionType.RaiseEvent => EventToRaise?.Id.ToString() ?? _rawTargetFuncName,
		ActionType.DoFunction => Function?.Name ?? _rawTargetFuncName,
		_ => _rawTargetFuncName,
	};

	/// <summary>
	/// Drops the raw function name carried over from the data. RaiseEvent and DoFunction
	/// derive <see cref="TargetFuncName"/> from EventToRaise/Function, but every other type
	/// echoes whatever was loaded — and those all have an empty TargetFuncName in the data —
	/// so retyping an action has to clear it or the writer emits a stale name.
	/// </summary>
	public void ClearRawTargetFuncName() => _rawTargetFuncName = "";

	public bool? Enabled { get; set; }


	public List<string>? GetParamStrings() {
		if (Function != null) 
			return Function.GetParamStrings();

		if ((uint)(ActionType - 1) <= 2u) {
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
		using var scope = VirtualMachine.EnterFillScope(LocalContext.Element);
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
		SourceConst = data.SourceConstId;
		TargetObject = TargetObject.Read(data.TargetObject, vm)!;
		if (ParamTarget.TryRead(data.TargetParam, vm, out var tp))
			TargetParam = tp;

		try {
			if ((uint)(ActionType - 1) <= 2u) {
				if (data.SourceParams is { Length: > 0 })
					Source = ParameterSource.Create(data.SourceParams[0], vm, TargetParam);
			} else if (ActionType == ActionType.RaiseEvent && data.SourceParams is { Length: > 0 }) {
				EventParams = data.SourceParams.Select((ps, i) => {
					var expected = EventToRaise != null && i < EventToRaise.Messages.Count
						? VmTypeHelper.GetVmTypeInfo(EventToRaise.Messages[i].Type, vm)
						: null;
					return ParameterSource.Create(ps, vm, null, expected);
				}).ToList();
			}
		} catch {
			Console.WriteLine(base.Id);
			throw;
		}
	}
		
	/// <summary>
	/// A blank SetParam action in the local context of <paramref name="parent"/>. SetParam is
	/// the only type that says nothing until it is filled in — every other one needs a function,
	/// an event or an expression the caller has not chosen yet.
	/// </summary>
	public static Action New(VirtualMachine vm, ulong id, VmElement parent) => new(id) {
		Name = "",
		ActionType = ActionType.SetParam,
		MathOperationType = MathOperationType.None,
		OrderIndex = 0,
		LocalContext = new(ActionLine.LocalContextOf(parent))
	};

	public override void OnDestroy(VirtualMachine vm) {
		if (SourceExpression != null)
			vm.RemoveElement(SourceExpression);
	}
}
