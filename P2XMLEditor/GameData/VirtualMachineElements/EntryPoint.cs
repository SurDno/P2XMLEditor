using System;
using System.Collections.Generic;
using System.Xml.Linq;
using P2XMLEditor.Core;
using P2XMLEditor.Data;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.GameData.VirtualMachineElements.Interfaces;
using P2XMLEditor.Helper;
using P2XMLEditor.Parsing.RawData;
using static P2XMLEditor.Helper.XmlParsingHelper;

#pragma warning disable CS8618

namespace P2XMLEditor.GameData.VirtualMachineElements;

public class EntryPoint(ulong id) : VmElement(id), IFiller<RawEntryPointData> {
	public string Name { get; set; }
	public ActionLine? ActionLine { get; set; }
	public VmEither<State, Graph, Branch, Speech, Talking>? Parent { get; set; }


	public void FillFromRawData(RawEntryPointData data, VirtualMachine vm) {
		Name = data.Name;
		ActionLine = data.ActionLineId.HasValue ? vm.GetElement<ActionLine>(data.ActionLineId.Value) : null;
		Parent = data.ParentId.HasValue ? (VmEither<State, Graph, Branch, Speech, Talking>?)vm.GetElement<State, Graph, Branch, Speech, Talking>(data.ParentId.Value) : null;
	}
	
	/// <summary>
	/// A fresh entry point, with the action line it exists to run. Every one of the 42 000-odd
	/// entry points in the corpus has a line; one without has nothing to do on arrival, so the
	/// line is made here rather than left for the caller to remember.
	/// </summary>
	public static EntryPoint New(VirtualMachine vm, ulong id, VmElement parent) {
		var point = new EntryPoint(id) {
			Name = "Entry",
			Parent = new(parent)
		};
		point.ActionLine = CreateDefault<ActionLine>(vm, parent);
		return point;
	}
	
	public override void OnDestroy(VirtualMachine vm) {
		if (ActionLine != null)
			vm.RemoveElement(ActionLine);
			
		if (Parent?.Element is IGraphElement graphElement) {
			graphElement.EntryPoints?.Remove(this);
		} else if (Parent?.Element is Speech speech) {
			speech.EntryPoints?.Remove(this);
		} else if (Parent?.Element is Graph graph) {
			graph.EntryPoints?.Remove(this);
		} else if (Parent?.Element is Talking talking) {
			talking.EntryPoints?.Remove(this);
		} else if (Parent?.Element is ActionLine actionLine) {
			// ActionLine does not have EntryPoints collection in its class definition
		}
	}
}
