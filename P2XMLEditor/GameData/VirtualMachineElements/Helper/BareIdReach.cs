using System.Collections.Generic;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.Helper;

namespace P2XMLEditor.GameData.VirtualMachineElements.Helper;

/// <summary>
/// Whether naming an object by bare id actually reaches it when the action runs.
///
/// Parsing is not the test — a bare id resolves for any object, to the static template. Getting
/// from there to the live entity goes through <c>VMVariableService.GetDynamicContext</c>, and
/// <c>GetDynamicContextByBlueprintContext</c> forks on the object's own Static flag:
///
///   if (blueprintContext.Static)
///     return GetDynamicContextByStaticObject(blueprintContext);
///   if (ownerContext != null &amp;&amp; ownerContext.IsStaticDerived(blueprintContext.Blueprint))
///     return ownerContext;
///   Logger.AddError("Cannot get dynamic context by unbinded template context ...");
///
/// So there are exactly two ways a bare id lands, and one way it fails:
///
/// * Static — <c>GetDynamicContextByStaticObject</c> looks the entity up globally. It takes the
///   hierarchy branch only for an IWorldHierarchyObject, and a bare id resolves to the template,
///   never to that, so it always lands on <c>GetDynamicObjectEntityByStaticGuid</c>. A placed
///   object was filed under its hierarchy guid by <c>VirtualMachine.RegisterDynamicObject</c>
///   and is not in that table: "Dynamic entity by static object ... not found".
/// * Not static — resolves to the object currently running the action, and only when that
///   object is or derives from the named one. Anything else is the "unbinded template context"
///   error above.
///
/// Both corpora agree without exception. Of the bare-id action targets naming a world object,
/// all 3276 non-static ones are self-targets, all 1844 static ones are unplaced, and no action
/// anywhere names a static placed object by id. The one apparent exception — a Character
/// targeting the blueprint StrangeBride — is the IsStaticDerived case, not a self-target.
/// </summary>
public static class BareIdReach {
	/// <summary>Why a bare id will not reach this object at runtime, or null when it will.</summary>
	public static string? Problem(ParameterHolder? target, ParameterHolder? owner, VirtualMachine vm) {
		if (target == null) return null;

		// VMGameRoot.Static is an override returning true, so the root needs no Static field and
		// the 4611 actions targeting it from anywhere are all fine.
		if (target is GameRoot) return null;

		if (target.Static == true) {
			return WorldHierarchy.For(vm).IsPlaced(target.Id)
				? "placed in the world — its live entity is filed under a hierarchy guid, so an id "
				  + "cannot reach it; name it under Scene hierarchy instead"
				: null;
		}

		if (owner != null && DerivesFrom(owner, target, vm)) return null;

		return owner == null
			? "not a static object — an id reaches it only from logic running on that object itself"
			: $"not a static object — an id reaches it only from logic running on that object itself, "
			  + $"and this action runs on {owner.Name}";
	}

	/// <summary>
	/// The editor's read of <c>IsStaticDerived</c>: the object itself, or anything whose
	/// InheritanceInfo chain reaches it. Inheritance, not Parent — Parent is scene nesting and
	/// every object's chain ends at the GameRoot, which would make this always true.
	/// </summary>
	private static bool DerivesFrom(ParameterHolder candidate, ParameterHolder prototype, VirtualMachine vm) {
		var visited = new HashSet<ParameterHolder>();
		var pending = new Stack<ParameterHolder>();
		pending.Push(candidate);

		while (pending.Count > 0) {
			var holder = pending.Pop();
			if (!visited.Add(holder)) continue;
			if (ReferenceEquals(holder, prototype) || holder.Id == prototype.Id) return true;

			foreach (var inherited in holder.InheritanceInfo ?? [])
				if (ulong.TryParse(inherited, out var id) &&
					vm.GetNullableElement<ParameterHolder>(id) is { } parent)
					pending.Push(parent);
		}
		return false;
	}
}
