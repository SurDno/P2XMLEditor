using P2XMLEditor.GameData.VirtualMachineElements.Interfaces;
using P2XMLEditor.Logging;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes;

public class UnresolvedParameter : ICommonVariableParameter {
	public string RawData { get; }
	public string Id => RawData; 

	public UnresolvedParameter(string rawData) {
		Logger.Log(LogLevel.Error, $"Unresolved parameter {rawData}, preserving as string.");
		RawData = rawData;
	}
}