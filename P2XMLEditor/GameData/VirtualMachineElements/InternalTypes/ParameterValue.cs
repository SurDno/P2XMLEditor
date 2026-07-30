using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using P2XMLEditor.Core;
using P2XMLEditor.Forms.MainForm.Combinations;
using P2XMLEditor.GameData.Enums;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.GameData.VirtualMachineElements.Placeholders;
using P2XMLEditor.Helper;
using static P2XMLEditor.Parsing.Helpers.XElementExtensions;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes;

public abstract class ParameterValue {
	public abstract string XmlType { get; }
	public abstract string Serialize();
	public abstract bool Is<T>();
	public abstract T As<T>();

	private static ParameterValue? CreateRef<T>(VirtualMachine vm, string type, string value,
		bool nullable = false) where T : VmElement {
		if (string.IsNullOrEmpty(value)) return new RefValue<T>(type, null);

		if (value.Contains('H') && HierarchyGuid.TryParse(value, vm, out var hierarchy))
			return new HierarchyRefValue<T>(type, hierarchy!);

		if (Guid.TryParse(value, out _))
			return new RefValue<T>(type, vm.GetByEngineTemplateId(value) as T) { SerializeAsGuid = true };

		var id = ulong.Parse(value);
		var element = vm.GetNullableElement<T>(id);
		if (element == null && vm.ElementsById.GetValueOrDefault(id) == null) {
			VmElement placeholder = typeof(T) == typeof(State) ? new StatePlaceholder(id) : new ParameterPlaceholder(id);
			vm.Register(placeholder);
			return new RefValue<T>(type, placeholder as T);
		}
		return new RefValue<T>(type, element);
	}
	
	public static ParameterValue? Create(VirtualMachine vm, VmTypeInfo typeInfo, string value) {
		return Create(vm, typeInfo.Serialize(), value);
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
			"IObjRef" => CreateRef<ParameterHolder>(vm, type, value),
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
			"WeatherLayer" => new EnumValue<WeatherLayer>(type, value.Deserialize<WeatherLayer>()),
			"GameLocalizationName" => new EnumValue<GameLocalizationName>(type, value.Deserialize<GameLocalizationName>()),
			"GateState"            => new EnumValue<GateState>(type, value.Deserialize<GateState>()),
			"Notification"         => new EnumValue<NotificationType>(type, value.Deserialize<NotificationType>()),
			"PriorityParameter"    => new EnumValue<Priority>(type, value.Deserialize<Priority>()),
			"WeaponKind"           => new EnumValue<WeaponKind>(type, value.Deserialize<WeaponKind>()),
			"AttackerAttackType" => new EnumValue<AttackKind>(type, value.Deserialize<AttackKind>()),
			"AttackerNPCAttackType" => new EnumValue<NpcAttackKind>(type, value.Deserialize<NpcAttackKind>()),
			"AttackerPlayerAttackType" => new EnumValue<PlayerAttackKind>(type, value.Deserialize<PlayerAttackKind>()),
			"AttackerFinishType" => new EnumValue<FinishKind>(type, value.Deserialize<FinishKind>()),
			"AttackerDiseasedPlayerPushKind" => new EnumValue<PlayerPushesDiseasedKind>(type, value.Deserialize<PlayerPushesDiseasedKind>()),
			"DialogStringType" => new EnumValue<DialogStringEnum>(type, value.Deserialize<DialogStringEnum>()),
			"MailState" => new EnumValue<MailStateEnum>(type, value.Deserialize<MailStateEnum>()),
			
			"IBlueprintRef" => new BasicValue<Guid>(type, string.IsNullOrEmpty(value) ? Guid.Empty : Guid.Parse(value)), // todo: POSSIBLY store the wrapper?
			
			// Dynamic types
			_ when type.StartsWith("IObjRef%") => CreateRef<ParameterHolder>(vm, type, value),
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
	public bool SerializeAsGuid { get; set; }
	public T? TypedValue { get; set; } = value;
	public override string Serialize() => SerializeAsGuid ? (TypedValue as GameObject)?.EngineTemplateId ?? string.Empty
		: TypedValue?.Id.ToString() ?? string.Empty;
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