using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Xml;

namespace P2XMLEditor.Helper;

public static class XmlReaderExtensions {

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool EndOfContainerReached(this XmlReader r) => r.NodeType != XmlNodeType.Element;
	
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static XmlReader InitializeFullFileReader(string path, int size = 32768) {
		var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, size, FileOptions.SequentialScan);
		var settings = new XmlReaderSettings { IgnoreWhitespace = true, IgnoreComments = true, CheckCharacters = false,
			DtdProcessing = DtdProcessing.Ignore, ConformanceLevel = ConformanceLevel.Fragment };
		return XmlReader.Create(fs, settings);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void SkipDeclarationAndRoot(this XmlReader r) {
		r.Read();
		r.Read();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ulong GetIdAndEnter(this XmlReader r) {
		var id = ulong.Parse(r.GetAttribute("id")!);
		r.Read();
		return id;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static string GetStringValueAndAdvance(this XmlReader r) {
		r.Read();
		var value = r.Value;
		r.Read();
		r.Read();
		return value;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ulong GetULongValueAndAdvance(this XmlReader r) => ulong.Parse(r.GetStringValueAndAdvance());
	
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static long GetLongValueAndAdvance(this XmlReader r) => long.Parse(r.GetStringValueAndAdvance());

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int GetIntValueAndAdvance(this XmlReader r) => int.Parse(r.GetStringValueAndAdvance());

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool GetBoolValueAndAdvance(this XmlReader r) => r.GetStringValueAndAdvance()[0] == 'T';

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float GetFloatValueAndAdvance(this XmlReader r) => float.Parse(r.GetStringValueAndAdvance());
	
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static string GetOptionalStringValueAndAdvance(this XmlReader xr) {
		if (!xr.IsEmptyElement) 
			return xr.GetStringValueAndAdvance();
		xr.Read();
		return string.Empty;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void SkipEmptyELement(this XmlReader r) => r.Read();

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void SkipFilledElement(this XmlReader r) {
		r.Read();
		r.Read();
		r.Read();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static List<string> GetStringListAndAdvance(this XmlReader r) {
		r.Read();
		var list = new List<string>();
		while (!r.EndOfContainerReached()) 
			list.Add(r.GetStringValueAndAdvance());
		r.Read();
		return list;
	}
	
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static List<ulong> GetULongListAndAdvance(this XmlReader r) {
		r.Read();
		var list = new List<ulong>();
		while (!r.EndOfContainerReached()) 
			list.Add(ulong.Parse(r.GetStringValueAndAdvance()));
		r.Read();
		return list;
	}
	
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Dictionary<string, ulong> GetULongDictAndAdvance(this XmlReader r) {
		r.Read();
		var dict = new Dictionary<string, ulong>();
		while (!r.EndOfContainerReached()) {
			var key = r.GetAttribute("key")!;
			dict[key] = r.GetULongValueAndAdvance();
		}

		r.Read();
		return dict;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Dictionary<string, string> GetStringDictAndAdvance(this XmlReader r) {
		r.Read();
		var dict = new Dictionary<string, string>();
		while (!r.EndOfContainerReached())
			dict[r.GetAttribute("key")!] = r.GetStringValueAndAdvance();
		r.Read();
		return dict;
	}
}