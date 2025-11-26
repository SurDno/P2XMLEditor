using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.GameData.VirtualMachineElements.Enums;

namespace P2XMLEditor.Parsing.RawData;

public struct RawBranchData {
	public ulong Id;
	public List<ulong>? BranchConditionIds;
	public BranchType BranchType;
	public List<BranchVariantInfo>? BranchVariantInfo;
	public List<ulong> EntryPointIds;
	public bool? IgnoreBlock;
	public ulong OwnerId;
	public List<ulong>? InputLinkIds;
	public List<ulong>? OutputLinkIds;
	public bool? Initial;
	public string Name;
	public ulong ParentId;

	public override int GetHashCode() => Id.GetHashCode();
}