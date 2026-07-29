using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using P2XMLEditor.Attributes;
using P2XMLEditor.Core;
using P2XMLEditor.GameData;
using P2XMLEditor.GameData.Enums;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes;
using P2XMLEditor.GameData.VirtualMachineElements.Placeholders;
using P2XMLEditor.Logging;
using VmType = P2XMLEditor.GameData.Enums.VmType;

namespace P2XMLEditor.Helper;

public static class VmTypeHelper {
	private static readonly Dictionary<VmComponent, string> ComponentToXml;
	private static readonly Dictionary<string, VmComponent> XmlToComponent;
	private static readonly VmComponent[] ValidComponents;
	private static readonly Dictionary<VmType, Type> VmToSystemType;

	static VmTypeHelper() {
		ComponentToXml = new Dictionary<VmComponent, string>();
		XmlToComponent = new Dictionary<string, VmComponent>(StringComparer.OrdinalIgnoreCase);
		VmToSystemType = new Dictionary<VmType, Type> {
			[VmType.Boolean] = typeof(bool),
			[VmType.Int32] = typeof(int),
			[VmType.Single] = typeof(float),
			[VmType.String] = typeof(string),
			[VmType.UInt64] = typeof(ulong),
			[VmType.GameTime] = typeof(GameTime),
			[VmType.GameMode] = typeof(GameMode),
			[VmType.MindMap] = typeof(MindMap),
			[VmType.MindMapNode] = typeof(MindMapNode),
			[VmType.GameString] = typeof(GameString),
			[VmType.GameObject] = typeof(GameObject),
			[VmType.State] = typeof(State),
			[VmType.Talking] = typeof(Talking),
			[VmType.Sample] = typeof(Sample),
			[VmType.SampleRefModel] = typeof(Sample),
			[VmType.SampleRefLipSync] = typeof(Sample),
			[VmType.SampleRefSnapshot] = typeof(Sample),
			[VmType.BehaviorObject] = typeof(Sample),
			[VmType.LipSyncObject] = typeof(Sample),
			[VmType.MapPlaceholder] = typeof(Sample),
			[VmType.MapTooltipResource] = typeof(Sample),
			[VmType.BlueprintObject] = typeof(Sample),
			[VmType.SampleRefBlueprint] = typeof(Sample),
			[VmType.BoundCharacterPlaceholder] = typeof(Sample),
			[VmType.Model] = typeof(Sample),
			[VmType.Scene] = typeof(Scene),
			[VmType.Quest] = typeof(Quest),
			[VmType.Character] = typeof(Character),
			[VmType.Branch] = typeof(Branch),
			[VmType.BlueprintRef] = typeof(Guid),
			[VmType.BlueprintRefStorable] = typeof(Guid),
			[VmType.EntityRef] = typeof(Guid),
			[VmType.List] = typeof(CommonList),
			[VmType.Area] = typeof(Area),
			[VmType.LiquidTypeEnum] = typeof(LiquidType),
			[VmType.SpawnpointKindEnum] = typeof(SpawnpointKind),
			[VmType.StammKind] = typeof(StammKind),
			[VmType.StorableGroupEnum] = typeof(StorableGroup),
			[VmType.InteractType] = typeof(InteractType),
			[VmType.CombatStyleEnum] = typeof(CombatStyle),
			[VmType.BuildingEnum] = typeof(BuildingType),
			[VmType.BlockType] = typeof(BlockType),
			[VmType.DiseasedStateEnum] = typeof(DiseasedStateType),
			[VmType.BoundHealthStateEnum] = typeof(BoundHealthState),
			[VmType.FractionEnum] = typeof(FractionEnum),
			[VmType.OutdoorCrowdLayout] = typeof(OutdoorCrowdLayout),
			[VmType.CombatAction] = typeof(CombatActionEnum),
			[VmType.ContainerOpenState] = typeof(ContainerOpenState),
			[VmType.GateLockState] = typeof(LockState),
			[VmType.FastTravelPointEnum] = typeof(FastTravelPoint),
			[VmType.JerboaColorEnum] = typeof(JerboaColor),
			[VmType.BoundCharacterGroup] = typeof(BoundCharacterGroupEnum),
			[VmType.PriorityParameterEnum] = typeof(Priority),
			[VmType.NotificationEnum] = typeof(NotificationType),
			[VmType.WeaponKind] = typeof(WeaponKind),
			[VmType.GateState] = typeof(GateState),
			[VmType.GameLocalizationName] = typeof(GameLocalizationName),
			[VmType.WeatherLayer] = typeof(WeatherLayer),
			[VmType.AttackerAttackType] = typeof(AttackKind),
			[VmType.AttackerNPCAttackType] = typeof(NpcAttackKind),
			[VmType.AttackerPlayerAttackType] = typeof(PlayerAttackKind),
			[VmType.AttackerFinishType] = typeof(FinishKind),
			[VmType.AttackerDiseasedPlayerPushKind] = typeof(PlayerPushesDiseasedKind),
			[VmType.DialogStringType] = typeof(DialogStringEnum),
			[VmType.MailState] = typeof(MailStateEnum),
		};
		var fields = typeof(VmComponent).GetFields(BindingFlags.Static | BindingFlags.Public);
		foreach (var obj in fields) {
			var customAttribute = obj.GetCustomAttribute<ComponentAttribute>();
			var vmComponent = (VmComponent)obj.GetValue(null);
			ComponentToXml[vmComponent] = customAttribute.Value;
			XmlToComponent[customAttribute.Value] = vmComponent;
		}

		ValidComponents = (from x in Enum.GetValues<VmComponent>()
			where x != VmComponent.None
			select x).ToArray();
	}

