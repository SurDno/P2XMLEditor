using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using P2XMLEditor.Attributes;

namespace P2XMLEditor.Helper;

public static class EnumExtensions {
	private static readonly Dictionary<Type, Dictionary<int, string>> SerializeCache = new();
	private static readonly Dictionary<Type, Dictionary<string, int>> DeserializeCache = new();

	static EnumExtensions() {
		var asm = Assembly.GetExecutingAssembly();

		foreach (var type in asm.GetTypes()) {
			if (!type.IsEnum) continue;
			if (!Attribute.IsDefined(type, typeof(SerializationEnum), false)) continue;

			var fields = type.GetFields(BindingFlags.Public | BindingFlags.Static);
			var sDict = new Dictionary<int, string>(fields.Length);
			var dDict = new Dictionary<string, int>(fields.Length);
			var arr = new (string text, int raw)[fields.Length];

			for (var i = 0; i < fields.Length; i++) {
				var f = fields[i];
				var attr = f.GetCustomAttribute<SerializationData>()!;
				var intVal = (int)f.GetValue(null)!;

				sDict[intVal] = attr.Value;
				dDict[attr.Value] = intVal;

				arr[i] = (attr.Value, intVal);
			}

			SerializeCache[type] = sDict;
			DeserializeCache[type] = dDict;
		}
	}

	public static string Serialize(this Enum value) {
		var t = value.GetType();
		var key = Convert.ToInt32(value);

		if (SerializeCache.TryGetValue(t, out var map) && map.TryGetValue(key, out var s))
			return s;

		throw new ArgumentException($"No serialization data for {value}");
	}

	public static T Deserialize<T>(this string value) where T : Enum {
		var t = typeof(T);
		if (!DeserializeCache.TryGetValue(t, out var map))
			throw new ArgumentException($"Enum type {t.Name} not initialized in cache");
		
		if (map.TryGetValue(value, out var raw))
			return Unsafe.As<int, T>(ref raw);
		return (T)AssignUnknownValue(t, value);
	}
	public static T DeserializeNoNewValues<T>(this string value, T defaultValue = default(T)) where T : Enum {
		var t = typeof(T);
		if (!DeserializeCache.TryGetValue(t, out var map))
			throw new ArgumentException($"Enum type {t.Name} not initialized in cache");
		if (map.TryGetValue(value, out var raw))
			return Unsafe.As<int, T>(ref raw);
		return defaultValue;
	}
	public static Enum Deserialize(this Type enumType, string value) {
		if (!DeserializeCache.TryGetValue(enumType, out var map))
			throw new ArgumentException($"Enum type {enumType.Name} not initialized in cache");
		
		if (map.TryGetValue(value, out var raw))
			return (Enum)Enum.ToObject(enumType, raw);

		return AssignUnknownValue(enumType, value);
	}

	private static Enum AssignUnknownValue(Type enumType, string value) {
		var used = SerializeCache[enumType];
		int? free = Enum.GetValues(enumType).Cast<Enum>()
			.Select(Convert.ToInt32)
			.Where(intVal => !used.ContainsKey(intVal))
			.Cast<int?>()
			.FirstOrDefault();

		if (free == null)
			throw new InvalidOperationException(
				$"No free value in {enumType.Name} to map '{value}' — every declared member is " +
				$"already mapped. The enum is missing members present in the data.");

		SerializeCache[enumType][free.Value] = value;
		DeserializeCache[enumType][value] = free.Value;
		return (Enum)Enum.ToObject(enumType, free.Value);
	}
}
