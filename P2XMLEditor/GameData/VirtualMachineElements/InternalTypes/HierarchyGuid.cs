using System;
using System.Collections.Generic;
using System.Linq;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.Interfaces;
using P2XMLEditor.GameData.VirtualMachineElements.Placeholders;
using P2XMLEditor.Helper;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes;

public class HierarchyGuid(string data, VirtualMachine vm)  {
	private readonly List<VmEither<Scene, Geom, Other, Item, ScenePlaceholder>> _elements =
		data.Split('H', StringSplitOptions.RemoveEmptyEntries).Select(
			d => vm.GetNullableElement<Scene, Geom, Other, Item, ScenePlaceholder>(ulong.Parse(d)) ??
				 new(vm.Register(new ScenePlaceholder(ulong.Parse(d))))).ToList();

	private readonly VirtualMachine _vm = vm;

	public bool IsHierarchy => _elements.Count > 1;

	public List<VmEither<Scene, Geom, Other, Item, ScenePlaceholder>> Elements => _elements;

	public string Write() => string.Join("H", _elements.Select(el => el.Id.ToString()));

	public static bool LooksLikeHierarchy(ReadOnlySpan<char> data) {
		var hasSeparator = false;
		var hasDigit = false;
		foreach (var c in data) {
			if (char.IsAsciiDigit(c)) { hasDigit = true; continue; }
			if (c == 'H') { hasSeparator = true; continue; }
			return false;
		}
		return hasSeparator && hasDigit;
	}

	public static bool TryParse(string data, VirtualMachine vm, out HierarchyGuid? result) {
		result = null;
		if (!LooksLikeHierarchy(data)) return false;
		try {
			result = new HierarchyGuid(data, vm);
			return true;
		}
		catch {
			result = null;
			return false;
		}
	}

	public static bool TryParseDoubled(string data, VirtualMachine vm, out HierarchyGuid? result) {
		result = null;
		var sep = data.IndexOf('%');
		if (sep <= 0 || sep == data.Length - 1) return false;

		var left = data.AsSpan(0, sep);
		var right = data.AsSpan(sep + 1);
		if (!left.SequenceEqual(right)) return false;
		if (!LooksLikeHierarchy(left)) return false;

		return TryParse(data[..sep], vm, out result);
	}

	public string ParamId => Write();
}
