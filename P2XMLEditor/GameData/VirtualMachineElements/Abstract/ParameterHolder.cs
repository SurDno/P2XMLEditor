using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using P2XMLEditor.Core;
using P2XMLEditor.Data;
using P2XMLEditor.GameData.VirtualMachineElements.Interfaces;
using static P2XMLEditor.Helper.XmlParsingHelper;

#pragma warning disable CS8618

namespace P2XMLEditor.GameData.VirtualMachineElements.Abstract;

public abstract class ParameterHolder(ulong id) : VmElement(id), INamedElement {
	public bool? Static { get; set; }
	public List<FunctionalComponent> FunctionalComponents { get; set; }
	public Graph? EventGraph { get; set; }
	public Dictionary<string, Parameter> StandartParams { get; set; }
	public Dictionary<string, Parameter>? CustomParams { get; set; }
	public string? GameTimeContext { get; set; }
	public string Name { get; set; }
	public ParameterHolder? Parent { get; set; }
	public List<string>? InheritanceInfo { get; set; }
	public List<Event>? Events { get; set; }
	public List<ParameterHolder>? ChildObjects { get; set; }


	public override void OnDestroy(VirtualMachine vm) {
		foreach (var functionalComponent in FunctionalComponents?.ToList() ?? [])
			vm.RemoveElement(functionalComponent);
		if (EventGraph != null)
			vm.RemoveElement(EventGraph);
		foreach (var kvp in StandartParams?.ToList() ?? []) 
			vm.RemoveElement(kvp.Value);
		foreach (var kvp in CustomParams?.ToList() ?? []) 
			vm.RemoveElement(kvp.Value);
		foreach (var ev in Events?.ToList() ?? [])
			vm.RemoveElement(ev);
		foreach (var ph in vm.GetElementsByType<ParameterHolder>().ToList()) {
			if (ph.ChildObjects != null && ph.ChildObjects.Contains(this))
				ph.ChildObjects.Remove(this);
		}
		
		// There are cases of one-sided ParameterHolder-GameString relations.
		// Those are all outdated strings and aren't actually used by the game, but will crash P2XMLE on later reloads.
		// So they have to be forcibly removed at this step (but can theoretically be cleaned up earlier).
		foreach (var gs in vm.GetElementsByType<GameString>().Where(g => g.Parent.Element == this).ToList())
			vm.RemoveElement(gs);
	}

	public string ParamId => id.ToString();
}
