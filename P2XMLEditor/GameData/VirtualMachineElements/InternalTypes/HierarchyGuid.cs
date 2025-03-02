using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.Interfaces;
using P2XMLEditor.GameData.VirtualMachineElements.Placeholders;
using P2XMLEditor.Helper;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes;

public class HierarchyGuid(string data, VirtualMachine vm) : ICommonVariableParameter {
	private readonly List<VmEither<Scene, Geom, Other, Item, ScenePlaceholder>> _elements = 
		data.Split('H', StringSplitOptions.RemoveEmptyEntries).Select(
			d => vm.GetNullableElement<Scene, Geom, Other, Item, ScenePlaceholder>(d) ?? 
			     new(vm.Register(new ScenePlaceholder(d)))).ToList();
	private readonly VirtualMachine _vm = vm;

	public bool IsHierarchy => _elements.Count > 1;

	public string Write() => string.Join("H", _elements.Select(el => el.Id));

	public static bool TryParse(string data, VirtualMachine vm, out HierarchyGuid? result) {
		result = new HierarchyGuid(data, vm);
		return true;
	}

	public string Id => Write();
}