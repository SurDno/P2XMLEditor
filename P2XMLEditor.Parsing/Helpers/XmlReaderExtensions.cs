using System.Runtime.CompilerServices;
using System.Xml;

namespace P2XMLEditor.Parsing.Helpers;

public static class XmlReaderExtensions {

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool EndOfContainerReached(this XmlReader r) => r.NodeType != XmlNodeType.Element;
	
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static XmlReader InitializeFullFileReader(string path, int size = 32768) {
		var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, size, FileOptions.SequentialScan);
		var settings = new XmlReaderSettings { IgnoreWhitespace = true, IgnoreComments = true,
			IgnoreProcessingInstructions = true, CheckCharacters = false,
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
		var value = r.ReadElementContentAsString();
		return value;
	}

	private static readonly char[] internal_buf = new char[32];
	
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ulong GetULongValueAndAdvance(this XmlReader r) => GetULongValueAndAdvance(r, internal_buf);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ulong GetULongValueAndAdvance(this XmlReader r, char[] buf) {
		r.Read(); 

		var len = r.ReadValueChunk(buf, 0, buf.Length);

		r.Read(); 
		r.Read(); 

		ulong v = 0;
		for (var i = 0; i < len; i++)
			v = (v << 1) + (v << 3) + (ulong)(buf[i] - '0');
		
		return v;
	}
	
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
	public static void SkipEmptyElement(this XmlReader r) => r.Read();

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void SkipFilledElement(this XmlReader r) {
		r.Read();
		r.Read();
		r.Read();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static string[] GetStringListAndAdvance(this XmlReader r) {
		int length = int.Parse(r.GetAttribute("count")!);
		r.Read();
		var list = new string[length];
		for (var i = 0; i < length; i++) 
			list[i] = r.GetStringValueAndAdvance();
		r.Read();
		return list;
	}
	
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ulong[] GetULongListAndAdvance(this XmlReader r) {
		int length = int.Parse(r.GetAttribute("count")!);
		r.Read();
		var list = new ulong[length];
		for (var i = 0; i < length; i++) 
			list[i] = r.GetULongValueAndAdvance();
		r.Read();
		return list;
	}
	
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static (string, ulong)[] GetULongDictAndAdvance(this XmlReader r) {
		int length = int.Parse(r.GetAttribute("count")!);
		r.Read();
		var dict = new (string, ulong)[length];
		for (var i = 0; i < length; i++)
			dict[i] = (r.GetAttribute("key")!, r.GetULongValueAndAdvance());
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