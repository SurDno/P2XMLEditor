using System;
using System.Collections.Generic;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions;

[Function("GameComponent.GetProfileBoolValue")]
public class GetProfileBoolValueFunction : VmFunction {
	public override FunctionReturnType ReturnType => FunctionReturnType.Bool;
	public override int ParamCount => 1;
   
	private readonly string _profileKey;
   
	public GetProfileBoolValueFunction(string profileKey) {
		_profileKey = profileKey ?? throw new ArgumentNullException(nameof(profileKey));
	}
   
	public GetProfileBoolValueFunction(VirtualMachine vm, List<string> parameters) {
		if (parameters.Count != 1)
			throw new ArgumentException($"Expected 1 parameter, got {parameters.Count}");
           
		var value = parameters[0];
		if (!value.StartsWith('%'))
			throw new ArgumentException($"Profile key must start with %: {value}");
           
		_profileKey = value.TrimStart('%');
	}
   
	public override List<string> GetParamStrings() => [$"%{_profileKey}"];
}