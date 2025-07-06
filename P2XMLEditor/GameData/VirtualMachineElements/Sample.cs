using System.Xml.Linq;
using P2XMLEditor.Core;
using P2XMLEditor.Data;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.GameData.VirtualMachineElements.Enums;
using P2XMLEditor.GameData.VirtualMachineElements.Interfaces;
using P2XMLEditor.Helper;
using static P2XMLEditor.Helper.XmlParsingHelper;

#pragma warning disable CS8618

namespace P2XMLEditor.GameData.VirtualMachineElements;

public class Sample(string id) : VmElement(id), ICommonVariableParameter {
	protected override HashSet<string> KnownElements { get; } = ["SampleType", "EngineID"];
    
	public SampleType SampleType { get; set; }
	public string EngineId { get; set; }

	public record RawSampleData(string Id, string SampleType, string EngineId) : RawData(Id);

	public override XElement ToXml(WriterSettings settings) {
		var element = CreateBaseElement(Id);
		element.Add(
			new XElement("SampleType", SampleType.Serialize()),
			new XElement("EngineID", EngineId)
		);
		return element;
	}

	protected override RawData CreateRawData(XElement element) {
		return new RawSampleData(
			element.Attribute("id")?.Value ?? throw new ArgumentException("Id missing"),
			GetRequiredElement(element, "SampleType").Value,
			GetRequiredElement(element, "EngineID").Value
		);
	}

	public override void FillFromRawData(RawData rawData, VirtualMachine vm) {
		if (rawData is not RawSampleData data)
			throw new ArgumentException($"Expected RawSampleData but got {rawData.GetType()}");
		
		SampleType = data.SampleType.Deserialize<SampleType>();
		EngineId = data.EngineId;
	}
	
	protected override VmElement New(VirtualMachine vm, string id, VmElement parent) => throw new NotImplementedException();

	public override void OnDestroy(VirtualMachine vm) {
		vm.First<GameRoot>(_ => true).Samples.Remove(this);
	}
}