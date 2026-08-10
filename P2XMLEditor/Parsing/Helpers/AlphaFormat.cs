using System.IO;
using System.Xml;

namespace P2XMLEditor.Parsing.Helpers;

/// <summary>
/// The one thing about the alpha corpus that is not the demo shape with the id moved: it writes the
/// engine namespace with a Cyrillic 'с' (U+0441) where the standard uses a Latin 'c' —
/// "PLVirtualMa<b>с</b>hine" — in every type string it stores. Left as-is, none of those types
/// resolve (<c>GetVmTypeInfo</c> returns Unknown for thousands of parameters), so type strings are
/// normalised to the Latin spelling the model works in on the way in and put back on the way out.
///
/// The swap is of the whole "PLVirtualMachine" substring, never the bare letter, so the Russian
/// text that fills GameString — which carries the same Cyrillic letter in ordinary words — is left
/// untouched: that exact Latin-surrounded sequence never occurs inside it.
/// </summary>
public static class AlphaFormat {
	public const string EngineNamespace = "PLVirtualMachine";       // Latin c
	public const string AlphaNamespace = "PLVirtualMaсhine";   // Cyrillic с (U+0441)

	/// <summary>Alpha spelling → the Latin spelling the model uses. A no-op on any other format.</summary>
	public static string Normalize(string text) => text.Replace(AlphaNamespace, EngineNamespace);

	/// <summary>The model's Latin spelling → the alpha spelling, for writing the corpus back out.</summary>
	public static string Denormalize(string text) => text.Replace(EngineNamespace, AlphaNamespace);

	/// <summary>
	/// A reader over an alpha file with its type strings already normalised. The whole file is read
	/// and the substring replaced before parsing, so every occurrence — a parameter's Type, a type
	/// embedded in a serialized Value, a message's declared type — is handled in one place rather
	/// than field by field.
	/// </summary>
	public static XmlReader OpenReader(string filePath) {
		var text = Normalize(File.ReadAllText(filePath));
		var settings = new XmlReaderSettings {
			IgnoreWhitespace = true, IgnoreComments = true, IgnoreProcessingInstructions = true,
			CheckCharacters = false, DtdProcessing = DtdProcessing.Ignore,
			ConformanceLevel = ConformanceLevel.Fragment
		};
		return XmlReader.Create(new StringReader(text), settings);
	}
}
