namespace P2XMLEditor.GameData.VirtualMachineElements.Placeholders;

// For cases where an IStateRef parameter points at a State that no longer exists.
public class StatePlaceholder(ulong id) : State(id), IPlaceholder;