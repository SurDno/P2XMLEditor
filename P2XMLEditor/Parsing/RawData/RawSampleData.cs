using P2XMLEditor.GameData.VirtualMachineElements.Enums;

namespace P2XMLEditor.Parsing.RawData;

public struct RawSampleData {
	public ulong Id;
	public SampleType SampleType;
	public string EngineId;

	public override int GetHashCode() => Id.GetHashCode();
}