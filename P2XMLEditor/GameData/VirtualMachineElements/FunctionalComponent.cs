using System.Xml;
using System.Xml.Linq;
using P2XMLEditor.Core;
using P2XMLEditor.Data;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.GameData.VirtualMachineElements.Interfaces;
using P2XMLEditor.Helper;
using P2XMLEditor.Parsing.RawData;
using static P2XMLEditor.Helper.XmlParsingHelper;
using static P2XMLEditor.Helper.XmlReaderExtensions;

#pragma warning disable CS8618

namespace P2XMLEditor.GameData.VirtualMachineElements;

public class FunctionalComponent(ulong id) : VmElement(id), IFiller<RawFunctionalComponentData> {
    protected override HashSet<string> KnownElements { get; } = ["Events", "Main", "LoadPriority", "Name", "Parent"];
    public List<Event> Events { get; set; }
    public bool? Main { get; set; }
    public long LoadPriority { get; set; }
    public string Name { get; set; }
    public ParameterHolder Parent { get; set; }

    public override XElement ToXml(WriterSettings settings) {
        var element = CreateBaseElement(Id);
        element.Add(CreateListElement("Events", Events.Select(e => e.Id.ToString())));
        if (Main != null)
            element.Add(CreateBoolElement("Main", (bool)Main));
        element.Add(
            new XElement("LoadPriority", LoadPriority),
            new XElement("Name", Name),
            new XElement("Parent", Parent.Id)
        );
        return element;
    }
    
    public void FillFromRawData(RawFunctionalComponentData data, VirtualMachine vm) {
        Events = data.EventIds?.Select(vm.GetElement<Event>).ToList() ?? [];
        Main = data.Main;
        LoadPriority = data.LoadPriority;
        Name = data.Name;
        Parent = vm.GetElement<ParameterHolder>(data.ParentId);
    }

    public void OnDestroy(VirtualMachine vm) {
        var compToRemove = Parent.FunctionalComponents.FirstOrDefault(f => f == this);
        if (compToRemove != null)
            Parent.FunctionalComponents.Remove(compToRemove);
        foreach (var @event in Events.ToList())
            vm.RemoveElement(@event);
    }
}

