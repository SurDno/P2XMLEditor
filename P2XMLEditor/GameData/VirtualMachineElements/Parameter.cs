using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using P2XMLEditor.Core;
using P2XMLEditor.Data;
using P2XMLEditor.Forms.MainForm.Combinations;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.GameData.VirtualMachineElements.Interfaces;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes;
using P2XMLEditor.GameData.Enums;
using P2XMLEditor.Helper;
using P2XMLEditor.Attributes;
using P2XMLEditor.Parsing.RawData;
using static P2XMLEditor.Helper.XmlParsingHelper;
using static P2XMLEditor.Parsing.Helpers.XElementExtensions;

#pragma warning disable CS8618

namespace P2XMLEditor.GameData.VirtualMachineElements;

public class Parameter(ulong id) : VmElement(id), IFiller<RawParameterData>, IVmCreator<Parameter> {
	public string Name { get; set; }
	public FunctionalComponent? OwnerComponent { get; set; }
	public bool Implicit { get; set; }
	public VmEither<ParameterHolder, Expression>? Parent { get; set; }
	public bool Custom { get; set; }
	public ParameterValue Value { get; set; }

	public string Type => Value?.XmlType ?? string.Empty;
	public string SerializedValue => Value?.Serialize() ?? string.Empty;

	/// <summary>
	/// True for a parameter that holds an expression's constant operand rather than living on
	/// an object. It is storage for a literal the expression reads, so nothing can be written
	/// into it and no object can be reached through it. The data agrees: of the 6224 such
	/// parameters in PathologicSandbox and 512 in MarbleNest, not one is named by any action —
	/// not as a target object, not as a target param, not as a source.
	/// </summary>
	public bool IsConstant => Parent?.Element is Expression;

	public override bool IsOrphaned() {
		return Parent?.Element switch {
			ParameterHolder ph => ph.StandartParams.Concat(ph.CustomParams ?? []).All(p => p.Value != this),
			Expression e => e.Const != this,
			_ => true
		};
	}

	public void FillFromRawData(RawParameterData data, VirtualMachine vm) {
		Name = data.Name;
		OwnerComponent = data.OwnerComponentId.HasValue
			? vm.GetElement<FunctionalComponent>(data.OwnerComponentId.Value)
			: null;
		Implicit = data.Implicit;
		Parent = data.ParentId.HasValue ? (VmEither<ParameterHolder, Expression>?)vm.GetElement<ParameterHolder, Expression>(data.ParentId.Value) : null;
		Custom = data.Custom;
		Value = ParameterValue.Create(vm, data.Type, data.Value);
	}

	public static Parameter New(VirtualMachine vm, ulong id, VmElement parent) {
		return new Parameter(id) {
			Name = "NewParam",
			Parent = new(parent),
			Implicit = false,
			Custom = false,
			Value = new BasicValue<bool>("System.Boolean", false)
		};
	}

	public override void OnDestroy(VirtualMachine vm) {
		switch (Parent?.Element) {
			case ParameterHolder ph:
				var keyToRemove = ph.StandartParams.FirstOrDefault(kvp => kvp.Value == this).Key;
				if (keyToRemove != null) ph.StandartParams.Remove(keyToRemove);
				keyToRemove = ph.CustomParams?.FirstOrDefault(kvp => kvp.Value == this).Key;
				if (keyToRemove != null) ph.CustomParams.Remove(keyToRemove);
				break;
			case Expression e:
				e.Const = null;
				break;
		}
		if (Value is RefValue<GameString> textRef && textRef.TypedValue != null)
			vm.RemoveElement(textRef.TypedValue);
	}

	public string ParamId => Id.ToString();

	public bool IsCustom() {
		return Custom;
		if (Parent?.Element is not ParameterHolder ph) return true;
		return ph.CustomParams != null && ph.CustomParams.Any(kvp => kvp.Value == this);
	}

	public FunctionalComponent? FindOwnerComponent() {
		if (OwnerComponent != null) return OwnerComponent;
		if (Parent?.Element is not ParameterHolder ph || IsCustom()) return null;
		var key = ph.StandartParams.FirstOrDefault(kvp => kvp.Value == this).Key;
		if (key == null) return null;
		return ph.FunctionalComponents.FirstOrDefault(fc => key.StartsWith(fc.Name));
	}

	public T? GetTypedValue<T>() => Value.As<T>();

	public bool IsRef<T>() where T : VmElement => Value is RefValue<T> || Value is HierarchyRefValue<T>;
}