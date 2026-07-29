using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Xml.Linq;
using ZLinq;
using Enumerable = System.Linq.Enumerable;

namespace P2XMLEditor.Helper;

// TODO: Separate into separate classes handling reading and writing operations.
public static class XmlParsingHelper {
	
	public static XElement CreateBaseElement(ulong id) =>
		new("Item", new XAttribute("id", id));

	public static XElement CreateSelfClosingElement(string name, string? value) =>
		string.IsNullOrEmpty(value) ? new(name) : new(name, value);

	public static XElement CreateBoolElement(string name, bool value) => new(name, value ? "True" : "False");

	public static XElement CreateVector3Element(string name, Vector3 vec) => new(name, $"{vec.X}, {vec.Y}, {vec.Z}");

	public static XElement? CreateListElement(string name, IEnumerable<string>? items) {
		if (items == null || !items.Any()) return null;
		
		var list = items.ToList();
		if (list.Count > 0 && ulong.TryParse(list[0], out _))
			list = list.OrderBy(x => ulong.TryParse(x, out var val) ? val : 0).ToList();

		return new(name, new XAttribute("count", list.Count),
			list.Select(x => new XElement("Item", x)));
	}

	public static XElement? CreateListElementUnsorted(string name, IEnumerable<string>? items) {
		if (items == null || !items.Any()) return null;
		
		var list = items.ToList();
		
		return new(name, new XAttribute("count", list.Count),
			list.Select(x => new XElement("Item", x)));
	}

	public static XElement? CreateDictionaryElement(string name, Dictionary<string, string>? items) {
		if (items == null || items.Count == 0) return null;
		
		return new(name, new XAttribute("count", items.Count),
			items.OrderBy(x => x.Key, CustomComparer.Instance).Select(x => new XElement("Item", new XAttribute("key", x.Key), x.Value)));
	}

	public class CustomComparer : StringComparer {
		public static readonly CustomComparer Instance = new();

		public override int Compare(string? x, string? y) {
			if (ReferenceEquals(x, y)) return 0;
			if (x == null) return -1;
			if (y == null) return 1;

			var len = Math.Min(x.Length, y.Length);
			for (var i = 0; i < len; i++) {
				var cx = x[i];
				var cy = y[i];
				if (cx == cy) continue;

				var wx = GetWeight(cx);
				var wy = GetWeight(cy);
				if (wx != wy) return wx.CompareTo(wy);
				if (char.IsDigit(cx)) return cx.CompareTo(cy);
				break; 
			}

			var cmp = string.Compare(x, y, CultureInfo.InvariantCulture, CompareOptions.IgnoreCase);
			if (cmp != 0) return cmp;
			return string.Compare(x, y, StringComparison.Ordinal);
		}

		private static int GetWeight(char c) {
			if (c == '_') return 1;
			if (char.IsDigit(c)) return 10;
			if (c == '-') return 100;
			if (char.IsLetter(c)) return 1000;
			return 2000;
		}

		public override bool Equals(string? x, string? y) => x == y;
		public override int GetHashCode(string obj) => obj.GetHashCode();
	}
}
