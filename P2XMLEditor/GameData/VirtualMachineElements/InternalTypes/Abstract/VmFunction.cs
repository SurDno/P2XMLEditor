using P2XMLEditor.GameData.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using P2XMLEditor.Core;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;

public abstract class VmFunction {


	public string Name => GetType().GetCustomAttribute<FunctionAttribute>()!.Name;
	public abstract VmType ReturnType { get; }
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
	
	public static VmFunction GetFunction(string name, VirtualMachine vm, string[] parameters) {
		if (!_functionTypes.TryGetValue(name, out var type))
			throw new ArgumentException($"Unknown function name: {name}");
			
		return (VmFunction)Activator.CreateInstance(type, vm, parameters)! ?? throw new InvalidOperationException();
	}
}