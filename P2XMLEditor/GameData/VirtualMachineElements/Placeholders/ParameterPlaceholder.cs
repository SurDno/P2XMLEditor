namespace P2XMLEditor.GameData.VirtualMachineElements.Placeholders;

// For cases where TargetParam property in Expression point to a non-existing Parameter.
public class ParameterPlaceholder(ulong id) : Parameter(id), IPlaceholder;