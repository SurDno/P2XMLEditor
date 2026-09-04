using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.Logging;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes;


public readonly struct BranchVariantInfo {
	public string Name { get; init; }
	
	public string Type { get; init; } // CAST, never the variable's declared type

	public Message? Message { get; init; }
	public InputParameter? InputParam { get; init; }

	public bool IsResolved => Message != null || InputParam != null;

	public string? DeclaredType => Message?.Type ?? InputParam?.Type;

	public static BranchVariantInfo Read(string name, string castType, VirtualMachine vm, VmElement scope) {
		if (vm.TryResolveMessage(name, out var message))
			return new() { Name = name, Type = castType, Message = message };

		if (InputParameter.TryParse(name, vm, out var inputParam, scope))
			return new() { Name = name, Type = castType, InputParam = inputParam };

		Logger.Log(LogLevel.Warning, $"BranchVariantInfo '{name}' resolves to neither a message nor an input param.");
		return new() { Name = name, Type = castType };
	}
}