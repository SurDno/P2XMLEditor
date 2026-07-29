using System.Collections.Generic;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.Enums;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions.Actions.Weather;

[Function("Weather.SetWeatherLayerWeight")]
public class WeatherSetWeatherLayerWeightFunction(
	VirtualMachine vm,
	string[] parameters)
	: VmFunction {
	public override VmType ReturnType => VmType.Void;
	public override int ParamCount => 3;
	public FunctionSourceParam<WeatherLayer> Layer { get; } = FunctionSourceParam<WeatherLayer>.Read(parameters[0], vm);
	public FunctionSourceParam<float> Weight { get; } = FunctionSourceParam<float>.Read(parameters[1], vm);
	public FunctionSourceParam<float> Duration { get; } = FunctionSourceParam<float>.Read(parameters[2], vm);
	public override List<string>? GetParamStrings() => [Layer.Write(), Weight.Write(), Duration.Write()];
}