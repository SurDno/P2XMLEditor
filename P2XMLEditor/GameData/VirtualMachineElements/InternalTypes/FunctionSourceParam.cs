using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.Helper;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes;

public class FunctionSourceParam<T>(ParameterSource source, VirtualMachine vm) {
	public ParameterSource Source => source;
	public T? Element {
		get {
			if (typeof(T) == typeof(BlueprintRef) && source.BlueprintReference != null) {
			return (T)(object)source.BlueprintReference;
		}
			if (typeof(T) == typeof(EntityRef) && source.EntityReference != null) {
			return (T)(object)source.EntityReference;
		}
			var elementReference = source.ElementReference;
			if (elementReference is T) {
			return (T)(object)((elementReference is T) ? elementReference : null);
		}
			return default(T);
		}
	}
	public ParameterHolder? Prefix => source.PrefixHolder;
	public static FunctionSourceParam<T> Read(string data, VirtualMachine vm) => new FunctionSourceParam<T>(ParameterSource.Create(data, vm, null, VmTypeHelper.GetVmType(typeof(T))), vm);
	public string Write() => source.Write();
	public override string ToString() => source.ToString();
}
