using System;
using System.Collections.Generic;
using System.Linq;
using P2XMLEditor.Data;
using P2XMLEditor.GameData;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes;
using P2XMLEditor.GameData.VirtualMachineElements.Placeholders;
using P2XMLEditor.Helper;
using Action = P2XMLEditor.GameData.VirtualMachineElements.Action;

namespace P2XMLEditor.Core;

public class VirtualMachine {
	public readonly Dictionary<ulong, VmElement> ElementsById = new();
	public readonly TemplateManager TemplateManagerInst;
	public readonly GameType Type;
	public VmVersionSettings? VmMetadata { get; set; }

	public readonly Dictionary<Type, List<VmElement>> ElementsByType = new() {
		[typeof(VmElement)] = [],
		[typeof(ParameterHolder)] = [],
		[typeof(GameObject)] = [],
		[typeof(Action)] = [],
		[typeof(ActionLine)] = [],
		[typeof(Blueprint)] = [],
		[typeof(Branch)] = [],
		[typeof(Character)] = [],
		[typeof(Condition)] = [],
		[typeof(CustomType)] = [],
		[typeof(EntryPoint)] = [],
		[typeof(Event)] = [],
		[typeof(Expression)] = [],
		[typeof(FunctionalComponent)] = [],
		[typeof(GameMode)] = [],
		[typeof(GameRoot)] = [],
		[typeof(GameString)] = [],
		[typeof(Geom)] = [],
		[typeof(Graph)] = [],
		[typeof(GraphLink)] = [],
		[typeof(Item)] = [],
		[typeof(MindMap)] = [],
		[typeof(MindMapLink)] = [],
		[typeof(MindMapNode)] = [],
		[typeof(MindMapNodeContent)] = [],
		[typeof(Other)] = [],
		[typeof(Parameter)] = [],
		[typeof(PartCondition)] = [],
		[typeof(Quest)] = [],
		[typeof(Reply)] = [],
		[typeof(Sample)] = [],
		[typeof(Scene)] = [],
		[typeof(Speech)] = [],
		[typeof(State)] = [],
		[typeof(Talking)] = []
	};
	public HashSet<string> Languages { get; } = [];

	// fast access cache
	private Dictionary<string, Message>? _messageIndex;
	private Dictionary<string, GameObject>? _byEngineTemplateId;
	// Reverse reference map (target id -> the elements that name it), built once on first use and
	// dropped whenever an element is added or removed. Lets the reference browser answer "where is
	// this used" from a dictionary lookup instead of a whole-VM scan per element.
	private Dictionary<ulong, List<VmElement>>? _referrers;

	public IReadOnlyList<VmElement> GetReferrers(ulong id) {
		_referrers ??= DomainReferenceFinder.BuildReferenceIndex(this);
		return _referrers.GetValueOrDefault(id) ?? (IReadOnlyList<VmElement>)Array.Empty<VmElement>();
	}
	
	private Dictionary<string, VmTypeInfo>? _standartParamTypes;
	public bool TryResolveStandartParamType(string name, out VmTypeInfo type) {
		_standartParamTypes ??= BuildStandartParamTypes();
		return _standartParamTypes.TryGetValue(name, out type!);
	}

	/// <summary>
	/// Every standard parameter name in the data with its declared type — the same index
	/// <see cref="TryResolveStandartParamType"/> answers from, exposed so an editor can offer
	/// the names that fit a slot instead of asking about one name at a time.
	/// </summary>
	public IReadOnlyDictionary<string, VmTypeInfo> StandartParamTypes {
		get {
			_standartParamTypes ??= BuildStandartParamTypes();
			return _standartParamTypes;
		}
	}

	public void InvalidateStandartParamTypes() => _standartParamTypes = null;

	private Dictionary<string, VmTypeInfo> BuildStandartParamTypes() {
		var map = new Dictionary<string, VmTypeInfo>(StringComparer.Ordinal);
		foreach (var holder in GetElementsByType<ParameterHolder>()) {
			if (holder.StandartParams == null) continue;
			foreach (var (key, parameter) in holder.StandartParams)
				if (parameter != null) map.TryAdd(key, VmTypeHelper.GetVmTypeInfo(parameter.Type, this));
		}
		return map;
	}

	public GameObject? GetByEngineTemplateId(string guid) {
		_byEngineTemplateId ??= GetElementsByType<GameObject>()
			.Where(o => !string.IsNullOrEmpty(o.EngineTemplateId) && o.EngineTemplateId != new string('0', 32))
			.GroupBy(o => o.EngineTemplateId!)
			.ToDictionary(g => g.Key, g => g.First());
		return _byEngineTemplateId.GetValueOrDefault(guid);
	}
	
