using System.Reflection;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;

public abstract class VmFunction {
	public enum FunctionReturnType {
		Void, // 172 functions.
		Bool, // 15 functions
		Int, // 9 functions
		Float, // 5
		ULong, // 8
		GameMode, // 1 function
		GameTime, // 5 functions
		EntityTagEnum, // 1 function
		VmType, // 1
		Object, // 2
		ContainerOpenStateEnum, // 1 
		Entity, // 2
		AreaEnum, // 1
		String, // 1
		ObjRef, // 3
		BlueprintRef, // 1
		GameLocalizationName, // 1
	}

	public string Name => GetType().GetCustomAttribute<FunctionAttribute>()!.Name;
	public abstract FunctionReturnType ReturnType { get; }
	public abstract int ParamCount { get; }
	public abstract List<string>? GetParamStrings();
	
	[AttributeUsage(AttributeTargets.Class)]
	protected class FunctionAttribute(string name) : Attribute {
		public string Name { get; } = name;
	}
    
	private static readonly Dictionary<string, Type> _functionTypes = new();

	static VmFunction() {
		foreach (var type in Assembly.GetExecutingAssembly().GetTypes()
			         .Where(t => t.GetCustomAttribute<FunctionAttribute>() != null))
			_functionTypes[type.GetCustomAttribute<FunctionAttribute>()!.Name] = type;
	}

	public static IEnumerable<string> GetAvailableFunctions() => _functionTypes.Keys;
    
	public static VmFunction GetFunction(string name, VirtualMachine vm, List<string> parameters) {
		if (!_functionTypes.TryGetValue(name, out var type))
			throw new ArgumentException($"Unknown function name: {name}");
            
		return (VmFunction)Activator.CreateInstance(type, vm, parameters)! ?? throw new InvalidOperationException();
	}
}