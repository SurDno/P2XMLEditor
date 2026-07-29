using System.Collections.Generic;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.Enums;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions.Actions.Weather;

[Function("Weather.SetWeatherSampleGT")]
public class WeatherSetWeatherSampleGTFunction : VmFunction {
	public override VmType ReturnType => VmType.Void;
	public override int ParamCount => 3;
	public FunctionSourceParam<WeatherLayer>? Layer { get; }
	public FunctionSourceParam<Sample>? Sample { get; }
	public FunctionSourceParam<GameTime>? DurationGt { get; }
	public override List<string>? GetParamStrings() => [Layer?.Write() ?? "", Sample?.Write() ?? "", DurationGt?.Write() ?? ""];
	public WeatherSetWeatherSampleGTFunction(VirtualMachine vm, string[] parameters) {
		Layer = FunctionSourceParam<WeatherLayer>.Read(parameters[0], vm);
		Sample = FunctionSourceParam<Sample>.Read(parameters[1], vm);
		DurationGt = FunctionSourceParam<GameTime>.Read(parameters[2], vm);
	}
}