using System;
using System.Collections.Generic;
using System.Linq;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes;
using P2XMLEditor.GameData.VirtualMachineElements.Placeholders;
using Action = P2XMLEditor.GameData.VirtualMachineElements.Action;

namespace P2XMLEditor.Core;

public class VirtualMachine {
	public readonly Dictionary<ulong, VmElement> ElementsById = new();
	public readonly TemplateManager TemplateManagerInst;
	public readonly GameType Type;

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
	private Dictionary<string, MessageLookup>? _messageIndex;
	public readonly record struct MessageLookup(MessageInfo Info, Event Owner);
	
	// TODO: REFACTOR AWAY
	[field: ThreadStatic]
	public static VmElement? FillScope { get; private set; }
	public static FillScopeHandle EnterFillScope(VmElement? scope) => new(scope); 
	public readonly struct FillScopeHandle : IDisposable {
		private readonly VmElement? _previous;
		internal FillScopeHandle(VmElement? scope) { _previous = FillScope; FillScope = scope; }
		public void Dispose() => FillScope = _previous;
	}
	
	
	public VirtualMachine(int capacity, TemplateManager templateManagerInst, GameType type) {
		ElementsById = new Dictionary<ulong, VmElement>(capacity);
		TemplateManagerInst = templateManagerInst;
		Type = type;
	}
	
	public T AddElement<T>(T element, Type elementType) where T : VmElement {
		ElementsById[element.Id] = element;
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
	public bool TryResolveMessage(string messageName, out MessageLookup result) {
		_messageIndex ??= BuildMessageIndex();
		return _messageIndex.TryGetValue(messageName, out result);
	}

	/// <summary>Call whenever an Event's MessagesInfo is added, removed or renamed.</summary>
	public void InvalidateMessageIndex() => _messageIndex = null;
	
	private Dictionary<string, MessageLookup> BuildMessageIndex() {
		var map = new Dictionary<string, MessageLookup>(StringComparer.Ordinal);
		foreach (var e in GetElementsByType<Event>()) {
			if (e.MessagesInfo == null) continue;
			foreach (var m in e.MessagesInfo) map[m.Name] = new MessageLookup(m, e);
		}
		return map;
	}
}
