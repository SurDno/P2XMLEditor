using System.Xml.Linq;
using P2XMLEditor.Abstract;
using P2XMLEditor.Core;
using P2XMLEditor.Data;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.Helper;
using static P2XMLEditor.Helper.XmlParsingHelper;

#pragma warning disable CS8618

namespace P2XMLEditor.GameData.VirtualMachineElements;

public class FunctionalComponent(string id) : VmElement(id) {
    protected override HashSet<string> KnownElements { get; } = ["Events", "Main", "LoadPriority", "Name", "Parent"];

    public List<Event> Events { get; set; }
    public bool Main { get; set; }
    public long LoadPriority { get; set; }
    public string Name { get; set; }
    public ParameterHolder Parent { get; set; }

    public record RawFunctionalComponentData(string Id, List<string>? EventIds, bool Main, long LoadPriority,
        string Name, string ParentId) : RawData(Id);

    public override XElement ToXml(WriterSettings settings) {
        var element = CreateBaseElement(Id);
        element.Add(CreateListElement("Events", Events.Select(e => e.Id)));
        element.Add(
            CreateBoolElement("Main", Main),
            new XElement("LoadPriority", LoadPriority),
            new XElement("Name", Name),
            new XElement("Parent", Parent.Id)
        );
        return element;
    }

    protected override RawData CreateRawData(XElement element) {
        return new RawFunctionalComponentData(
            element.Attribute("id")?.Value ?? throw new ArgumentException("Id missing"),
            ParseListElement(element, "Events"),
            ParseBool(GetRequiredElement(element, "Main")),
            ParseLong(GetRequiredElement(element, "LoadPriority")),
            GetRequiredElement(element, "Name").Value,
            GetRequiredElement(element, "Parent").Value
        );
    }

    public override void FillFromRawData(RawData rawData, VirtualMachine vm) {
        if (rawData is not RawFunctionalComponentData data)
            throw new ArgumentException($"Expected RawFunctionalComponentData but got {rawData.GetType()}");

        Events = data.EventIds?.Select(vm.GetElement<Event>).ToList() ?? [];
        Main = data.Main;
        LoadPriority = data.LoadPriority;
        Name = data.Name;
        Parent = vm.GetElement<ParameterHolder>(data.ParentId);
    }
    
    protected override VmElement New(VirtualMachine vm, string id, VmElement parent) => throw new NotImplementedException();

    public override void OnDestroy(VirtualMachine vm) {
        var compToRemove = Parent.FunctionalComponents.FirstOrDefault(f => f == this);
        if (compToRemove != null)
            Parent.FunctionalComponents.Remove(compToRemove);
    }
}

