using System;
using System.Collections.Generic;
using System.Xml.Linq;
using P2XMLEditor.Data;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.GameData.VirtualMachineElements.Interfaces;

namespace P2XMLEditor.GameData.VirtualMachineElements.Placeholders;

// For cases where TargetParam property in Expression point to a non-existing Parameter.
public class ParameterPlaceholder(ulong id) : VmElement(id) {
	public string ParamId => Id.ToString();
}
