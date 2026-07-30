using System;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.Helper;
using P2XMLEditor.Helper;
using P2XMLEditor.Logging;
using P2XMLEditor.GameData.VirtualMachineElements.Placeholders;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes;

public enum EventOwnerKind {
	/// <summary>Direct reference to a ParameterHolder: Quest, GameRoot, Character, Blueprint, Other.</summary>
	Holder,
	/// <summary>World placement, identified by its path of template guids.</summary>
	Hierarchy,
	/// <summary>Indirection through an IObjRef parameter.</summary>
	ParameterRef
}

/// <summary>
/// GraphLink.EventObject — the object whose event fires the link.
///
/// Deliberately NOT TargetObject. This slot resolves to a subscription target, not to a value:
///
///   GetEventOwnerDynamicContext(activeFSM, eventOwnerVariable, graphOwner)
///     if (eventOwnerVariable.IsNull) return activeFSM;              // absent -> the FSM itself
///     eventOwnerVariable.Bind(graphOwner, new VMType(typeof(IObjRef)));
///     if (eventOwnerVariable.VariableContext != null)
///         return GetDynamicContext(eventOwnerVariable.VariableContext, activeFSM);
///     return null;                                                   // -> link never subscribes
///
/// It reads VariableContext, never the value, and Bind runs against the graph's owning
/// blueprint at subscribe time — before any event has fired and outside any action's scope.
/// So the dynamic forms TargetObject supports cannot work here:
///
///   loop variable   scoped to an executing ActionLine, which does not exist at subscribe time
///   message         payload of an event that has not fired yet
///   input param     resolved against the live FSM, but Bind is passed the static blueprint
///   engine GUID     GetStaticContextByData only accepts GT_BASE and GT_HIERARCHY
///
/// All four parse, none produce a VariableContext, and the link silently fails to subscribe
/// with "Event subscribing error: event owner by variable {0} not found". None occur in the
/// data: 42 408 values are empty (35 392), holder (5 552), hierarchy (589) or parameter (168)
/// — the four forms with a static identity resolvable from the graph owner.
///
/// Absence is meaningful and common: a null EventObject means "this FSM's own event".
/// </summary>
public readonly struct EventOwner {
	public EventOwnerKind Kind { get; init; }

	public ParameterHolder? Holder { get; init; }
	public HierarchyGuid? Hierarchy { get; init; }

	/// <summary>Always IObjRef-typed in the data — Bind is called with needType = IObjRef.</summary>
	public Parameter? ParameterRef { get; init; }

	/// <summary>Written "X%X" rather than "X". 707 values.</summary>
	public bool IsSelf { get; init; }

	/// <summary>Value carried a leading '%'. Inert, kept for round-trip.</summary>
	public bool HasLeadingPercent { get; init; }

	/// <summary>Null when the link fires on the owning FSM's own event — 35 392 of 42 408.</summary>
	public static EventOwner? Read(string data, VirtualMachine vm) {
		if (string.IsNullOrEmpty(data) || data == "%") return null;

		var leading = data.StartsWith('%');
		var body = leading ? data[1..] : data;

		var isSelf = false;
		var sep = body.IndexOf('%');
		if (sep != -1) {
			var left = body[..sep];
			var right = body[(sep + 1)..];
			if (left == right) { isSelf = true; body = left; }
			else {
				Logger.Log(LogLevel.Warning, $"EventObject '{data}' has a distinct context part; not seen in vanilla data.");
				body = right;
			}
		}

		if (HierarchyGuid.TryParse(body, vm, out var hierarchy))
			return new() { Kind = EventOwnerKind.Hierarchy, Hierarchy = hierarchy,
						   IsSelf = isSelf, HasLeadingPercent = leading };

		if (ulong.TryParse(body, out var id)) {
			var element = vm.GetNullableElement(id) ?? vm.Register(new ParameterPlaceholder(id));

			switch (element) {
				case Parameter p:
					return new() { Kind = EventOwnerKind.ParameterRef, ParameterRef = p,
								   IsSelf = isSelf, HasLeadingPercent = leading };
				case ParameterHolder h:
					return new() { Kind = EventOwnerKind.Holder, Holder = h,
								   IsSelf = isSelf, HasLeadingPercent = leading };
			}
		}

		// Everything else is one of the dynamic forms above: it would parse, then fail to
		// subscribe at runtime. Refuse it here rather than store something unusable.
		Logger.Log(LogLevel.Error,
			$"EventObject '{data}' is not a static reference; an event link cannot subscribe through it.");
		return null;
	}

	public string Write() {
		var value = Kind switch {
			EventOwnerKind.Holder       => Holder!.Id.ToString(),
			EventOwnerKind.Hierarchy    => Hierarchy!.Write(),
			EventOwnerKind.ParameterRef => ParameterRef!.Id.ToString(),
			_ => throw new InvalidOperationException($"Cannot write an uninitialised {nameof(EventOwner)}.")
		};

		if (IsSelf) value = $"{value}%{value}";
		return HasLeadingPercent ? "%" + value : value;
	}

	public override string ToString() => Write();
}
