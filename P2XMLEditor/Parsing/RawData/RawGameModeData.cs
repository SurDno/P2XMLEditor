using System;

namespace P2XMLEditor.Parsing.RawData;

public struct RawGameModeData {
	public ulong Id;
	public bool? IsMain;
	public TimeSpan StartGameTime;
	public float GameTimeSpeed;
	public TimeSpan StartSolarTime;
	public float SolarTimeSpeed;
	public string PlayerRef;
	public string Name;
	public ulong ParentId;

	public override int GetHashCode() => Id.GetHashCode();
}