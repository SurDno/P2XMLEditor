using P2XMLEditor.GameData.Types;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.GameData.VirtualMachineElements.Placeholders;
using Action = P2XMLEditor.GameData.VirtualMachineElements.Action;

namespace P2XMLEditor.Core;

public class VirtualMachine {
	public readonly Dictionary<ulong, VmElement> ElementsById = new();

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


	public VirtualMachine(int capacity) {
		ElementsById = new Dictionary<ulong, VmElement>(capacity);
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
	
	public T First<T>(Func<T, bool> predicate) where T: VmElement => GetElementsByType<T>().FirstOrDefault(predicate) ??
	                                                                 throw new Exception("No element found");
	
	public bool HasLanguage(string language) => Languages.Contains(language);
	public void AddLanguage(string language) => Languages.Add(language);
}