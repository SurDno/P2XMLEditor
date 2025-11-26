using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;
using P2XMLEditor.Helper;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions;

[Function("Storage.CheckExistCombinationElemen")]
public class CheckExistCombinationElementFunction : VmFunction {
	public override FunctionReturnType ReturnType => FunctionReturnType.Bool;
	public override int ParamCount => 1;
   
	private readonly GameRoot root;
	private readonly Item item;
   
	public CheckExistCombinationElementFunction(GameRoot root, Item item) {
		this.root = root ?? throw new ArgumentNullException(nameof(root));
		this.item = item ?? throw new ArgumentNullException(nameof(item));
	}
   
	public CheckExistCombinationElementFunction(VirtualMachine vm, List<string> parameters) {
		if (parameters.Count != 1)
			throw new ArgumentException($"Expected 1 parameter, got {parameters.Count}");
           
		var parts = parameters[0].Split('%');
		if (parts.Length != 2)
			throw new ArgumentException($"Invalid parameter format: {parameters[0]}");
           
		root = vm.GetElement<GameRoot>(ulong.Parse(parts[0]));
		item = vm.GetElement<Item>(ulong.Parse(parts[1]));
	}
   
	public override List<string> GetParamStrings() => [$"{root.Id}%{item.Id}"];
}