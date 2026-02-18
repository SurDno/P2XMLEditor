using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Xml.Linq;
using ZLinq;

namespace P2XMLEditor.Helper;

// TODO: Separate into separate classes handling reading and writing operations.
public static class XmlParsingHelper {
    
    //
    public static XElement CreateBaseElement(ulong id) =>
        new("Item", new XAttribute("id", id));

    public static XElement CreateSelfClosingElement(string name, string? value) =>
        string.IsNullOrEmpty(value) ? new(name) : new(name, value);

    public static XElement CreateBoolElement(string name, bool value) => new(name, value ? "True" : "False");

    public static XElement CreateVector3Element(string name, Vector3 vec) => new(name, $"{vec.X}, {vec.Y}, {vec.Z}");

    public static XElement? CreateListElement(string name, IEnumerable<string>? items) {
        var itemsValue = items;
        if (itemsValue.Any() != true) return null;
        return new(name, new XAttribute("count", itemsValue.Count()),
            itemsValue.Select(x => new XElement("Item", x)));
    }



    public static XElement? CreateListElement<TEnumerator>(string name, ValueEnumerable<TEnumerator, string> items)
                                                              where TEnumerator : struct, IValueEnumerator<string> {
        if (!items.Any()) return null;
        return new XElement(name, new XAttribute("count", items.Count()), 
            items.Select(x => new XElement("Item", x)));
    }


    public static XElement? CreateDictionaryElement(string name, Dictionary<string, string>? items) {
        var itemsValue = items;
        if (itemsValue.Any() != true) return null;
        return new(name, new XAttribute("count", items.Count),
            itemsValue.Select(x => new XElement("Item", new XAttribute("key", x.Key), x.Value)));
    }
    

    public static XElement? CreateDictionaryElement<TEnumerator>(string name, ValueEnumerable<TEnumerator,
                                                                     KeyValuePair<string, string>> items) 
                                            where TEnumerator : struct, IValueEnumerator<KeyValuePair<string, string>> {
        if (!items.Any()) return null;

        return new XElement(name, new XAttribute("count", items.Count()),
            items.Select(x => new XElement("Item", new XAttribute("key", x.Key), x.Value))
        );
    }
}