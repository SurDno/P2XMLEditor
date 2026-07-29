using System;
using System.Collections.Generic;
using System.Xml.Linq;
using P2XMLEditor.Core;
using P2XMLEditor.Data;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.GameData.VirtualMachineElements.Enums;
using P2XMLEditor.GameData.VirtualMachineElements.Interfaces;
using P2XMLEditor.Helper;
using P2XMLEditor.Parsing.RawData;
using static P2XMLEditor.Helper.XmlParsingHelper;

#pragma warning disable CS8618

namespace P2XMLEditor.GameData.VirtualMachineElements;

public class Sample(ulong id) : VmElement(id), IFiller<RawSampleData> {
	public SampleType SampleType { get; set; }
	public string EngineId { get; set; }
	
	public void FillFromRawData(RawSampleData data, VirtualMachine vm) {
		SampleType = data.SampleType;
		EngineId = data.EngineId;
	}

	public static VmElement New(VirtualMachine vm, ulong id, VmElement parent) => throw new NotImplementedException();

	public override void OnDestroy(VirtualMachine vm) {
		vm.First<GameRoot>().Samples.Remove(this);
	}

	public string ParamId => id.ToString();
}
