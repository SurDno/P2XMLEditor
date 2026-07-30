using P2XMLEditor.GameData.Enums;
using System;
using System.Collections.Generic;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;
using P2XMLEditor.Helper;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions.Actions.GameComponent;

[Function("GameComponent.GetObjectClass")]
public class GetObjectClassFunction : VmFunction {
	public override VmType ReturnType => VmType.BlueprintRef;
	public override int ParamCount => 1;
	public FunctionSourceParam<ObjRef> Object { get; }

	public GetObjectClassFunction(VirtualMachine vm, string[] parameters) {
		Object = FunctionSourceParam<ObjRef>.Read(parameters[0], vm);
	}
	public override List<string>? GetParamStrings() => [Object.Write()];
}