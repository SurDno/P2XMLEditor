using System.Numerics;
using System.Xml.Linq;

namespace P2XMLEditor.Parsing.Helpers;

public static class XElementExtensions {
	
    public static XElement GetRequiredElement(XElement parent, string name) =>
        parent.Element(name) ?? throw new ArgumentException($"Required element {name} missing");

    public static bool ParseBool(XElement element) =>
        element.Value.Equals("True", StringComparison.OrdinalIgnoreCase);

    public static int ParseInt(this XElement element) => int.Parse(element.Value);
    public static float ParseFloat(this XElement element) => float.Parse(element.Value);
    public static ulong ParseULong(this XElement element) => ulong.Parse(element.Value);

    public static Vector3 ParseVector3(this XElement element) {
        var parts = element.Value.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return new Vector3(float.Parse(parts[0]), float.Parse(parts[1]), float.Parse(parts[2]));
    }

    public static TimeSpan ParseTimeSpan(XElement element) => ParseTimeSpan(element.Value);

    public static TimeSpan ParseTimeSpan(string timeSpanAsText) {
        var parts = timeSpanAsText.Split(':');
        var tail = TimeSpan.Parse("0:0:" + parts[2] + ":" + parts[3]);
        return new TimeSpan(int.Parse(parts[0]), int.Parse(parts[1]), 0, 0) + tail;
    }

    public static List<string> ParseListElement(XElement? parent, XName elementName) =>
        parent?.Element(elementName)?.Elements(XNameCache.Item).Select(x => x.Value).ToList() ?? [];
    
    public static List<ulong> ParseListElementAsUlong(XElement? parent, XName elementName) =>
        parent?.Element(elementName)?.Elements(XNameCache.Item).Select(x => ulong.Parse(x.Value)).ToList() ?? [];

    public static Dictionary<string, string> ParseDictionaryElement(XElement? parent, XName elementName) {
        var dict = new Dictionary<string, string>();
        var element = parent?.Element(elementName);
        if (element != null) {
            foreach (var item in element.Elements(XNameCache.Item)) {
                var key = item.Attribute(XNameCache.KeyAttribute);
                if (key != null) dict[key.Value] = item.Value;
            }
        }
        return dict;
    }
    
    public static List<ulong> ReadULongList(XElement element) {
        return element.Elements(XNameCache.Item).Select(x => ulong.Parse(x.Value)).ToList();
    }

    public static List<string> ReadStrList(XElement element) {
        return element.Elements(XNameCache.Item).Select(x => x.Value).ToList();
    }

    public static (string, ulong)[] ReadDictULong(XElement element) {
        try {
            if (element == null) return null;
            var length = int.Parse(element.Attribute(XNameCache.CountAttribute)!.Value);
            var dict = new (string, ulong)[length];
            var items = element.Elements(XNameCache.Item).ToList();
            for (var i = 0; i < items.Count; i++) {
                var item = items[i];
                dict[i] = (item.Attribute(XNameCache.KeyAttribute)!.Value, ulong.Parse(item.Value));
            }

            return dict;
        } catch(Exception e) {
            Console.WriteLine(e + element.Parent!.FirstAttribute!.Value);
            throw;
        }
    }
    
    
    public static TimeSpan ParseTimeSpanString(string s) {
        var parts = s.Split(':');
        if (parts.Length != 4)
            return default;
        return new TimeSpan(int.Parse(parts[0]), int.Parse(parts[1]), int.Parse(parts[2]), int.Parse(parts[3]));
    }
    
    public static T? Let<T>(this XElement? element, Func<XElement, T> transform) => 
        element != null ? transform(element) : default;
}