	public static string SerializeComponent(VmComponent component) {
		if (!ComponentToXml.TryGetValue(component, out var value)) {
			return component.ToString();
		}

		return value;
	}

	public static VmComponent DeserializeComponent(string value) {
		if (!string.IsNullOrEmpty(value)) {
			return XmlToComponent.GetValueOrDefault(value, VmComponent.None);
		}

		return VmComponent.None;
	}

	public static VmTypeInfo GetVmTypeInfo(string xmlType, VirtualMachine vm) {
		if (xmlType.StartsWith("CommonList%")) {
			var vmTypeInfo = new VmTypeInfo(VmType.List);
			var text = xmlType;
			vmTypeInfo.UnderlyingType = GetVmTypeInfo(text.Substring(11, text.Length - 11), vm);
			return vmTypeInfo;
		}

		var vmType = xmlType.DeserializeNoNewValues(VmType.Unknown);
		if (vmType != VmType.Unknown) {
			return new VmTypeInfo(vmType);
		}

		var vmTypeInfo2 = new VmTypeInfo(VmType.Unknown);
		var text2 = xmlType;
		if (xmlType.StartsWith("IObjRef")) {
			vmTypeInfo2.BaseType = VmType.GameObject;
			var text = xmlType;
			text2 = text.Substring(7, text.Length - 7);
			if (text2.StartsWith("%cf_")) {
				var num = text2.IndexOf('&');
				if (num == -1) {
					num = text2.Length;
				}

				var text3 = text2.Substring(4, num - 4);
				if (ulong.TryParse(text3, out var result)) {
					vmTypeInfo2.ObjBlueprint = vm.GetNullableElement<ParameterHolder>(result) ??
					                           vm.Register(new CharacterPlaceholder(result));
				} else {
					var vmComponent = DeserializeComponent(text3);
					if (vmComponent != VmComponent.None) {
						vmTypeInfo2.RequiredComponents.Add(vmComponent);
					}
				}

				text = text2;
				var num2 = num;
				text2 = text.Substring(num2, text.Length - num2);
			} else if (text2.StartsWith("%")) {
				var array = text2.Split('%', StringSplitOptions.RemoveEmptyEntries);
				if (array.Length != 0) {
					var vmType2 = array[0].DeserializeNoNewValues(VmType.Unknown);
					if (vmType2 != VmType.Unknown) {
						vmTypeInfo2.BaseType = vmType2;
						text = text2;
						var num2 = array[0].Length + 1;
						text2 = text.Substring(num2, text.Length - num2);
					}
				}
			}
		} else {
			Logger.Log(LogLevel.Debug, $"Encountered unknown VmType: {xmlType}");
		}

		if (text2.Contains('&')) {
			var array2 = text2.Split('&', StringSplitOptions.RemoveEmptyEntries);
			for (var num2 = 0; num2 < array2.Length; num2++) {
				var vmComponent2 = DeserializeComponent(array2[num2]);
				if (vmComponent2 != VmComponent.None) {
					vmTypeInfo2.RequiredComponents.Add(vmComponent2);
				}
			}
		}

		return vmTypeInfo2;
	}

