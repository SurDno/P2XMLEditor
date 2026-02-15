using System.ComponentModel;

namespace P2XMLEditor.Enums.Templates;

[TypeConverter(typeof(EnumConverter)), SerializationEnum]
public enum WeaponKind {
	[SerializationData("Unknown")] Unknown,
	[SerializationData("Hands")] Hands,
	[SerializationData("Knife")] Knife,
	[SerializationData("Revolver")] Revolver,
	[SerializationData("Rifle")] Rifle,
	[SerializationData("Visir")] Plaguefinder,
	[SerializationData("Flashlight")] Flashlight,
	[SerializationData("Scalpel")] Scalpel,
	[SerializationData("Lockpick")] Lockpick,
	[SerializationData("Bomb")] Bomb,
	[SerializationData("Samopal")] HandmadeGun,
	[SerializationData("Shotgun")] Shotgun,
}