using System.Reflection;
using P2XMLEditor.Attributes;
using P2XMLEditor.Logging;

namespace P2XMLEditor.Helper;

public static class EnumExtensions {
	private static readonly Dictionary<Type, Dictionary<int, string>> SerializeCache = new();
	private static readonly Dictionary<Type, Dictionary<string, int>> DeserializeCache = new();

	[PerformanceLogHook]
	static EnumExtensions() {
		var asm = Assembly.GetExecutingAssembly();

		foreach (var type in asm.GetTypes()) {
			if (!type.IsEnum) continue;
			if (!Attribute.IsDefined(type, typeof(SerializationEnum), false)) continue;

			var fields = type.GetFields(BindingFlags.Public | BindingFlags.Static);
			var sDict = new Dictionary<int, string>(fields.Length);
			var dDict = new Dictionary<string, int>(fields.Length);

			foreach (var field in fields) {
				var attr = field.GetCustomAttribute<SerializationData>()!;
				int intVal = (int)field.GetValue(null)!;
				sDict[intVal] = attr.Value;
				dDict[attr.Value] = intVal;
			}

			SerializeCache[type] = sDict;
			DeserializeCache[type] = dDict;
		}
	}

	public static string Serialize(this Enum value) {
		var t = value.GetType();
		int key = Convert.ToInt32(value);

		if (SerializeCache.TryGetValue(t, out var map) && map.TryGetValue(key, out var s))
			return s;

		throw new ArgumentException($"No serialization data for {value}");
	}

	public static T Deserialize<T>(this string value) where T : Enum {
		var t = typeof(T);
		if (DeserializeCache.TryGetValue(t, out var map)) {
			if (map.TryGetValue(value, out var raw))
				return (T)Enum.ToObject(t, raw);

			return (T)AssignUnknownValue(t, value);
		}

		throw new ArgumentException($"Enum type {t.Name} not initialized in cache");
	}

	public static Enum Deserialize(this Type enumType, string value) {
		if (DeserializeCache.TryGetValue(enumType, out var map)) {
			if (map.TryGetValue(value, out var raw))
				return (Enum)Enum.ToObject(enumType, raw);

			return AssignUnknownValue(enumType, value);
		}

		throw new ArgumentException($"Enum type {enumType.Name} not initialized in cache");
	}

	private static Enum AssignUnknownValue(Type enumType, string value) {
		var allValues = Enum.GetValues(enumType).Cast<Enum>().ToArray();
		var used = SerializeCache[enumType];
		var free = allValues
			.Select(ev => Convert.ToInt32(ev))
			.FirstOrDefault(intVal => !used.ContainsKey(intVal));

		if (!Enum.IsDefined(enumType, free))
			throw new InvalidOperationException($"No free value available in {enumType.Name} to map '{value}'");

		SerializeCache[enumType][free] = value;
		DeserializeCache[enumType][value] = free;

		Logger.Log(LogLevel.Error, $"An unknown value {value} in enum {enumType.Name}! Preserving directly as " +
		                           $"value {free}. Note that it may cause issues.");

		return (Enum)Enum.ToObject(enumType, free);
	}
}
