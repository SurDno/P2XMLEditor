namespace P2XMLEditor.GameData.VirtualMachineElements.Placeholders;

// For cases where a part of HierarchyGuid points to a non-existing Scene.
public class ScenePlaceholder(ulong id) : Scene(id), IPlaceholder;
