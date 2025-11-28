using System;
using P2XMLEditor.GameData.VirtualMachineElements.Interfaces;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes;

public class GuidParameter(Guid value) : ICommonVariableParameter {
	private Guid Value { get; } = value;
	public string ParamId => Value.ToString("N");
}