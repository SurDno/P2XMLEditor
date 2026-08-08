using System;
using System.Collections.Generic;
using System.Linq;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes;

namespace P2XMLEditor.GameData.VirtualMachineElements.Helper;

/// <summary>
/// What a functional component brings with it, learned from the data.
///
/// A standard parameter is not free-standing: its key is "&lt;Component&gt;.&lt;Param&gt;", its
/// OwnerComponent is that component, and the two always agree — all 47760 standard parameters in
/// PathologicSandbox and all 11128 in MarbleNest, no exceptions. So giving an object a component
/// means giving it that component's parameters, and taking the component away means taking them
/// with it; doing one without the other leaves a parameter no component declares or a component
/// whose parameters are missing.
///
/// There is no schema to read that set from — the engine gets it from compiled component classes
/// — so it is read off the corpus instead: what a component looks like everywhere else it is
/// used. That is well defined, because every standard parameter name has exactly one declared
/// type across both corpora (0 conflicts of 136 and 132 names). Defaults are less uniform (79 of
/// 136 names carry one value everywhere), so the most common value is used and the editor lets
/// it be changed afterwards.
///
/// A component's parameter is included when at least half the objects carrying that component
/// declare it. Twelve components in the Sandbox have a handful of objects missing one — Common
/// has 5667 objects and 5642 of them declare ObjectEnabled — and a rule of "everything ever
/// seen" would import those omissions as though they were the shape of the component.
/// </summary>
public sealed class ComponentCatalogue {
	/// <param name="Value">The most common default, written as the xml stores it.</param>
	public readonly record struct ComponentParam(string Name, string Type, string Value);

	private readonly Dictionary<string, List<ComponentParam>> _params = new(StringComparer.Ordinal);
	private readonly Dictionary<string, long> _loadPriority = new(StringComparer.Ordinal);

	/// <summary>Every component name in the data, in alphabetical order.</summary>
	public IReadOnlyList<string> Names { get; private set; } = [];

	public IReadOnlyList<ComponentParam> ParamsOf(string component) =>
		_params.TryGetValue(component, out var list) ? list : [];

	public long LoadPriorityOf(string component) =>
		_loadPriority.TryGetValue(component, out var priority) ? priority : 0;

	public static ComponentCatalogue Build(VirtualMachine vm) {
		var catalogue = new ComponentCatalogue();

		var holders = new Dictionary<string, int>(StringComparer.Ordinal);
		var seen = new Dictionary<string, Dictionary<string, (int Count, Counter Types, Counter Values)>>(
			StringComparer.Ordinal);

		foreach (var component in vm.GetElementsByType<FunctionalComponent>()) {
			if (string.IsNullOrEmpty(component.Name)) continue;
			holders[component.Name] = holders.GetValueOrDefault(component.Name) + 1;
			catalogue._loadPriority.TryAdd(component.Name, component.LoadPriority);
		}

		foreach (var holder in vm.GetElementsByType<ParameterHolder>()) {
			if (holder.StandartParams == null) continue;
			foreach (var (key, parameter) in holder.StandartParams) {
				if (parameter == null) continue;
				var dot = key.IndexOf('.');
				if (dot <= 0) continue;

				var component = key[..dot];
				var name = key[(dot + 1)..];
				if (!seen.TryGetValue(component, out var byName)) seen[component] = byName = new();
				if (!byName.TryGetValue(name, out var entry)) entry = (0, new Counter(), new Counter());

				entry.Types.Add(parameter.Type);
				entry.Values.Add(parameter.SerializedValue);
				byName[name] = (entry.Count + 1, entry.Types, entry.Values);
			}
		}

		foreach (var (component, byName) in seen) {
			var owners = holders.GetValueOrDefault(component);
			var list = new List<ComponentParam>();
			foreach (var (name, entry) in byName.OrderBy(p => p.Key, StringComparer.Ordinal))
				if (owners == 0 || entry.Count * 2 >= owners)
					list.Add(new ComponentParam(name, entry.Types.Most(), entry.Values.Most()));
			catalogue._params[component] = list;
		}

		catalogue.Names = catalogue._params.Keys
			.Union(holders.Keys, StringComparer.Ordinal)
			.OrderBy(n => n, StringComparer.Ordinal)
			.ToList();

		return catalogue;
	}

	/// <summary>
	/// Gives <paramref name="holder"/> a component and the parameters that come with it. A
	/// parameter the object already has under that key is left alone — its value is somebody's
	/// edit, and the point of adding the component is the ones that are missing.
	/// </summary>
	public FunctionalComponent AddTo(ParameterHolder holder, string name, VirtualMachine vm) {
		var component = VmElement.CreateDefault<FunctionalComponent>(vm, holder);
		component.Name = name;
		component.Main = false;
		component.LoadPriority = LoadPriorityOf(name);
		component.Events ??= [];
		(holder.FunctionalComponents ??= []).Add(component);

		holder.StandartParams ??= new Dictionary<string, Parameter>();
		foreach (var declared in ParamsOf(name)) {
			var key = $"{name}.{declared.Name}";
			if (holder.StandartParams.ContainsKey(key)) continue;

			var parameter = VmElement.CreateDefault<Parameter>(vm, holder);
			parameter.Name = declared.Name;
			parameter.Custom = false;
			parameter.Implicit = false;
			parameter.OwnerComponent = component;
			parameter.Value = ParameterValue.Create(vm, declared.Type, declared.Value)
							  ?? ParameterValue.Create(vm, declared.Type, "")!;
			holder.StandartParams[key] = parameter;
		}

		return component;
	}

	/// <summary>
	/// Takes a component off an object along with the parameters it declares. What that costs is
	/// the caller's to ask about first — a standard parameter is routinely read by actions
	/// elsewhere, and nothing here checks.
	/// </summary>
	public static void RemoveFrom(ParameterHolder holder, FunctionalComponent component, VirtualMachine vm) {
		foreach (var key in ParamKeysOf(holder, component).ToList()) {
			var parameter = holder.StandartParams[key];
			holder.StandartParams.Remove(key);
			vm.RemoveElement(parameter);
		}

		// OnDestroy unhooks it from the holder's list and takes its events with it.
		vm.RemoveElement(component);
	}

	/// <summary>The keys of the parameters a component declares on one object.</summary>
	public static IEnumerable<string> ParamKeysOf(ParameterHolder holder, FunctionalComponent component) =>
		holder.StandartParams == null
			? []
			: holder.StandartParams
				.Where(pair => pair.Value != null && ReferenceEquals(pair.Value.OwnerComponent, component))
				.Select(pair => pair.Key);

	private sealed class Counter {
		private readonly Dictionary<string, int> _counts = new(StringComparer.Ordinal);

		public void Add(string? value) {
			var key = value ?? "";
			_counts[key] = _counts.GetValueOrDefault(key) + 1;
		}

		public string Most() =>
			_counts.Count == 0 ? "" : _counts.OrderByDescending(p => p.Value).ThenBy(p => p.Key, StringComparer.Ordinal).First().Key;
	}
}
