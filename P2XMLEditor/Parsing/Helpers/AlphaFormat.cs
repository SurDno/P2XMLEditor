using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;

namespace P2XMLEditor.Parsing.Helpers;

/// <summary>
/// The alpha corpus names types the way the old engine assemblies did — fully qualified, and with
/// the engine namespace spelled with a Cyrillic 'с' (U+0441): "PLVirtualMa<b>с</b>hine.Common.ITextRef",
/// "Engine.Common.Components.Movable.AreaEnum", "…EngineAPI.VMCommonList". The model works in the
/// short serialization names the later formats use — "ITextRef", "Area", "CommonList" — and left
/// alone none of the qualified names resolve, so thousands of parameters load as an Unknown type.
///
/// This converts between the two: the qualified alpha spelling → the short model spelling on the
/// way in, and back on the way out. Most names are just the qualified name's last segment, but a
/// handful the engine renamed outright (VMCommonList → CommonList, AreaEnum → Area, LockState →
/// GateLockState, …), so the whole set is spelled out rather than derived.
///
/// Reading replaces the qualified strings wholesale — they are unambiguous, a .NET namespace path
/// never occurs inside ordinary text, so even the Russian in GameString (which carries the same
/// Cyrillic letter) is safe. Writing is the ambiguous direction — a short name like "Area" is an
/// ordinary word — so it only expands a name that sits in a type position: after a &lt;Type&gt; tag, the
/// "_type_" marker in a serialized list value, a '%' joining two types, or one of the delimiters of
/// the packed value strings a type is buried in (a DoFunction argument, a graph input parameter, a
/// message cast, a readable predicate) — and always immediately before the '%'/'&amp;' a type is joined
/// on by, which a name never carries. See <see cref="ShortNameInTypePosition"/>.
/// </summary>
public static class AlphaFormat {
	private const string EngineNamespace = "PLVirtualMachine";       // Latin c
	private const string AlphaNamespace = "PLVirtualMaсhine";   // Cyrillic с (U+0441)

	/// <summary>Qualified alpha type name (Latin, post-Cyrillic-fix) → short model name.</summary>
	private static readonly (string Qualified, string Short)[] TypeNames = {
		("PLVirtualMachine.Common.EngineAPI.VMCommonList", "CommonList"),
		("PLVirtualMachine.Common.EngineAPI.VMECS.EVMGameLocalizationName", "GameLocalizationName"),
		("PLVirtualMachine.Common.EngineAPI.GameTime", "GameTime"),
		("PLVirtualMachine.Common.IObjRef", "IObjRef"),
		("PLVirtualMachine.Common.ITextRef", "ITextRef"),
		("PLVirtualMachine.Common.IStateRef", "IStateRef"),
		("PLVirtualMachine.Common.ISampleRef", "ISampleRef"),
		("PLVirtualMachine.Common.IBlueprintRef", "IBlueprintRef"),
		("PLVirtualMachine.Common.ObjectCombinationDataStruct", "ObjectCombinationDataStruct"),
		("Engine.Common.Commons.CombatActionEnum", "CombatAction"),
		("Engine.Common.Commons.BoundHealthStateEnum", "BoundHealthStateEnum"),
		("Engine.Common.Commons.CombatStyleEnum", "CombatStyleEnum"),
		("Engine.Common.Commons.DiseasedStateEnum", "DiseasedStateEnum"),
		("Engine.Common.Commons.FractionEnum", "FractionEnum"),
		("Engine.Common.Commons.LiquidTypeEnum", "LiquidTypeEnum"),
		("Engine.Common.Commons.StammKind", "StammKind"),
		("Engine.Common.Components.Crowds.OutdoorCrowdLayoutEnum", "OutdoorCrowdLayout"),
		("Engine.Common.Components.Gate.LockState", "GateLockState"),
		("Engine.Common.Components.Interactable.InteractType", "InteractType"),
		("Engine.Common.Components.Movable.AreaEnum", "Area"),
		("Engine.Common.Components.Regions.BuildingEnum", "BuildingEnum"),
		("Engine.Common.Components.Storable.ContainerOpenStateEnum", "ContainerOpenState"),
	};

	// A short name expands back only where it is genuinely a type — never where it could be an
	// ordinary word — which means sitting immediately after one of the markers a type follows and
	// immediately before the '%'/'&' (or LIST/ELEM) a type is joined to the next thing by. The
	// markers are every context a type appears in across the corpus: a &lt;Type&gt; element, the "_type_"
	// tag inside a serialized list value, a '%' joining two types, and the delimiters of the packed
	// value strings — a DoFunction argument's "PART&amp;<type>&amp;PARAM", a graph input parameter's
	// "…P&amp;PM<type>", a message cast's "CAST&amp;INFO<type>", a readable predicate's "(?,<type>". A
	// name never carries the trailing '%'/'&', so &lt;Name&gt;Area&lt;/Name&gt; is left alone. Longest names
	// first so none is matched inside a longer one.
	private static readonly Regex ShortNameInTypePosition = new(
		@"(?<=<Type>|_type_|%|PART&amp;|P&amp;PM|CAST&amp;INFO|\(\?,)(" +
		string.Join("|", TypeNames.Select(t => Regex.Escape(t.Short)).OrderByDescending(s => s.Length)) +
		")(?=[%&<]|LIST|ELEM|$)",
		RegexOptions.Compiled);

	private static readonly Dictionary<string, string> ShortToQualified =
		TypeNames.GroupBy(t => t.Short).ToDictionary(g => g.Key, g => g.First().Qualified);

	/// <summary>Alpha spelling → the short, Latin spelling the model uses. A no-op on any other format.</summary>
	public static string Normalize(string text) {
		text = text.Replace(AlphaNamespace, EngineNamespace);
		foreach (var (qualified, shortName) in TypeNames)
			text = text.Replace(qualified, shortName);
		return text;
	}

	/// <summary>The model's short spelling → the alpha spelling, for writing the corpus back out.</summary>
	public static string Denormalize(string text) {
		text = ShortNameInTypePosition.Replace(text, m => ShortToQualified[m.Value]);
		return text.Replace(EngineNamespace, AlphaNamespace);
	}

	/// <summary>
	/// A reader over an alpha file with its type names already in the model's spelling. The whole
	/// file is read and normalised before parsing, so every occurrence — a parameter's Type, a type
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
