namespace P2XMLEditor.Parsing.RawData;

public struct RawExpressionData {
	public ulong Id;
	public string ExpressionType;
	public string? TargetFunctionName;
	public string TargetObject;
	public string? TargetParam;
	public ulong? ConstId;
	public List<string>? SourceParams;
	public ulong LocalContextId;
	public bool? Inversion;
	public List<ulong>? FormulaChilds;
	public List<string>? FormulaOperations;

	public override int GetHashCode() => Id.GetHashCode();
	
	public override string ToString() {
		return $"RawExpressionData {{ " +
		       $"Id={Id}, " +
		       $"ExpressionType={ExpressionType}, " +
		       $"TargetFunctionName={TargetFunctionName}, " +
		       $"TargetObject={TargetObject}, " +
		       $"TargetParam={TargetParam}, " +
		       $"ConstId={ConstId}, " +
		       $"SourceParams=[{(SourceParams != null ? string.Join(", ", SourceParams) : "")}], " +
		       $"LocalContextId={LocalContextId}, " +
		       $"Inversion={Inversion}, " +
		       $"FormulaChilds=[{(FormulaChilds != null ? string.Join(", ", FormulaChilds) : "")}], " +
		       $"FormulaOperations=[{(FormulaOperations != null ? string.Join(", ", FormulaOperations) : "")}] }}";
	}
}