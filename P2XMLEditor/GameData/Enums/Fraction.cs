using System.ComponentModel;
using P2XMLEditor.Attributes;

namespace P2XMLEditor.GameData.Enums;

[TypeConverter(typeof(EnumConverter))]
[SerializationEnum]
public enum FractionEnum {
	[SerializationData("None")] None,
	[SerializationData("Player")] Player,
	[SerializationData("PlayerLowReputation")] PlayerLowReputation,
	[SerializationData("Child")] Child,
	[SerializationData("Civilian")] Civilian,
	[SerializationData("Bandit")] Bandit,
	[SerializationData("Military")] Military,
	[SerializationData("Diseased")] Diseased,
	[SerializationData("Witch")] Witch,
	[SerializationData("Doghead")] Doghead,
	[SerializationData("Soulandahalve")] SoulAndAHalve,
	[SerializationData("Civilian_Angry")] CivilianAngry,
	[SerializationData("Civilian_WitchHunter")] CivilianWitchHunter,
	[SerializationData("DiseaseCloud")] DiseaseCloud,
	[SerializationData("Gypsy")] Gypsy,
	[SerializationData("Soldier")] Soldier,
	[SerializationData("Sanitar")] Sanitar,
	[SerializationData("Pateint_Town")] PatientTown,
	[SerializationData("Patient_Hospital")] PatientHospital,
	[SerializationData("Bound")] Bound,
	[SerializationData("Arsonist")] Arsonist,
}