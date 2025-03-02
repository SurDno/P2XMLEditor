using System.ComponentModel;
using P2XMLEditor.Attributes;

namespace P2XMLEditor.GameData.VirtualMachineElements.Enums;

[TypeConverter(typeof(EnumConverter))]
public enum SampleType {
	[SerializationData("IModel")] IModel,
	[SerializationData("IBehaviorObject")] IBehaviorObject,
	[SerializationData("MindMap2.IPlaceholder")] MindMapPicture,
	[SerializationData("ISnapshot")] ISnapshot,
	[SerializationData("IMapPlaceholder")] IMapPlaceholder,
	[SerializationData("IBlueprintObject")] IBlueprintObject,
	[SerializationData("ILipSyncObject")] ILipSyncObject,
	[SerializationData("IBoundCharacterPlaceholder")] IBoundCharacterPlaceholder,
	[SerializationData("IMapTooltipResource")] IMapTooltipResource
}