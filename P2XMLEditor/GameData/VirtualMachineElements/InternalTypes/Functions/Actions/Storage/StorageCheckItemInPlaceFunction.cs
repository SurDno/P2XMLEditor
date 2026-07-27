using System.Collections.Generic;
using System.Linq;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;
using P2XMLEditor.Helper;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions.Actions.Storage;

[Function("Storage.CheckItemInPlace")]
public class StorageCheckItemInPlaceFunction : VmFunction {
	public override FunctionReturnType ReturnType => FunctionReturnType.Bool;
	public override int ParamCount => 2;
	private GameRoot Root { get; }
	private VmEither<Item, Other> Storable { get; }
	private Other Container { get; }
	public override List<string>? GetParamStrings() => [$"{Root.Id}%{Container.EngineTemplateId}"];
	public StorageCheckItemInPlaceFunction(VirtualMachine vm, string[] parameters) {
		var parts1 = parameters[0].Split('%');
		var parts2 = parameters[1].Split('%');
		Root = vm.GetElement<GameRoot>(ulong.Parse(parts1[0]));
		Storable = new VmEither<Item, Other>((VmElement)(((object)vm.GetElementsByType<Item>().FirstOrDefault(i => i.EngineTemplateId == parts1[1])) ?? vm.GetElementsByType<Other>().First(i => i.EngineTemplateId == parts1[1])));
		Container = vm.GetElementsByType<Other>().First(i => i.EngineTemplateId == parts2[1]);
	}
}