	// The element currently being filled, so a reference deep in the parse (an input parameter
	// walking up to its graph) can find its context without threading the scope through every call.
	// Instance state, not a static, so two VMs can load without trampling each other.
	public VmElement? FillScope { get; private set; }
	public FillScopeHandle EnterFillScope(VmElement? scope) => new(this, scope);
	public readonly struct FillScopeHandle : IDisposable {
		private readonly VirtualMachine _vm;
		private readonly VmElement? _previous;
		internal FillScopeHandle(VirtualMachine vm, VmElement? scope) { _vm = vm; _previous = vm.FillScope; vm.FillScope = scope; }
		public void Dispose() => _vm.FillScope = _previous;
	}
	
	
	public VirtualMachine(int capacity, TemplateManager templateManagerInst, GameType type) {
		ElementsById = new Dictionary<ulong, VmElement>(capacity);
		TemplateManagerInst = templateManagerInst;
		Type = type;
	}
	
	public T AddElement<T>(T element, Type elementType) where T : VmElement {
		_referrers = null; // stale once the element set changes; rebuilt on next GetReferrers
		ElementsById[element.Id] = element;
		if (element is IPlaceholder) return element;
		while (elementType != typeof(VmElement) && elementType != typeof(object)) {
			if (!ElementsByType.TryGetValue(elementType, out var list))
				ElementsByType[elementType] = list = [];
			list.Add(element);
			elementType = elementType.BaseType!;
		}
		return element;
	}
	
	public int GetDataCapacity() => ElementsById.Count(e => e.Value is not (ParameterPlaceholder or ScenePlaceholder));


	
	public T Register<T>(T element) where T : VmElement {
		if (ElementsById.TryGetValue(element.Id, out var el))
			throw new ArgumentException($"Element with id {element.Id} already exists.");
		return AddElement(element, element.GetType());
	}
	
	public void RemoveElement(VmElement? el) {
		if (el == null)
			return;
		
		el.OnDestroy(this);
		_referrers = null; // stale once the element set changes; rebuilt on next GetReferrers
		var id = el.Id;
		if (!ElementsById.Remove(id, out var element))
			throw new ArgumentException($"Element with id {id} does not exist.");

		var type = element.GetType();
		while (type != typeof(VmElement)) {
			if (ElementsByType.TryGetValue(type, out var list))
				list.Remove(element);
			type = type.BaseType!;
		}
	}

	public IEnumerable<T> GetElementsByType<T>() where T : VmElement {
		return ElementsByType.TryGetValue(typeof(T), out var elements) ? elements.Cast<T>() : [];
	}
	
	public T First<T>() where T: VmElement => GetElementsByType<T>().FirstOrDefault() ??
	                                          throw new Exception("No element found");
	
	public T First<T>(Func<T, bool> predicate) where T: VmElement => GetElementsByType<T>().FirstOrDefault(predicate) ??
																	 throw new Exception("No element found");
	
	public bool HasLanguage(string language) => Languages.Contains(language);
	public void AddLanguage(string language) => Languages.Add(language);
	
	/// <summary>
	/// Resolves "&lt;EventName&gt;_message_&lt;param&gt;" / "&lt;eventId&gt;_message_&lt;param&gt;" to its MessageInfo.
	///
	/// Engine events (Manual=False) have their messages regenerated at runtime from
	/// EngineAPIManager.GetAPIEventInfoByName(component, eventName); the &lt;MessagesInfo&gt; in the
	/// data mirrors the same [Event(...)] attributes, so reading it here is correct.
	/// The engine keys on (component, event name) and we only have the name, but that is
	/// unambiguous: Event.xml holds 16675 message entries over 162 distinct names with zero
	/// name-to-type conflicts, and manual events key on the event id rather than a name, so
	/// the two namespaces cannot overlap.
	///
	/// Names are matched verbatim. Eight carry a leading space after "_message_"
	/// (e.g. "OnFurnitureLoaded_message_ Region"), inherited from param specs written
	/// "Region:Region, level of disease". Do not trim either side.
	/// </summary>
	public bool TryResolveMessage(string name, out Message? result) {
		_messageIndex ??= BuildMessageIndex();
		return _messageIndex.TryGetValue(name, out result);
	}

	private Dictionary<string, Message> BuildMessageIndex() {
		var map = new Dictionary<string, Message>(StringComparer.Ordinal);
		foreach (var e in GetElementsByType<Event>())
			foreach (var m in e.Messages) map[m.Name] = m;
		return map;
	}
}