	public static string ToXmlType(VmTypeInfo info) {
		if (info is { BaseType: VmType.List }) {
			if (info.UnderlyingType == null) {
				return "CommonList";
			}

			return "CommonList%" + ToXmlType(info.UnderlyingType);
		}

		if (info.BaseType == VmType.GameObject) {
			var text = "IObjRef";
			var text2 = "&";
			if (info.ObjBlueprint != null) {
				text = text + "%cf_" + info.ObjBlueprint.Id;
			} else if (!info.RequiredComponents.IsEmpty) {
				text2 = "%cf_";
			}

			if (info.RequiredComponents.IsEmpty) {
				return text;
			}

			if (info.RequiredComponents.IsOrdered) {
				var validComponents = ValidComponents;
				foreach (var vmComponent in validComponents) {
					if ((info.RequiredComponents.Mask & vmComponent) != VmComponent.None) {
						text = text + text2 + SerializeComponent(vmComponent);
						text2 = "&";
					}
				}
			} else {
				var validComponents = info.RequiredComponents.CustomOrder;
				foreach (var component in validComponents) {
					text = text + text2 + SerializeComponent(component);
					text2 = "&";
				}
			}

			return text;
		}

		return info.BaseType.Serialize();
	}

	public static VmType GetVmType(string xmlType, VirtualMachine vm) => GetVmTypeInfo(xmlType, vm).BaseType;

	public static VmType GetVmType(Type type) {
		foreach (var item in VmToSystemType) {
			if (item.Value == type) {
				return item.Key;
			}
		}

		if (type == typeof(EntityRef)) {
			return VmType.EntityRef;
		}

		if (type == typeof(BlueprintRef)) {
			return VmType.BlueprintRef;
		}


		if (typeof(VmElement).IsAssignableFrom(type)) {
			if (type == typeof(GameObject)) {
				return VmType.GameObject;
			}

			if (type == typeof(GameMode)) {
				return VmType.GameMode;
			}
			if (type == typeof(MindMap)) {
				return VmType.MindMap;
			}

			if (type == typeof(MindMapNode)) {
				return VmType.MindMapNode;
			}

			if (type == typeof(CommonList)) {
				return VmType.List;
			}

			if (type == typeof(State)) {
				return VmType.State;
			}

			if (type == typeof(Talking)) {
				return VmType.Talking;
			}

			if (type == typeof(Sample)) {
				return VmType.Sample;
			}

			if (type == typeof(Scene)) {
				return VmType.Scene;
			}

			if (type == typeof(Quest)) {
				return VmType.Quest;
			}

			if (type == typeof(Character)) {
				return VmType.Character;
			}

			if (type == typeof(Branch)) {
				return VmType.Branch;
			}

			if (type == typeof(Area)) {
				return VmType.Area;
			}
		}

		return VmType.Unknown;
	}

	public static string GetXmlType(VmType type) => type.Serialize();
	public static Type? GetSystemType(VmType type) => VmToSystemType.GetValueOrDefault(type);
	public static Type? ResolveType(string xmlType, VirtualMachine vm) => GetSystemType(GetVmType(xmlType, vm));

	public static bool IsVmElement(string xmlType, VirtualMachine vm) {
		var type = ResolveType(xmlType, vm);
		if (type != null) {
			return typeof(VmElement).IsAssignableFrom(type);
		}

		return false;
	}

	public static VmTypeInfo GetUnderlyingTypeInfo(string commonListType, VirtualMachine vm) {
		if (string.IsNullOrEmpty(commonListType) || !commonListType.StartsWith("CommonList%")) {
			return VmTypeInfo.Unknown;
		}

		return GetVmTypeInfo(commonListType.Substring(11, commonListType.Length - 11), vm);
	}

	public static VmTypeInfo GetVmTypeInfo(Type type) => new VmTypeInfo(GetVmType(type));
}
