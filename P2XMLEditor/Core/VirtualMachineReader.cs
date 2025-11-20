using System.Xml.Linq;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.Helper;
using P2XMLEditor.Logging;
using Action = P2XMLEditor.GameData.VirtualMachineElements.Action;

namespace P2XMLEditor.Core;

public class VirtualMachineReader {
    private readonly string _vmPath;
    private readonly VirtualMachine _virtualMachine;
    private readonly Dictionary<string, VmElement.RawData> _rawData;

    public VirtualMachineReader(string vmPath) {
        _vmPath = vmPath;
        var dataCapacity = ReadDataCapacity(vmPath);
        _virtualMachine = new VirtualMachine(dataCapacity);
        _rawData = new Dictionary<string, VmElement.RawData>(dataCapacity);
    }

    private static int ReadDataCapacity(string vmPath) {
        const int fallbackCapacity = 131072;
        
        var versionPath = Path.Combine(vmPath, "Version.xml");
        if (!File.Exists(versionPath)) {
            Logger.Log(LogLevel.Error, $"No Version.xml is found, cannot infer data capacity! Loading the " +
                                       $"virtual machine will be significantly slower, and it can't be loaded by the " +
                                       $"game. Please ensure a valid Version.xml file exists in the VM folder.");
            return fallbackCapacity;
        }

        if (!int.TryParse(XDocument.Load(versionPath).Root?.Element("DataCapacity")?.Value, out var val)) {
            Logger.Log(LogLevel.Error, $"No DataCapacity tag in Version.xml or the value is invalid! Loading the " +
                                       $"virtual machine will be significantly slower, and it can't be loaded by the " +
                                       $"game. Please ensure a valid Version.xml file exists in the VM folder.");
            return fallbackCapacity;
        }

        Logger.Log(LogLevel.Info, $"DataCapacity inferred from Version.xml: {val}");
        return val;
    }

    private static readonly (Type Type, string FileName)[] TypeMappings = [
        (typeof(Action), "Action.xml"),
        (typeof(ActionLine), "ActionLine.xml"),
        (typeof(Blueprint), "Blueprint.xml"),
        (typeof(Branch), "Branch.xml"),
        (typeof(Character), "Character.xml"),
        (typeof(Condition), "Condition.xml"),
        (typeof(CustomType), "CustomType.xml"),
        (typeof(EntryPoint), "EntryPoint.xml"),
        (typeof(Event), "Event.xml"),
        (typeof(FunctionalComponent), "FunctionalComponent.xml"),
        (typeof(GameMode), "GameMode.xml"),
        (typeof(GameRoot), "GameRoot.xml"),
        (typeof(GameString), "GameString.xml"),
        (typeof(Geom), "Geom.xml"),
        (typeof(Graph), "Graph.xml"),
        (typeof(GraphLink), "GraphLink.xml"),
        (typeof(Item), "Item.xml"),
        (typeof(MindMap), "MindMap.xml"),
        (typeof(MindMapLink), "MindMapLink.xml"),
        (typeof(MindMapNode), "MindMapNode.xml"),
        (typeof(MindMapNodeContent), "MindMapNodeContent.xml"),
        (typeof(Other), "Other.xml"),
        (typeof(Parameter), "Parameter.xml"),
        (typeof(PartCondition), "PartCondition.xml"),
        (typeof(Quest), "Quest.xml"),
        (typeof(Reply), "Reply.xml"),
        (typeof(Sample), "Sample.xml"),
        (typeof(Scene), "Scene.xml"),
        (typeof(Speech), "Speech.xml"),
        (typeof(State), "State.xml"),
        (typeof(Talking), "Talking.xml"),
        (typeof(Expression), "Expression.xml")
    ];

    public VirtualMachine LoadVirtualMachine() {
        LoadAllRawData();
        CreateMinimalInstances();
        FillFromRawData();
        LoadLocalizations();
         
        return _virtualMachine;
    }

    [PerformanceLogHook]
    private void LoadAllRawData() {
        foreach (var (type, fileName) in TypeMappings) {
            var filePath = Path.Combine(_vmPath, fileName);
            if (!File.Exists(filePath)) continue;

            var minimalInstance = CreateMinimalInstance(type, null!);
            foreach (var element in XDocument.Load(filePath).Root!.Elements()) {
                try {
                    var rawData = minimalInstance.GetRawData(element);
                    rawData.SourceType = type;
                    _rawData[rawData.Id] = rawData;
                } catch (Exception ex) {
                    Logger.Log(LogLevel.Error, $"Error parsing {type.Name} from {fileName}: {ex.Message}");
                }
            }
        }
    }

