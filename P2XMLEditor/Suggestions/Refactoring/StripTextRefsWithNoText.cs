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
/// Clears an ITextRef parameter that points at a game string with no text in any language.
///
/// This is not tidying — it changes what the game draws. <c>LocalizationService.GetVmTextImpl</c>
/// reads:
///
/// <code>
/// if (id == 0) return "";
/// ... current language, then the default language ...
/// return found ? text : id.ToString();
/// </code>
///
/// So a parameter with no value renders as the empty string, and a parameter pointing at a string
/// that was never written renders as <em>the raw id</em> — "6192355001262556" on screen wherever
/// that field is shown. Clearing the value turns the second case into the first.
///
/// The population is not marginal. Of the 288 Storable.SpecialDescription values set in
/// PathologicSandbox, 276 point at a string with no text in any language and only 12 carry real
/// text; in MarbleNest it is 115 against 1. Across all standard ITextRef parameters it is 876 and
/// 277. Meanwhile 826 storables in the Sandbox already ship with SpecialDescription empty, so the
/// end state is one the engine handles everywhere already —
/// <c>EngineAPIManager.CreateEngineTextInstance</c> null-checks the ref and its text and hands
/// back LocalizedText.Empty.
///
/// Only the value goes. The parameter stays, because it is a standard parameter its component
/// declares. The game string it pointed at is left alone too; if nothing else names it,
/// <c>RemoveUnreferencedGameStrings</c> is the pass that collects it.
///
/// Nothing here can lose writing: a reference is only dropped when the string behind it is empty
/// in every loaded language. Emptying a field that does carry text, on the grounds that the game
/// never displays that field, is a separate and riskier claim — see
/// <see cref="StripUnreadTextProperties"/>.
/// </summary>
[Refactoring("Refactor/Parameters/Strip text references that have no text"),
 SuppressMessage("ReSharper", "UnusedType.Global")]
public class StripTextRefsWithNoText(VirtualMachine vm) : Suggestion(vm) {
	public override void Execute() {
		var stripped = new Dictionary<string, int>();
		var keptWithText = 0;
		var total = 0;

		foreach (var holder in Vm.GetElementsByType<ParameterHolder>().ToList()) {
			foreach (var (key, parameter) in Named(holder)) {
				if (parameter == null || parameter.Type != "ITextRef") continue;
				if (string.IsNullOrEmpty(parameter.SerializedValue)) continue;

				total++;
				if (HasText(parameter)) {
					keptWithText++;
					continue;
				}

				if (ParameterValue.Create(Vm, parameter.Type, "") is not { } empty) continue;
				parameter.Value = empty;
				stripped[key] = stripped.GetValueOrDefault(key) + 1;
			}
		}

		var count = stripped.Values.Sum();
		Logger.Log(LogLevel.Info,
			$"Stripped {count} of {total} text reference(s) that resolved to no text in any language; "
			+ $"kept {keptWithText} that carry text.");
		foreach (var (key, number) in stripped.OrderByDescending(p => p.Value))
			Logger.Log(LogLevel.Info, $"   {key}: {number}");
	}

	/// <summary>
	/// True when the string has something to say in at least one loaded language. Every language
	/// counts, not just the one being previewed — a text that exists only in Russian is still a
	/// text, and clearing it would be deleting content rather than a dead reference.
	/// </summary>
	private bool HasText(Parameter parameter) {
		if (parameter.Value is not RefValue<GameString> { TypedValue: { } text }) return false;
		return Vm.Languages.Any(language => !string.IsNullOrWhiteSpace(text.GetText(language)));
	}

	private static IEnumerable<KeyValuePair<string, Parameter>> Named(ParameterHolder holder) {
		var standart = holder.StandartParams ?? new Dictionary<string, Parameter>();
		var custom = holder.CustomParams ?? new Dictionary<string, Parameter>();
		return standart.Concat(custom);
	}
}
