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

public class Parameter(ulong id) : VmElement(id), IFiller<RawParameterData>, ICommonVariableParameter, IVmCreator<Parameter> {
	public string Name { get; set; }
	public FunctionalComponent? OwnerComponent { get; set; }
	public bool? Implicit { get; set; }
	public VmEither<ParameterHolder, Expression> Parent { get; set; }
	public bool? Custom { get; set; }
	public ParameterValue Value { get; set; }

	public string Type => Value?.XmlType ?? string.Empty;
	public string SerializedValue => Value?.Serialize() ?? string.Empty;

	public override bool IsOrphaned() {
		return Parent.Element switch {
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
		Parent = vm.GetElement<ParameterHolder, Expression>(data.ParentId);
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
		switch (Parent.Element) {
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
		if (Custom.HasValue) return Custom.Value;
		if (Parent.Element is not ParameterHolder ph) return true;
		return ph.CustomParams != null && ph.CustomParams.Any(kvp => kvp.Value == this);
	}

	public FunctionalComponent? FindOwnerComponent() {
		if (OwnerComponent != null) return OwnerComponent;
		if (Parent.Element is not ParameterHolder ph || IsCustom()) return null;
		var key = ph.StandartParams.FirstOrDefault(kvp => kvp.Value == this).Key;
		return ph.FunctionalComponents.FirstOrDefault(fc => key.StartsWith(fc.Name));
	}

	public T? GetTypedValue<T>() => Value.As<T>();

	public bool IsRef<T>() where T : VmElement => Value is RefValue<T> || Value is HierarchyRefValue<T>;
}

public abstract class ParameterValue {
	public abstract string XmlType { get; }
	public abstract string Serialize();
	public abstract bool Is<T>();
	public abstract T As<T>();

	private static ParameterValue? CreateRef<T>(VirtualMachine vm, string type, string value, bool nullable = false) where T : VmElement {
		if (string.IsNullOrEmpty(value)) return new RefValue<T>(type, null);
		if (value.Contains('H') && HierarchyGuid.TryParse(value, vm, out var hierarchy)) {
			return new HierarchyRefValue<T>(type, hierarchy!);
		}
		var id = ulong.Parse(value);
		return new RefValue<T>(type, (nullable || id == 0) ? vm.GetNullableElement<T>(id) : vm.GetElement<T>(id));
	}

	public static ParameterValue? Create(VirtualMachine vm, string type, string value) {
		if (type == null) return null;

		return type switch {
			"System.Boolean" => new BasicValue<bool>(type, !string.IsNullOrEmpty(value) && bool.Parse(value)),
			"System.Int32" => new BasicValue<int>(type, string.IsNullOrEmpty(value) ? 0 : int.Parse(value)),
			"System.Single" => new BasicValue<float>(type, string.IsNullOrEmpty(value) ? 0f : float.Parse(value.Replace(",", "."), CultureInfo.InvariantCulture)),
			"System.String" => new BasicValue<string>(type, value ?? string.Empty),
			"System.UInt64" => new BasicValue<ulong>(type, string.IsNullOrEmpty(value) ? 0uL : ulong.Parse(value)),
			"GameTime" => new GameTimeValue(type, string.IsNullOrEmpty(value) ? TimeSpan.Zero : ParseTimeSpanString(value)),
			"ObjectCombinationDataStruct" => new CombinationDataValue(CombinationHelper.Parse(vm, value)),
			"ITextRef" => CreateRef<GameString>(vm, type, value),
			"IObjRef" => CreateRef<GameObject>(vm, type, value),
			"IStateRef" => CreateRef<State>(vm, type, value),
			"ISampleRef" => CreateRef<Sample>(vm, type, value),
			"IBehaviorObject" => CreateRef<Sample>(vm, type, value),
			"ILipSyncObject" => CreateRef<Sample>(vm, type, value),
			"IMapPlaceholder" => CreateRef<Sample>(vm, type, value),
			"IBlueprintObject" => CreateRef<Sample>(vm, type, value, true),
			"IBoundCharacterPlaceholder" => CreateRef<Sample>(vm, type, value),
			"IModel" => CreateRef<Sample>(vm, type, value),
			"ICfRef" => CreateRef<FunctionalComponent>(vm, type, value),
			"ISceneRef" => CreateRef<Scene>(vm, type, value),
			"IQuestRef" => CreateRef<Quest>(vm, type, value),
			"ICharacterRef" => CreateRef<Character>(vm, type, value),
			"IBranchRef" => CreateRef<Branch>(vm, type, value),
			
			"Area" => new EnumValue<Area>(type, value.Deserialize<Area>()),
			"LiquidTypeEnum" => new EnumValue<LiquidType>(type, value.Deserialize<LiquidType>()),
			"SpawnpointKindEnum" => new EnumValue<SpawnpointKind>(type, value.Deserialize<SpawnpointKind>()),
			"StammKind" => new EnumValue<StammKind>(type, value.Deserialize<StammKind>()),
			"StorableGroupEnum" => new EnumValue<StorableGroup>(type, value.Deserialize<StorableGroup>()),
			"InteractType" => new EnumValue<InteractType>(type, value.Deserialize<InteractType>()),
			"CombatStyleEnum" => new EnumValue<CombatStyle>(type, value.Deserialize<CombatStyle>()),
			"BuildingEnum" => new EnumValue<BuildingType>(type, value.Deserialize<BuildingType>()),
			"BlockType" => new EnumValue<BlockType>(type, value.Deserialize<BlockType>()),
			"DiseasedStateEnum" => new EnumValue<DiseasedStateType>(type, value.Deserialize<DiseasedStateType>()),
			"BoundHealthStateEnum" => new EnumValue<BoundHealthState>(type, value.Deserialize<BoundHealthState>()),
			"FractionEnum" => new EnumValue<FractionEnum>(type, value.Deserialize<FractionEnum>()),
			"OutdoorCrowdLayout" => new EnumValue<OutdoorCrowdLayout>(type, value.Deserialize<OutdoorCrowdLayout>()),
			"CombatAction" => new EnumValue<CombatActionEnum>(type, value.Deserialize<CombatActionEnum>()),
			"ContainerOpenState" => new EnumValue<ContainerOpenState>(type, value.Deserialize<ContainerOpenState>()),
			"GateLockState" => new EnumValue<LockState>(type, value.Deserialize<LockState>()),
			"FastTravelPointEnum" => new EnumValue<FastTravelPoint>(type, value.Deserialize<FastTravelPoint>()),
			"JerboaColorEnum" => new EnumValue<JerboaColor>(type, value.Deserialize<JerboaColor>()),
			"BoundCharacterGroup" => new EnumValue<BoundCharacterGroupEnum>(type, value.Deserialize<BoundCharacterGroupEnum>()),
			"IBlueprintRef" => new BasicValue<Guid>(type, string.IsNullOrEmpty(value) ? Guid.Empty : Guid.Parse(value)),
			
			// Dynamic types
			_ when type.StartsWith("IObjRef%") => CreateRef<GameObject>(vm, type, value),
			_ when type.StartsWith("CommonList%") => new CommonListValue(type, ParseCommonList(vm, value)),
			
			_ => new UnknownValue(type, value)
		};
	}

	private static List<ParameterValue> ParseCommonList(VirtualMachine vm, string value) {
		if (string.IsNullOrEmpty(value)) return [];

		if (!value.Contains("LIST&ELEM") && !value.Contains("value_")) {
			return value.Split('%', StringSplitOptions.RemoveEmptyEntries)
				.Select(idStr => Create(vm, "IObjRef", idStr))
				.Where(v => v != null).Cast<ParameterValue>().ToList();
		}

		var elements = new List<ParameterValue>();
		var parts = value.Split(["LIST&ELEM"], StringSplitOptions.RemoveEmptyEntries);
		foreach (var part in parts) {
			var typeIdx = part.IndexOf("_type_");
			if (typeIdx == -1) continue;

			var itemType = part[(typeIdx + 6)..];
			var itemData = part.Substring(6, typeIdx - 6);

			var item = Create(vm, itemType, itemData);
			if (item != null) elements.Add(item);
		}
		return elements;
	}
}

public class GameTimeValue(string type, TimeSpan value) : ParameterValue {
	public override string XmlType => type;
	public TimeSpan TypedValue { get; set; } = value;
	public override string Serialize() => $"{TypedValue.Days}:{TypedValue.Hours}:{TypedValue.Minutes}:{TypedValue.Seconds}";
	public override bool Is<TVal>() => TypedValue is TVal;
	public override TVal As<TVal>() => TypedValue is TVal v ? v : default!;
}

public class BasicValue<T>(string type, T value) : ParameterValue {
	public override string XmlType => type;
	public T TypedValue { get; set; } = value;
	public override string Serialize() {
		if (TypedValue is Guid guid) return guid.ToString("N");
		return TypedValue?.ToString() ?? string.Empty;
	}
	public override bool Is<TVal>() => TypedValue is TVal;
	public override TVal As<TVal>() => TypedValue is TVal v ? v : default!;
}

public class UnknownValue(string type, string value) : ParameterValue {
	public override string XmlType => type;
	public string TypedValue { get; set; } = value;
	public override string Serialize() => TypedValue ?? string.Empty;
	public override bool Is<TVal>() => TypedValue is TVal;
	public override TVal As<TVal>() => TypedValue is TVal v ? v : default!;
}

public class RefValue<T>(string type, T? value) : ParameterValue where T : VmElement {
	public override string XmlType => type;
	public T? TypedValue { get; set; } = value;
	public override string Serialize() => TypedValue?.Id.ToString() ?? string.Empty;
	public override bool Is<TVal>() => TypedValue is TVal;
	public override TVal As<TVal>() => TypedValue is TVal v ? v : default!;
}

public class HierarchyRefValue<T>(string type, HierarchyGuid hierarchy) : ParameterValue where T : VmElement {
	public override string XmlType => type;
	public HierarchyGuid Hierarchy { get; set; } = hierarchy;
	public T? TypedValue => Hierarchy.Elements[^1].Element as T;
	public override string Serialize() => Hierarchy.Write();
	public override bool Is<TVal>() => TypedValue is TVal;
	public override TVal As<TVal>() => TypedValue is TVal v ? v : default!;
}

public class EnumValue<T>(string type, T value) : ParameterValue where T : struct, Enum {
	public override string XmlType => type;
	public T TypedValue { get; set; } = value;
	public override string Serialize() => TypedValue.Serialize();
	public override bool Is<TVal>() => TypedValue is TVal;
	public override TVal As<TVal>() => TypedValue is TVal v ? v : default!;
}

public class CombinationDataValue(List<ICombinationPart> parts) : ParameterValue {
	public override string XmlType => "ObjectCombinationDataStruct";
	public List<ICombinationPart> TypedValue { get; set; } = parts;
	public override string Serialize() => CombinationHelper.Serialize(TypedValue);
	public override bool Is<TVal>() => TypedValue is TVal;
	public override TVal As<TVal>() => TypedValue is TVal v ? v : default!;
}

public class CommonListValue(string type, List<ParameterValue> elements) : ParameterValue {
	public override string XmlType => type;
	public List<ParameterValue> TypedValue { get; set; } = elements;
	public override string Serialize() => string.Join("LIST&ELEM", TypedValue.Select(e => $"value_{e.Serialize()}_type_{e.XmlType}"));
	public override bool Is<TVal>() => TypedValue is TVal;
	public override TVal As<TVal>() => TypedValue is TVal v ? v : default!;
}