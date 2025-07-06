using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.Interfaces;
using P2XMLEditor.GameData.VirtualMachineElements.Placeholders;
using P2XMLEditor.Helper;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes;

public class CommonVariable : IEquatable<CommonVariable> {
	private readonly VirtualMachine _vm;
	public ICommonVariableParameter? ContextParameter { get; private set; }
	public ICommonVariableParameter? VariableParameter { get; private set; }

	private CommonVariable(VirtualMachine vm) => _vm = vm;

	public static CommonVariable? Read(string data, VirtualMachine vm) {
		var inst = new CommonVariable(vm);
		var parts = data.Split('%');

		switch (parts.Length) {
			case 1:
				inst.ContextParameter = ParseParameter(parts[0], vm);
				inst.VariableParameter = null;
				break;
			case >= 2:
				inst.ContextParameter = !string.IsNullOrEmpty(parts[0]) ? ParseParameter(parts[0], vm) : null;
				inst.VariableParameter = !string.IsNullOrEmpty(parts[1]) ? ParseParameter(parts[1], vm) : null;
				break;
		}

		return inst;
	}

	private static ICommonVariableParameter ParseParameter(string data, VirtualMachine vm) {
		if (data.Contains("_message_"))
			return new Message(data);
		if (data.Contains('H') && HierarchyGuid.TryParse(data, vm, out var hierarchyGuid))
			return hierarchyGuid!;
		if (vm.ElementsById.ContainsKey(data))
			return vm.GetElement<ICommonVariableParameter>(data);
		if (ulong.TryParse(data, out _))
			return vm.Register(new ParameterPlaceholder(data));
		if (Guid.TryParse(data, out var guid))
			return new GuidParameter(guid);
		if (data.Contains("_inputparam_")) {
			if (InputParameter.TryParse(data, vm, out var inputParam))
				return inputParam;
			return new UnresolvedParameter(data);
		}
		if (data.Contains('.') && ParamByName.Validate(data, vm))
			return new ParamByName(data);
		throw new ArgumentException($"Could not parse parameter: {data}");
	}

	public string Write() {
		return VariableParameter == null ? ContextParameter?.Id ?? "%" :
			   ContextParameter == null  ? $"%{VariableParameter.Id}" : $"{ContextParameter.Id}%{VariableParameter.Id}";
	}

	public override bool Equals(object? obj) => Equals(obj as CommonVariable);
	public bool Equals(CommonVariable? other) => other != null && Write() == other.Write();
	public override int GetHashCode() => Write().GetHashCode();
	public static bool operator ==(CommonVariable? left, CommonVariable? right) => left?.Equals(right) ?? right is null;
	public static bool operator !=(CommonVariable? left, CommonVariable? right) => !(left == right);
}