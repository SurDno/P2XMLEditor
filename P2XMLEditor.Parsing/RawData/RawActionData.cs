using P2XMLEditor.Enums.VirtualMachine;

namespace P2XMLEditor.Parsing.RawData;

public struct RawActionData {
	public ulong Id;
	public ActionType ActionType;
	public MathOperationType MathOperationType;
	public string TargetFuncName;
	public ulong? SourceExpressionId;
	public string TargetObject;
	public string TargetParam;
	public string[]? SourceParams;
	public string Name;
	public ulong LocalContextId;
	public int OrderIndex;

	public override int GetHashCode() => Id.GetHashCode();
}