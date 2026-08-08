using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes;
using P2XMLEditor.Logging;

namespace P2XMLEditor.Suggestions.Refactoring;

/// <summary>
/// Clears text properties the game never reads, including the ones that do carry text.
///
/// This is the other half of <see cref="StripTextRefsWithNoText"/> and the more dangerous one.
/// That pass drops references to strings nobody wrote, which cannot lose anything; this one drops
/// real text, on the grounds that the field it sits in is never displayed. So it works from an
/// explicit list rather than a rule, and every entry on that list has to be shown to be write-only
/// in the game assembly before it goes on.
///
/// Storable.SpecialDescription is the one that qualifies today. StorableComponent declares it and
/// the save proxy reads and writes it, and that is the whole of it: searching Assembly-CSharp for
/// uses of the property outside its own declaration finds none, against 38 for Title, 28 for
/// Description and 9 for Tooltip. The VM can still assign it — GlobalStorageManager has
/// SetAllStorablesSpecialDescription and SetStorablesTemplateSpecialDescription, called once each
/// in PathologicSandbox — but assigning a field nothing reads changes nothing either.
///
/// It empties the parameter's value, not the parameter: the parameter belongs to the Storable
/// component that declares it. The game strings left behind stay too, until
/// RemoveUnreferencedGameStrings is run — which will delete their text for good, so the text of
/// everything stripped here is written to the log first. In the corpora that is 12 strings in
/// PathologicSandbox and 1 in MarbleNest; the other 276 and 115 values are references to strings
/// that were never written in any language.
/// </summary>
[Refactoring("Refactor/Parameters/Strip text properties the game never reads"),
 SuppressMessage("ReSharper", "UnusedType.Global")]
public class StripUnreadTextProperties(VirtualMachine vm) : Suggestion(vm) {
	/// <summary>
	/// Parameter key to the reason it is safe to empty. A key earns its place here by being
	/// write-only in the shipped game code — not by looking unused in the data, which is a
	/// different claim about a different thing.
	/// </summary>
	private static readonly Dictionary<string, string> Unread = new(StringComparer.Ordinal) {
		["Storable.SpecialDescription"] =
			"StorableComponent.SpecialDescription is declared and serialised but read nowhere in the game"
	};

	public override void Execute() {
		var stripped = new Dictionary<string, int>(StringComparer.Ordinal);
		var carried = 0;

		foreach (var holder in Vm.GetElementsByType<ParameterHolder>().ToList()) {
			if (holder.StandartParams == null) continue;

			foreach (var (key, parameter) in holder.StandartParams.ToList()) {
				if (parameter == null || !Unread.ContainsKey(key)) continue;
				if (parameter.Type != "ITextRef" || string.IsNullOrEmpty(parameter.SerializedValue)) continue;

				// Said out loud before it goes: this is the one pass here that can delete writing.
				if (TextOf(parameter) is { Length: > 0 } text) {
					carried++;
					Logger.Log(LogLevel.Info, $"{holder.Name}.{key} carried text, dropping: \"{Shorten(text)}\"");
				}

				if (ParameterValue.Create(Vm, parameter.Type, "") is not { } empty) continue;
				parameter.Value = empty;
				stripped[key] = stripped.GetValueOrDefault(key) + 1;
			}
		}

		Logger.Log(LogLevel.Info,
			$"Stripped {stripped.Values.Sum()} value(s) from text properties the game never reads, "
			+ $"{carried} of which carried text.");
		foreach (var (key, count) in stripped.OrderByDescending(p => p.Value))
			Logger.Log(LogLevel.Info, $"   {key}: {count}   ({Unread[key]})");
	}

	/// <summary>The text in any loaded language, so a Russian-only string is reported as text too.</summary>
	private string? TextOf(Parameter parameter) {
		if (parameter.Value is not RefValue<GameString> { TypedValue: { } gameString }) return null;
		return Vm.Languages
			.Select(gameString.GetText)
			.FirstOrDefault(text => !string.IsNullOrWhiteSpace(text));
	}

	private static string Shorten(string text) {
		var single = text.Replace('\n', ' ').Replace('\r', ' ');
		return single.Length <= 120 ? single : single[..119] + "…";
	}
}
