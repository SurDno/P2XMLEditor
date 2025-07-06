using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.GameData.VirtualMachineElements.Placeholders;

namespace P2XMLEditor.Core;

public class VirtualMachine {
	public readonly Dictionary<string, VmElement> ElementsById = new();
	public readonly Dictionary<Type, List<VmElement>> ElementsByType = new();
	public HashSet<string> Languages { get; } = [];

	public T AddElement<T>(T element) where T : VmElement {
		ElementsById[element.Id] = element;
		var type = element.GetType();
		while (type != typeof(VmElement) && type != typeof(object)) {
			if (!ElementsByType.TryGetValue(type, out var list))
				ElementsByType[type] = list = [];
			list.Add(element);
			type = type.BaseType!;
		}
		return element;
	}
	
	public int GetDataCapacity() => ElementsById.Count(e => e.Value is not (ParameterPlaceholder or ScenePlaceholder));
	
	public T Register<T>(T element) where T : VmElement {
		if (ElementsById.TryGetValue(element.Id, out var el))
			throw new ArgumentException($"Element with id {element.Id} already exists.");
		return AddElement(element);
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