using System.ComponentModel;
using P2XMLEditor.Attributes;

namespace P2XMLEditor.GameData.Enums;

[TypeConverter(typeof(EnumConverter))]
[SerializationEnum]
public enum NotificationType {
	[SerializationData("None")] None = 0,
	[SerializationData("Main_Layer")] MainLayer = 1024,
	[SerializationData("Map")] Map = 1025,
	[SerializationData("MindMap")] MindMap = 1026,
	[SerializationData("Stats")] Stats = 1027,
	[SerializationData("BoundCharacters")] BoundCharacters = 1028,
	[SerializationData("Tooltip_Layer")] TooltipLayer = 2048,
	[SerializationData("Tooltip")] Tooltip = 2049,
	[SerializationData("Text")] Text = 2050,
	[SerializationData("LargeText")] LargeText = 2051,
	[SerializationData("Reputation_Layer")] ReputationLayer = 3072,
	[SerializationData("Reputation")] Reputation = 3073,
	[SerializationData("Foundation")] Foundation = 3074,
	[SerializationData("Item_Layer")] ItemLayer = 4096,
	[SerializationData("ItemRecieve")] ItemRecieve = 4097,
	[SerializationData("ItemDrop")] ItemDrop = 4098,
	[SerializationData("ItemBroken")] ItemBroken = 4099,
	[SerializationData("Region_Layer")] RegionLayer = 5120,
	[SerializationData("Region")] Region = 5121,
	[SerializationData("MindMap_Layer")] MindMapLayer = 6144,
	[SerializationData("MindMapNode")] MindMapNode = 6145
}
