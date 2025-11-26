using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;
using P2XMLEditor.Helper;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions;

[Function("Storage.GetItemsCountByTemplate")]
public class GetItemsCountByTemplateFunction : VmFunction {
	public override FunctionReturnType ReturnType => FunctionReturnType.Int;
	public override int ParamCount => 1;
    
	private readonly GameRoot root;
	private readonly GameObject itemRef;
    
	public GetItemsCountByTemplateFunction(GameRoot root, Item itemReference) {
		this.root = root ?? throw new ArgumentNullException(nameof(root));
		itemRef = itemReference ?? throw new ArgumentNullException(nameof(itemReference));
	}
    
	public GetItemsCountByTemplateFunction(VirtualMachine vm, List<string> parameters) {
		if (parameters.Count != 1) 
			throw new ArgumentException($"Expected 1 parameter, got {parameters.Count}");
            
		var parts = parameters[0].Split('%');
		if (parts.Length != 2) 
			throw new ArgumentException($"Invalid parameter format: {parameters[0]}");
            
		root = vm.GetElement<GameRoot>(ulong.Parse(parts[0]));
		itemRef = vm.GetElementsByType<Item>().FirstOrDefault(i => i.EngineTemplateId == parts[1]) as GameObject ?? 
		          vm.GetElementsByType<Other>().FirstOrDefault(i => i.EngineTemplateId == parts[1]) as GameObject ??
		          throw new ArgumentException($"No item found with template ID {parts[1]}");
	}
    
	public override List<string>? GetParamStrings() => [$"{root.Id}%{itemRef.EngineTemplateId}"];
}