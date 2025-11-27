using P2XMLEditor.Logging;
using P2XMLEditor.Parsing.Element;
using P2XMLEditor.Parsing.RawData;

namespace P2XMLEditor.Parsing;

public abstract class ParsingExecutor {
    protected internal abstract IParser<RawActionLineData> ActionLineLoader { get; }
    protected internal abstract IParser<RawActionData> ActionLoader { get; }
    protected internal abstract IParser<RawBlueprintData> BlueprintLoader { get; }
    protected internal abstract IParser<RawBranchData> BranchLoader { get; }
    protected internal abstract IParser<RawConditionData> ConditionLoader { get; }
    protected internal abstract IParser<RawCustomTypeData> CustomTypeLoader { get; }
    protected internal abstract IParser<RawEntryPointData> EntryPointLoader { get; }
    protected internal abstract IParser<RawEventData> EventLoader { get; }
    protected internal abstract IParser<RawExpressionData> ExpressionLoader { get; }
    protected internal abstract IParser<RawFunctionalComponentData> FunctionalComponentLoader { get; }
    protected internal abstract IParser<RawGameModeData> GameModeLoader { get; }

    protected internal abstract IParser<RawGameObjectData> ItemLoader { get; }
    protected internal abstract IParser<RawGameObjectData> OtherLoader { get; }
    protected internal abstract IParser<RawGameObjectData> SceneLoader { get; }
    protected internal abstract IParser<RawGameObjectData> GeomLoader { get; }
    protected internal abstract IParser<RawGameObjectData> CharacterLoader { get; }

    protected internal abstract IParser<RawGameRootData> GameRootLoader { get; }
    protected internal abstract IParser<RawGameStringData> GameStringLoader { get; }
    protected internal abstract IParser<RawGraphLinkData> GraphLinkLoader { get; }
    protected internal abstract IParser<RawGraphData> GraphLoader { get; }
    protected internal abstract IParser<RawMindMapLinkData> MindMapLinkLoader { get; }
    protected internal abstract IParser<RawMindMapData> MindMapLoader { get; }
    protected internal abstract IParser<RawMindMapNodeContentData> MindMapNodeContentLoader { get; }
    protected internal abstract IParser<RawMindMapNodeData> MindMapNodeLoader { get; }
    protected internal abstract IParser<RawParameterData> ParameterLoader { get; }
    protected internal abstract IParser<RawPartConditionData> PartConditionLoader { get; }
    protected internal abstract IParser<RawQuestData> QuestLoader { get; }
    protected internal abstract IParser<RawReplyData> ReplyLoader { get; }
    protected internal abstract IParser<RawSampleData> SampleLoader { get; }
    protected internal abstract IParser<RawSpeechData> SpeechLoader { get; }
    protected internal abstract IParser<RawStateData> StateLoader { get; }
    protected internal abstract IParser<RawTalkingData> TalkingLoader { get; }

    public readonly List<RawActionLineData> ActionLines = new();
    public readonly List<RawActionData> Actions = new();
    public readonly List<RawBlueprintData> Blueprints = new();
    public readonly List<RawBranchData> Branches = new();
    public readonly List<RawConditionData> Conditions = new();
    public readonly List<RawCustomTypeData> CustomTypes = new();
    public readonly List<RawEntryPointData> EntryPoints = new();
    public readonly List<RawEventData> Events = new();
    public readonly List<RawExpressionData> Expressions = new();
    public readonly List<RawFunctionalComponentData> FunctionalComponents = new();
    public readonly List<RawGameModeData> GameModes = new();

    public readonly List<RawGameObjectData> Items = new();
    public readonly List<RawGameObjectData> Others = new();
    public readonly List<RawGameObjectData> Scenes = new();
    public readonly List<RawGameObjectData> Geoms = new();
    public readonly List<RawGameObjectData> Characters = new();

