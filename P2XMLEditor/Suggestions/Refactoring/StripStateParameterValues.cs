using System.Diagnostics.CodeAnalysis;
using System.Linq;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes;
using P2XMLEditor.GameData.VirtualMachineElements.Placeholders;
using P2XMLEditor.Logging;

namespace P2XMLEditor.Suggestions.Refactoring;

/// <summary>
/// Empties the stored value of every implicit "_state" parameter, which the engine never reads.
///
/// <c>DynamicParameter.GetValue</c> ends with: if the parameter is implicit, typed IStateRef and
/// named "*_state", it builds a fresh VMStateRef from <c>parentFSM.CurrentState</c> and returns
/// that — the loaded value is passed over entirely. Nothing writes one either: not one of the
/// 2376 across the two corpora is named by any action or expression. The other half of the
/// machinery, <c>VMLogicObject.GetStateParam</c>, is not called from anywhere in the engine
/// source at all.
///
/// So the value is a reference kept alive for nothing, and it does not even stay correct: every
/// one of them points at a State, and 8 of MarbleNest's 303 already point at a State that no
/// longer exists. Emptying it is a shape the data already has — 538 IStateRef parameters in
/// PathologicSandbox and 137 in MarbleNest ship with no value.
///
/// The parameter itself stays. Removing it is a different question with a different answer; see
/// <see cref="RemoveStateParametersFromGraphlessObjects"/>.
/// </summary>
[Refactoring("Refactor/Parameters/Strip implicit state parameter values"),
 SuppressMessage("ReSharper", "UnusedType.Global")]
public class StripStateParameterValues(VirtualMachine vm) : Suggestion(vm) {
	public override void Execute() {
		var stripped = 0;
		var dangling = 0;

		foreach (var parameter in Vm.GetElementsByType<Parameter>().ToList()) {
			if (!IsImplicitState(parameter)) continue;
			if (string.IsNullOrEmpty(parameter.SerializedValue)) continue;

			// Worth counting separately: a value that no longer resolves is proof the field was
			// never maintained, which is the argument for dropping it.
			if (parameter.Value is RefValue<State> { TypedValue: null or StatePlaceholder })
				dangling++;

			if (ParameterValue.Create(Vm, parameter.Type, "") is not { } empty) continue;
			parameter.Value = empty;
			stripped++;
		}

		Logger.Log(LogLevel.Info,
			$"Stripped {stripped} implicit state parameter value(s); {dangling} of them no longer resolved.");
	}

	/// <summary>
	/// The engine's own test, verbatim: implicit, IStateRef, name ending in "_state". All three
	/// matter — a custom parameter that merely ends in "_state" is ordinary storage.
	/// </summary>
	internal static bool IsImplicitState(Parameter? parameter) =>
		parameter is { Implicit: true } &&
		(parameter.Name ?? "").EndsWith("_state", System.StringComparison.Ordinal) &&
		(parameter.Type ?? "").StartsWith("IStateRef", System.StringComparison.Ordinal);
}
