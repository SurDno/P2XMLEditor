namespace P2XMLEditor.GameData.VirtualMachineElements.Placeholders;

// For cases where Source property in GraphLink point to a non-existing Graph.
public class GraphPlaceholder(ulong id) : Graph(id), IPlaceholder;
