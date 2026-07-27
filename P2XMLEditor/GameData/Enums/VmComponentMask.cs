using System;
using System.Collections.Generic;

namespace P2XMLEditor.GameData.Enums;

public struct VmComponentMask {
	public VmComponent Mask { get; private set; }
	public VmComponent[]? CustomOrder { get; private set; }
	public bool IsEmpty => Mask == VmComponent.None;
	public bool IsOrdered => CustomOrder == null;
	public void Add(VmComponent component) {
		if (component == VmComponent.None) {
			return;
		}
		if (CustomOrder == null) {
			if (component > Mask) {
				Mask |= component;
				return;
			}
			ConvertToCustomOrder();
		}
		AddToCustomOrder(component);
	}
	private void ConvertToCustomOrder() {
		if (Mask == VmComponent.None) {
			CustomOrder = [];
			return;
		}
		List<VmComponent> list = [];
		for (var i = 0; i < 64; i++) {
			var vmComponent = (VmComponent)(1L << i);
			if ((Mask & vmComponent) != VmComponent.None) {
				list.Add(vmComponent);
			}
		}
		CustomOrder = list.ToArray();
	}
	private void AddToCustomOrder(VmComponent component) {
		var array = CustomOrder ?? [];
		var array2 = new VmComponent[array.Length + 1];
		Array.Copy(array, array2, array.Length);
		array2[array.Length] = component;
		CustomOrder = array2;
		Mask |= component;
	}
	public static implicit operator VmComponent(VmComponentMask mask) => mask.Mask;
}
