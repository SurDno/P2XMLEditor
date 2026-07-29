using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.Helper;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes;

public class FunctionSourceParam<T>(ParameterSource source, VirtualMachine vm) {
	public ParameterSource Source => source;

	public T? Element {
		get {
			if (typeof(T) == typeof(BlueprintRef) && source.BlueprintReference != null)
				return (T)(object)source.BlueprintReference;
			if (typeof(T) == typeof(EntityRef) && source.EntityReference != null)
				return (T)(object)source.EntityReference;
			return source.ElementReference is T typed ? typed : default;
		}
	}

	public ParameterHolder? Prefix => source.PrefixHolder;

	public static FunctionSourceParam<T> Read(string data, VirtualMachine vm) =>
		new(ParameterSource.Create(data, vm, null, VmTypeHelper.GetVmType(typeof(T))), vm);

	// for "object" type (list funcs) where expectedType is evaluated from other parameter
	public static FunctionSourceParam<T> Read(string data, VirtualMachine vm, VmTypeInfo? expectedType) =>
		new(ParameterSource.Create(data, vm, null, expectedType), vm);

	public string Write() => source.Write();
	public override string ToString() => source.ToString();
}