    public readonly List<RawGameRootData> GameRoots = new();
    public readonly List<RawGameStringData> GameStrings = new();
    public readonly List<RawGraphLinkData> GraphLinks = new();
    public readonly List<RawGraphData> Graphs = new();
    public readonly List<RawMindMapLinkData> MindMapLinks = new();
    public readonly List<RawMindMapData> MindMaps = new();
    public readonly List<RawMindMapNodeContentData> MindMapNodeContents = new();
    public readonly List<RawMindMapNodeData> MindMapNodes = new();
    public readonly List<RawParameterData> Parameters = new();
    public readonly List<RawPartConditionData> PartConditions = new();
    public readonly List<RawQuestData> Quests = new();
    public readonly List<RawReplyData> Replies = new();
    public readonly List<RawSampleData> Samples = new();
    public readonly List<RawSpeechData> Speeches = new();
    public readonly List<RawStateData> States = new();
    public readonly List<RawTalkingData> Talkings = new();

    [PerformanceLogHook]
    public void ExecuteAll(string directory) {
        Load(ActionLineLoader, Path.Combine(directory, "ActionLine.xml"), ActionLines);
        Load(ActionLoader, Path.Combine(directory, "Action.xml"), Actions);
        Load(BlueprintLoader, Path.Combine(directory, "Blueprint.xml"), Blueprints);
        Load(BranchLoader, Path.Combine(directory, "Branch.xml"), Branches);
        Load(ConditionLoader, Path.Combine(directory, "Condition.xml"), Conditions);
        Load(CustomTypeLoader, Path.Combine(directory, "CustomType.xml"), CustomTypes);
        Load(EntryPointLoader, Path.Combine(directory, "EntryPoint.xml"), EntryPoints);
        Load(EventLoader, Path.Combine(directory, "Event.xml"), Events);
        Load(ExpressionLoader, Path.Combine(directory, "Expression.xml"), Expressions);
        Load(FunctionalComponentLoader, Path.Combine(directory, "FunctionalComponent.xml"), FunctionalComponents);
        Load(GameModeLoader, Path.Combine(directory, "GameMode.xml"), GameModes);

        Load(ItemLoader, Path.Combine(directory, "Item.xml"), Items);
        Load(OtherLoader, Path.Combine(directory, "Other.xml"), Others);
        Load(SceneLoader, Path.Combine(directory, "Scene.xml"), Scenes);
        Load(GeomLoader, Path.Combine(directory, "Geom.xml"), Geoms);
        Load(CharacterLoader, Path.Combine(directory, "Character.xml"), Characters);

        Load(GameRootLoader, Path.Combine(directory, "GameRoot.xml"), GameRoots);
        Load(GameStringLoader, Path.Combine(directory, "GameString.xml"), GameStrings);
        Load(GraphLinkLoader, Path.Combine(directory, "GraphLink.xml"), GraphLinks);
        Load(GraphLoader, Path.Combine(directory, "Graph.xml"), Graphs);
        Load(MindMapLinkLoader, Path.Combine(directory, "MindMapLink.xml"), MindMapLinks);
        Load(MindMapLoader, Path.Combine(directory, "MindMap.xml"), MindMaps);
        Load(MindMapNodeContentLoader, Path.Combine(directory, "MindMapNodeContent.xml"), MindMapNodeContents);
        Load(MindMapNodeLoader, Path.Combine(directory, "MindMapNode.xml"), MindMapNodes);
        Load(ParameterLoader, Path.Combine(directory, "Parameter.xml"), Parameters);
        Load(PartConditionLoader, Path.Combine(directory, "PartCondition.xml"), PartConditions);
        Load(QuestLoader, Path.Combine(directory, "Quest.xml"), Quests);
        Load(ReplyLoader, Path.Combine(directory, "Reply.xml"), Replies);
        Load(SampleLoader, Path.Combine(directory, "Sample.xml"), Samples);
        Load(SpeechLoader, Path.Combine(directory, "Speech.xml"), Speeches);
        Load(StateLoader, Path.Combine(directory, "State.xml"), States);
        Load(TalkingLoader, Path.Combine(directory, "Talking.xml"), Talkings);
    }
    
    private static void Load<T>(IParser<T> loader, string path, List<T> target) where T : struct {
        loader.ProcessFile(path, target);
        Logger.Log(LogLevel.Info, $"Loaded {target.Count} raw elements from {path}");
    }
}
