using System.Diagnostics.CodeAnalysis;
using System.Xml;
using System.Xml.Linq;
using P2XMLEditor.Core;
using P2XMLEditor.Data;
using P2XMLEditor.GameData.VirtualMachineElements.Interfaces;

namespace P2XMLEditor.GameData.VirtualMachineElements.Abstract;

public abstract class VmElement(ulong id) {
    public ulong Id { get; } = id;
    public abstract XElement ToXml(WriterSettings settings);
    protected abstract HashSet<string> KnownElements { get; }

    public static T CreateDefault<T>(VirtualMachine vm, VmElement parent) where T : VmElement => 
        vm.Register((T)CreateDefaultImpl(vm, IdGenerator.GetNewId<T>(vm), typeof(T), parent));

    private static VmElement CreateDefaultImpl(VirtualMachine vm, ulong id, Type type, VmElement parent) {
        return type switch {
            not null when type == typeof(MindMap) =>  MindMap.New(vm, id, parent),
            not null when type == typeof(MindMapNode) => MindMapNode.New(vm, id, parent),
            not null when type == typeof(MindMapNodeContent) => MindMapNodeContent.New(vm, id, parent),
            not null when type == typeof(MindMapLink) => MindMapLink.New(vm, id, parent),
            not null when type == typeof(Condition) => Condition.New(vm, id, parent),
            not null when type == typeof(PartCondition) => PartCondition.New(vm, id, parent),
            not null when type == typeof(GameString) => GameString.New(vm, id, parent),
            not null when type == typeof(Expression) => Expression.New(vm, id, parent),
            not null when type == typeof(Parameter) => Parameter.New(vm, id, parent),
            _ => throw new ArgumentException($"Type {type!.Name} does not yet support default creation")
        };
    }

    public virtual bool IsOrphaned() => false;
    public virtual void OnDestroy(VirtualMachine vm) { } 
}