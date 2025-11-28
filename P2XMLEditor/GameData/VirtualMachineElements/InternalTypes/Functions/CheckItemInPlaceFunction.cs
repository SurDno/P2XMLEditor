using System;
using System.Collections.Generic;
using System.Linq;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;
using P2XMLEditor.Helper;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions;

[Function("Storage.CheckItemInPlace")]
public class CheckItemInPlaceFunction : VmFunction {
	public override FunctionReturnType ReturnType => FunctionReturnType.Bool;
	public override int ParamCount => 2;
   
	private readonly GameRoot root;
	private readonly GameObject item;
	private readonly Other container;
   
	public CheckItemInPlaceFunction(GameRoot root, Item item, Other container) {
		this.root = root ?? throw new ArgumentNullException(nameof(root));
		this.item = item ?? throw new ArgumentNullException(nameof(item));
		this.container = container ?? throw new ArgumentNullException(nameof(container));
	}
   
	public CheckItemInPlaceFunction(VirtualMachine vm, List<string> parameters) {
		if (parameters.Count != 2)
			throw new ArgumentException($"Expected 2 parameters, got {parameters.Count}");
           
		var parts1 = parameters[0].Split('%');
		if (parts1.Length != 2)
			throw new ArgumentException($"Invalid first parameter format: {parameters[0]}");
           
		var parts2 = parameters[1].Split('%');
		if (parts2.Length != 2)
			throw new ArgumentException($"Invalid second parameter format: {parameters[1]}");
           
		root = vm.GetElement<GameRoot>(ulong.Parse(parts1[0]));
		item = vm.GetElementsByType<Item>().FirstOrDefault(i => i.EngineTemplateId == parts1[1]) as GameObject ?? 
		       vm.GetElementsByType<Other>().FirstOrDefault(i => i.EngineTemplateId == parts1[1]) as GameObject ??
		       throw new ArgumentException($"No item found with template ID {parts1[1]}");
		container = vm.GetElementsByType<Other>().FirstOrDefault(i => i.EngineTemplateId == parts2[1])
		            ?? throw new ArgumentException($"No item found with template ID {parts2[1]}");
	}
   
	public override List<string> GetParamStrings() => [
		$"{root.Id}%{item.EngineTemplateId}", 
		$"{root.Id}%{container.EngineTemplateId}"
	];
}