using System.Xml.Linq;
using P2XMLEditor.Core;
using P2XMLEditor.Data;
using P2XMLEditor.GameData.VirtualMachineElements;

namespace P2XMLEditor.Abstract;

public abstract class VmElement(string id) {
    public string Id { get; } = id;
    public abstract XElement ToXml(WriterSettings settings = default);
    protected abstract HashSet<string> KnownElements { get; }
    protected abstract RawData CreateRawData(XElement element);

    public RawData GetRawData(XElement element) {
        ValidateNoUnknownElements(element);
        return CreateRawData(element);
    }

    private void ValidateNoUnknownElements(XElement element) {
        var unknown = element.Elements().Select(e => e.Name.LocalName).Where(n => !KnownElements.Contains(n));
        
        if (unknown.Any()) 
            throw new($"{GetType().Name} {element.Attribute("id")?.Value}: Unknown elements: {string.Join(", ", unknown)}");
    }
    
    public static T CreateDefault<T>(VirtualMachine vm, VmElement parent) where T : VmElement => 
        vm.Register((T)CreateDefaultImpl(vm, IdGenerator.GetNewId<T>(vm), typeof(T), parent));

    protected abstract VmElement New(VirtualMachine vm, string id, VmElement parent);

    private static VmElement CreateDefaultImpl(VirtualMachine vm, string id, Type type, VmElement parent) {
        return type switch {
            not null when type == typeof(MindMap) => new MindMap(null!).New(vm, id, parent),
            not null when type == typeof(MindMapNode) => new MindMapNode(null!).New(vm, id, parent),
            not null when type == typeof(MindMapNodeContent) => new MindMapNodeContent(null!).New(vm, id, parent),
            not null when type == typeof(MindMapLink) => new MindMapLink(null!).New(vm, id, parent),
            not null when type == typeof(Condition) => new Condition(null!).New(vm, id, parent),
            not null when type == typeof(PartCondition) => new PartCondition(null!).New(vm, id, parent),
            not null when type == typeof(GameString) => new GameString(null!).New(vm, id, parent),
            not null when type == typeof(Expression) => new Expression(null!).New(vm, id, parent),
            not null when type == typeof(Parameter) => new Parameter(null!).New(vm, id, parent),
            _ => throw new ArgumentException($"Type {type!.Name} does not support default creation")
        };
    }
    
    public abstract void FillFromRawData(RawData rawData, VirtualMachine vm);
    public abstract record RawData(string Id) {
        public Type SourceType { get; set; } = null!;
    }

    public virtual bool IsOrphaned() => false;
    public virtual void OnDestroy(VirtualMachine vm) { } 
}