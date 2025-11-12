using System.Xml.Linq;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.Helper;
using P2XMLEditor.Logging;
using Action = P2XMLEditor.GameData.VirtualMachineElements.Action;

namespace P2XMLEditor.Core;

public class VirtualMachineReader(string vmPath) {
    private readonly VirtualMachine _virtualMachine = new();
    private readonly Dictionary<string, VmElement.RawData> _rawData = new();

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
        foreach (var i in _rawData.Values)
            CreateAndAddMinimalInstance(i.SourceType, i.Id);
        foreach (var rawData in _rawData.Values)
            _virtualMachine.GetElement<VmElement>(rawData.Id).FillFromRawData(rawData, _virtualMachine);
        LoadLocalizations();

        foreach (var el in _virtualMachine.ElementsById.Where(el => el.Value.IsOrphaned()))
            Logger.Log(LogLevel.Warning, $"Orphaned {el.Value.GetType().Name} with ID {el.Value.Id}.");

        return _virtualMachine;
    }

    private void LoadAllRawData() {
        foreach (var (type, fileName) in TypeMappings) {
            var filePath = Path.Combine(vmPath, fileName);
            if (!File.Exists(filePath)) continue;

            foreach (var element in XDocument.Load(filePath).Root!.Elements("Item")) {
                try {
                    var rawData = CreateMinimalInstance(type, null!).GetRawData(element);
                    rawData.SourceType = type;
                    _rawData[rawData.Id] = rawData;
                } catch (Exception ex) {
                    Logger.Log(LogLevel.Error, $"Error parsing {type.Name} from {fileName}: {ex.Message}");
                }
            }
        }
    }

    private void LoadLocalizations() {
        var localizationPath = Path.Combine(vmPath, "Localizations");
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

    private static readonly Dictionary<Type, Func<string, VmElement>> ElementCreators = new() {
        { typeof(Action), id => new Action(id) },
        { typeof(ActionLine), id => new ActionLine(id) },
        { typeof(Blueprint), id => new Blueprint(id) },
        { typeof(Branch), id => new Branch(id) },
        { typeof(Character), id => new Character(id) },
        { typeof(Condition), id => new Condition(id) },
        { typeof(CustomType), id => new CustomType(id) },
        { typeof(EntryPoint), id => new EntryPoint(id) },
        { typeof(Event), id => new Event(id) },
        { typeof(Expression), id => new Expression(id) },
        { typeof(FunctionalComponent), id => new FunctionalComponent(id) },
        { typeof(GameMode), id => new GameMode(id) },
        { typeof(GameRoot), id => new GameRoot(id) },
        { typeof(GameString), id => new GameString(id) },
        { typeof(Geom), id => new Geom(id) },
        { typeof(Graph), id => new Graph(id) },
        { typeof(GraphLink), id => new GraphLink(id) },
        { typeof(Item), id => new Item(id) },
        { typeof(MindMap), id => new MindMap(id) },
        { typeof(MindMapLink), id => new MindMapLink(id) },
        { typeof(MindMapNode), id => new MindMapNode(id) },
        { typeof(MindMapNodeContent), id => new MindMapNodeContent(id) },
        { typeof(Other), id => new Other(id) },
        { typeof(Parameter), id => new Parameter(id) },
        { typeof(PartCondition), id => new PartCondition(id) },
        { typeof(Quest), id => new Quest(id) },
        { typeof(Reply), id => new Reply(id) },
        { typeof(Sample), id => new Sample(id) },
        { typeof(Scene), id => new Scene(id) },
        { typeof(Speech), id => new Speech(id) },
        { typeof(State), id => new State(id) },
        { typeof(Talking), id => new Talking(id) }
    };

    private VmElement CreateAndAddMinimalInstance(Type type, string id) {
        if (!ElementCreators.TryGetValue(type, out var creator))
            throw new ArgumentException($"Unsupported type: {type?.Name}");

        var element = creator(id);
        return _virtualMachine.AddElement(element);
    }

    private static VmElement CreateMinimalInstance(Type type, string id) {
        if (!ElementCreators.TryGetValue(type, out var creator))
            throw new ArgumentException($"Unsupported type: {type?.Name}");

        return creator(id);
    }
}