using P2XMLEditor.GameData.VirtualMachineElements.Abstract;

namespace P2XMLEditor.GameData.VirtualMachineElements.Placeholders;

// probably wouldnt be template placeholder
public class TemplatePlaceholder(ulong id) : VmElement(id), IPlaceholder;
