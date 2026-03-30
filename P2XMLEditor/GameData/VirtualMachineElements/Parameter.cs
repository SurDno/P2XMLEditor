using System;
using System.Collections.Generic;
using System.Linq;
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

public class Parameter(ulong id) : VmElement(id), IFiller<RawParameterData>, ICommonVariableParameter, IVmCreator<Parameter> {
	public string Name { get; set; }
	public FunctionalComponent? OwnerComponent { get; set; } // Does not exist in Demo and ealier
	public string Type { get; set; }
	public string Value { get; set; }
	public bool? Implicit { get; set; }
	public VmEither<ParameterHolder, Expression> Parent { get; set; }
	public bool? Custom { get; set; } // Does not exist in Demo and ealier


	public override bool IsOrphaned() {
		return Parent.Element switch {
			ParameterHolder ph => ph.StandartParams.Concat(ph.CustomParams).All(p => p.Value != this),
			Expression e => e.Const != this,
			_ => true
		};
	}
	
	public void FillFromRawData(RawParameterData data, VirtualMachine vm) {
		try {
			Name = data.Name;
			OwnerComponent = data.OwnerComponentId.HasValue
				? vm.GetElement<FunctionalComponent>(data.OwnerComponentId.Value)
				: null;
			Type = data.Type;
			Value = data.Value;
			Implicit = data.Implicit;
			Parent = vm.GetElement<ParameterHolder, Expression>(data.ParentId);
			Custom = data.Custom;
		} catch (Exception e) {
			Console.WriteLine(e);
			Console.WriteLine(data.Id);
			throw;
		}
	}

	public static Parameter New(VirtualMachine vm, ulong id, VmElement parent) {
		var par = new Parameter(id) {
			Name = "NewParam",
			Parent = new(parent),
			Implicit = false,
			Custom = false,
			Type = "System.Boolean",
			Value = "False"
		};
		return par;
	}

	public override void OnDestroy(VirtualMachine vm) {
		switch (Parent.Element) {
			case ParameterHolder ph:		  
				var keyToRemove = ph.StandartParams.FirstOrDefault(kvp => kvp.Value == this).Key;
				if (keyToRemove != null)
					ph.StandartParams.Remove(keyToRemove);
				keyToRemove = ph.CustomParams.FirstOrDefault(kvp => kvp.Value == this).Key;
				if (keyToRemove != null)
					ph.CustomParams.Remove(keyToRemove);
				break;
			case Expression e:
				// Do we need to redo something about the expression if we remove the const value? Needs testing.
				e.Const = null;
				break;
		}
		
		// TODO: Redo when we start parsing types like normal human beings.
		if (Type == "ITextRef" && !string.IsNullOrEmpty(Value))
			vm.RemoveElement(vm.GetElement<GameString>(ulong.Parse(Value)));
	}

	public string ParamId => id.ToString();

	public bool IsCustom() {
		if (Custom.HasValue)
			return Custom.Value;
		
		if (Parent.Element is not ParameterHolder ph) 
			return true;
		
		return ph.CustomParams != null && ph.CustomParams.Any(kvp => kvp.Value == this);
	}

	public FunctionalComponent? FindOwnerComponent() {
		if (OwnerComponent != null)
			return OwnerComponent;

		if (Parent.Element is not ParameterHolder ph || IsCustom()) 
			return null;
		
		// TODO: THIS IS VERY IMPERFECT. WHAT IF A PARAMETER OR A FUNCTIONAL COMPONENT IS RENAMED? FIND A BETTER WAY? 
		var key = ph.StandartParams.FirstOrDefault(kvp => kvp.Value == this).Key;
		return ph.FunctionalComponents.FirstOrDefault(fc => key.StartsWith(fc.Name));
	}
}