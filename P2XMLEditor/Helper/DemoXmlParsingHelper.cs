using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Xml.Linq;

namespace P2XMLEditor.Helper;

public static class DemoXmlParsingHelper {
	
	public static XElement CreateDemoBaseElement(ulong id) =>
		new("object", new XAttribute("id", id));

	public static XElement? CreateDemoStringElement(string name, string? value) =>
		string.IsNullOrEmpty(value) ? null : new(name, value);

	public static XElement CreateDemoBoolElement(string name, bool value) => 
		new(name, value ? "True" : "False");

	public static XElement CreateDemoFloatElement(string name, float value) =>
		new(name, value.ToString().Replace('.', ','));

	public static XElement CreateDemoVector3Element(string name, Vector3 vec) => 
		new(name, $"{vec.X.ToString().Replace('.', ',')} {vec.Y.ToString().Replace('.', ',')} {vec.Z.ToString().Replace('.', ',')}");

	public static XElement CreateDemoListElement(string name, IEnumerable<string>? items) {
		var listName = name + ".List";
		if (items == null || !items.Any()) return new XElement(listName);
		return new XElement(listName, items.Select(x => new XElement("value", x)));
	}

	public static XElement CreateDemoListElementAsLong(string name, IEnumerable<ulong>? items) {
		var listName = name + ".List";
		if (items == null || !items.Any()) return new XElement(listName);
		return new XElement(listName, items.Select(x => new XElement("value", x)));
	}

	public static XElement CreateDemoDictElement(string name, Dictionary<string, string>? items) {
		var dictName = name + ".Dict";
		if (items == null || !items.Any()) return new XElement(dictName);
		return new XElement(dictName, items.Select(x => new XElement(x.Key, x.Value)));
	}

	public static XElement CreateDemoDictElementAsLong(string name, (string Key, ulong Value)[]? items) {
		var dictName = name + ".Dict";
		if (items == null || !items.Any()) return new XElement(dictName);
		return new XElement(dictName, items.Select(x => new XElement(x.Key, x.Value)));
	}

	public static XElement CreateGuidElement(ulong id) => new("Guid", id.ToString());
}