    [PerformanceLogHook]
    private void CreateMinimalInstances() {
        var typeChains = ComputeTypeChains();
        
        foreach (var raw in _rawData.Values) {
            var element = CreateMinimalInstance(raw.SourceType, raw.Id);
            _virtualMachine.ElementsById[element.Id] = element;

            foreach (var t in typeChains[raw.SourceType])
                _virtualMachine.ElementsByType[t].Add(element);
        }
    }

    private Dictionary<Type, Type[]> ComputeTypeChains() {
        var typeChains = new Dictionary<Type, Type[]>();
        foreach (var (type, _) in TypeMappings) {
            var chain = new List<Type>();
            var cur = type;
            while (cur != typeof(VmElement) && cur != typeof(object)) {
                chain.Add(cur);
                cur = cur.BaseType!;
            }
            typeChains[type] = chain.ToArray();
        }

        foreach (var chain in typeChains.Values)
            foreach (var t in chain)
                if (!_virtualMachine.ElementsByType.ContainsKey(t))
                    _virtualMachine.ElementsByType[t] = new List<VmElement>();

        return typeChains;
    }

    [PerformanceLogHook]
    private void FillFromRawData() {
        foreach (var rawData in _rawData.Values)
            _virtualMachine.GetElement<VmElement>(rawData.Id).FillFromRawData(rawData, _virtualMachine);
    }

    [PerformanceLogHook]
    private void LoadLocalizations() {
        var localizationPath = Path.Combine(_vmPath, "Localizations");
        if (!Directory.Exists(localizationPath)) return;

        foreach (var file in Directory.GetFiles(localizationPath, "*.txt")) {
            var language = Path.GetFileNameWithoutExtension(file);
            _virtualMachine.AddLanguage(language);

            foreach (var line in File.ReadLines(file)) {
                var spaceIndex = line.IndexOf(' ');
                if (spaceIndex <= 0) continue;
                var id = line[..spaceIndex];
                var text = line[(spaceIndex + 1)..];
                if (!_virtualMachine.ElementsById.TryGetValue(id, out var element) ||
                    element is not GameString gameString) continue;
                gameString.SetText(text, language);
            }
        }
    }
    
    private static VmElement CreateMinimalInstance(Type type, string id) {
        return type switch {
            not null when type == typeof(Action) => new Action(id),
            not null when type == typeof(ActionLine) => new ActionLine(id),
            not null when type == typeof(Blueprint) => new Blueprint(id),
            not null when type == typeof(Branch) => new Branch(id),
            not null when type == typeof(Character) => new Character(id),
            not null when type == typeof(Condition) => new Condition(id),
            not null when type == typeof(CustomType) => new CustomType(id),
            not null when type == typeof(EntryPoint) => new EntryPoint(id),
            not null when type == typeof(Event) => new Event(id),
            not null when type == typeof(Expression) => new Expression(id),
            not null when type == typeof(FunctionalComponent) => new FunctionalComponent(id),
            not null when type == typeof(GameMode) => new GameMode(id),
            not null when type == typeof(GameRoot) => new GameRoot(id),
            not null when type == typeof(GameString) => new GameString(id),
            not null when type == typeof(Geom) => new Geom(id),
            not null when type == typeof(Graph) => new Graph(id),
            not null when type == typeof(GraphLink) => new GraphLink(id),
            not null when type == typeof(Item) => new Item(id),
            not null when type == typeof(MindMap) => new MindMap(id),
            not null when type == typeof(MindMapLink) => new MindMapLink(id),
            not null when type == typeof(MindMapNode) => new MindMapNode(id),
            not null when type == typeof(MindMapNodeContent) => new MindMapNodeContent(id),
            not null when type == typeof(Other) => new Other(id),
            not null when type == typeof(Parameter) => new Parameter(id),
            not null when type == typeof(PartCondition) => new PartCondition(id),
            not null when type == typeof(Quest) => new Quest(id),
            not null when type == typeof(Reply) => new Reply(id),
            not null when type == typeof(Sample) => new Sample(id),
            not null when type == typeof(Scene) => new Scene(id),
            not null when type == typeof(Speech) => new Speech(id),
            not null when type == typeof(State) => new State(id),
            not null when type == typeof(Talking) => new Talking(id),
            _ => throw new ArgumentException($"Unsupported type: {type?.Name}")
        };
    }
}