using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes;
using P2XMLEditor.Logging;


public readonly struct ExpressionParamTarget {
	public ParamTarget? Param { get; init; }
	public VmElement? ObjectLiteral { get; init; }

	public bool IsLiteral => ObjectLiteral != null;

	public static ExpressionParamTarget Read(string data, VirtualMachine vm, VmElement? scope = null) {
		if (ParamTarget.TryRead(data, vm, out var param))
			return new() { Param = param };
		
		// TODO: we'll need to add support	
		Logger.Log(LogLevel.Error, $"Unresolved Expression TargetParam '{data}'.");
		return new() { Param = ParamTarget.Empty() };
	}

	public string Write() => Param?.Write() ?? ObjectLiteral!.Id.ToString();
	public override string ToString() => Write();
}
