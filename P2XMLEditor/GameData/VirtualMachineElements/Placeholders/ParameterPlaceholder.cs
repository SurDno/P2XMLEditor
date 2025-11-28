using System;
using System.Collections.Generic;
using System.Xml.Linq;
using P2XMLEditor.Data;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.GameData.VirtualMachineElements.Interfaces;

namespace P2XMLEditor.GameData.VirtualMachineElements.Placeholders;

// For cases where TargetParam property in Expression point to a non-existing Parameter.
public class ParameterPlaceholder(ulong id) : VmElement(id), ICommonVariableParameter {
	protected override HashSet<string> KnownElements => throw new InvalidOperationException();

	public override XElement ToXml(WriterSettings settings) => throw new InvalidOperationException();
	public string ParamId => Id.ToString();
}