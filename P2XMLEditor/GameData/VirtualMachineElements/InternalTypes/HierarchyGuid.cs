using System;
using System.Collections.Generic;
using System.Linq;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.Interfaces;
using P2XMLEditor.GameData.VirtualMachineElements.Placeholders;
using P2XMLEditor.Helper;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes;

public class HierarchyGuid(string data, VirtualMachine vm) : ICommonVariableParameter {
	private readonly List<VmEither<Scene, Geom, Other, Item, ScenePlaceholder>> _elements = 
		data.Split('H', StringSplitOptions.RemoveEmptyEntries).Select(
			d => vm.GetNullableElement<Scene, Geom, Other, Item, ScenePlaceholder>(ulong.Parse(d)) ?? 
			     new(vm.Register(new ScenePlaceholder(ulong.Parse(d))))).ToList();
	private readonly VirtualMachine _vm = vm;

	public bool IsHierarchy => _elements.Count > 1;

	public string Write() => string.Join("H", _elements.Select(el => el.Id.ToString()));

	public static bool TryParse(string data, VirtualMachine vm, out HierarchyGuid? result) {
		try {
			result = new HierarchyGuid(data, vm);
			return true;
		}
		catch {
			result = null;
			return false;
		}
	}

	public string ParamId => Write();
}