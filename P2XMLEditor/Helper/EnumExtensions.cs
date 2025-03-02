using System.Reflection;
using P2XMLEditor.Attributes;

namespace P2XMLEditor.Helper;

public static class EnumExtensions {
	public static string Serialize(this Enum value) => 
		value.GetType().GetMember(value.ToString())[0].GetCustomAttribute<SerializationData>()?.Value ??
		throw new ArgumentException($"No serialization data for {value}");

	public static T Deserialize<T>(this string value) where T : Enum => 
		Enum.GetValues(typeof(T)).Cast<T>().FirstOrDefault(e => e.Serialize() == value) ??
		throw new ArgumentException($"No serialization data for {value}");
}