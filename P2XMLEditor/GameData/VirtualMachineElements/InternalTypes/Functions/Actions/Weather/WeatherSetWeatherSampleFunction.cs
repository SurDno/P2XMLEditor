using System.Collections.Generic;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.Enums;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions.Actions.Weather;

[Function("Weather.SetWeatherSample")]
public class WeatherSetWeatherSampleFunction : VmFunction {
	public override FunctionReturnType ReturnType => FunctionReturnType.Void;
	public override int ParamCount => 3;
	public FunctionSourceParam<WeatherLayer>? Layer { get; }
	public FunctionSourceParam<Sample>? Sample { get; }
	public FunctionSourceParam<float>? Duration { get; }
	public override List<string>? GetParamStrings() => [Layer?.Write() ?? "", Sample?.Write() ?? "", Duration?.Write() ?? ""];
	public WeatherSetWeatherSampleFunction(VirtualMachine vm, string[] parameters) {
		Layer = FunctionSourceParam<WeatherLayer>.Read(parameters[0], vm);
		Sample = FunctionSourceParam<Sample>.Read(parameters[1], vm);
		Duration = FunctionSourceParam<float>.Read(parameters[2], vm);
	}
}
