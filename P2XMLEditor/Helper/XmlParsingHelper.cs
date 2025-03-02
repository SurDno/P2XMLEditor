using System.Xml.Linq;

namespace P2XMLEditor.Helper;

// TODO: Separate into separate classes handling reading and writing operations.
public static class XmlParsingHelper {
    public static XElement CreateBaseElement(string id) => 
        new("Item", new XAttribute("id", id));
    
    public static XElement CreateSelfClosingElement(string name, string? value) =>
        string.IsNullOrEmpty(value) ? new(name) : new(name, value);

    public static XElement CreateBoolElement(string name, bool value) => new(name, value ? "True" : "False");

    public static XElement? CreateListElement(string name, IEnumerable<string>? items) {
        if (items?.Any() != true) return null;
        return new(name, new XAttribute("count", items.Count()), 
            items.Select(x => new XElement("Item", x)));
    }
    
    public static XElement? CreateDictionaryElement(string name, Dictionary<string, string>? items) {
        if (items?.Any() != true) return null;
        return new(name, new XAttribute("count", items.Count),
            items.Select(x => new XElement("Item", new XAttribute("key", x.Key), x.Value)));
    }

    public static XElement GetRequiredElement(XElement parent, string name) =>
        parent.Element(name) ?? throw new ArgumentException($"Required element {name} missing");

    public static bool ParseBool(XElement element) => 
        element.Value.Equals("True", StringComparison.OrdinalIgnoreCase);
    
    public static int ParseInt(XElement element) => int.Parse(element.Value);
    public static float ParseFloat(XElement element) => float.Parse(element.Value);
    public static long ParseLong(XElement element) => long.Parse(element.Value);
    public static TimeSpan ParseTimeSpan(XElement element) {
        var parts = element.Value.Split(':').Select(int.Parse).ToArray();
        return new(parts[0], parts[1], parts[2], parts[3]);
    }

    public static List<string> ParseListElement(XElement? parent, string elementName) =>
        parent?.Element(elementName)?.Elements("Item").Select(x => x.Value).ToList() ?? [];

    public static Dictionary<string, string> ParseDictionaryElement(XElement? parent, string elementName) {
        var dict = new Dictionary<string, string>();
        var element = parent?.Element(elementName);
        if (element != null) {
            foreach (var item in element.Elements("Item")) {
                var key = item.Attribute("key");
                if (key != null) dict[key.Value] = item.Value;
            }
        }
        return dict;
    }
    
    public static T? Let<T>(this XElement? element, Func<XElement, T> transform) => 
        element != null ? transform(element) : default;
}