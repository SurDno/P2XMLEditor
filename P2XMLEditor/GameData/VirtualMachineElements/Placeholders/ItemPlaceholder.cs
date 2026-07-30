namespace P2XMLEditor.GameData.VirtualMachineElements.Placeholders;

// For cases where an entry in CombinationDataStruct points to a non-existing Item.
public class ItemPlaceholder(ulong id) : Item(id), IPlaceholder;