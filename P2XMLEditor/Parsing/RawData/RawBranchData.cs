using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.GameData.VirtualMachineElements.Enums;

namespace P2XMLEditor.Parsing.RawData;

public struct RawBranchData {
	public ulong Id;
	public ulong[]? BranchConditionIds;
	public BranchType BranchType;
	public BranchVariantInfo[]? BranchVariantInfo;
	public ulong[] EntryPointIds;
	public bool? IgnoreBlock;
	public ulong OwnerId;
	public ulong[]? InputLinkIds;
	public ulong[]? OutputLinkIds;
	public bool? Initial;
	public string Name;
	public ulong ParentId;

	public override int GetHashCode() => Id.GetHashCode();
}