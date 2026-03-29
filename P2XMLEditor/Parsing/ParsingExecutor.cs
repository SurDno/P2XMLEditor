using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
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

    public readonly List<RawActionLineData> ActionLines = [];
    public readonly List<RawActionData> Actions = [];
    public readonly List<RawBlueprintData> Blueprints = [];
    public readonly List<RawBranchData> Branches = [];
    public readonly List<RawConditionData> Conditions = [];
    public readonly List<RawCustomTypeData> CustomTypes = [];
    public readonly List<RawEntryPointData> EntryPoints = [];
    public readonly List<RawEventData> Events = [];
    public readonly List<RawExpressionData> Expressions = [];
    public readonly List<RawFunctionalComponentData> FunctionalComponents = [];
    public readonly List<RawGameModeData> GameModes = [];

    public readonly List<RawGameObjectData> Items = [];
    public readonly List<RawGameObjectData> Others = [];
    public readonly List<RawGameObjectData> Scenes = [];
    public readonly List<RawGameObjectData> Geoms = [];
    public readonly List<RawGameObjectData> Characters = [];

    public readonly List<RawGameRootData> GameRoots = [];
    public readonly List<RawGameStringData> GameStrings = [];
    public readonly List<RawGraphLinkData> GraphLinks = [];
    public readonly List<RawGraphData> Graphs = [];
    public readonly List<RawMindMapLinkData> MindMapLinks = [];
    public readonly List<RawMindMapData> MindMaps = [];
    public readonly List<RawMindMapNodeContentData> MindMapNodeContents = [];
    public readonly List<RawMindMapNodeData> MindMapNodes = [];
    public readonly List<RawParameterData> Parameters = [];
    public readonly List<RawPartConditionData> PartConditions = [];
    public readonly List<RawQuestData> Quests = [];
    public readonly List<RawReplyData> Replies = [];
    public readonly List<RawSampleData> Samples = [];
    public readonly List<RawSpeechData> Speeches = [];
    public readonly List<RawStateData> States = [];
    public readonly List<RawTalkingData> Talkings = [];

    public void ExecuteAll(string directory)
    {
        // Arranged in the order of most-expensive to least-expensive in unmodded PathologicSandbox
        Parallel.Invoke(
            () => Load(EventLoader, Path.Combine(directory, "Event.xml"), Events),
            () => Load(GraphLinkLoader, Path.Combine(directory, "GraphLink.xml"), GraphLinks),
            () => Load(GameRootLoader, Path.Combine(directory, "GameRoot.xml"), GameRoots),
            () => Load(StateLoader, Path.Combine(directory, "State.xml"), States),
            () => Load(FunctionalComponentLoader, Path.Combine(directory, "FunctionalComponent.xml"), FunctionalComponents),
            
            () => Load(ActionLineLoader, Path.Combine(directory, "ActionLine.xml"), ActionLines),
            () => Load(ActionLoader, Path.Combine(directory, "Action.xml"), Actions),
            () => Load(BranchLoader, Path.Combine(directory, "Branch.xml"), Branches),
            () => Load(ConditionLoader, Path.Combine(directory, "Condition.xml"), Conditions),
            () => Load(EntryPointLoader, Path.Combine(directory, "EntryPoint.xml"), EntryPoints),
            () => Load(ExpressionLoader, Path.Combine(directory, "Expression.xml"), Expressions),

            () => Load(ItemLoader, Path.Combine(directory, "Item.xml"), Items),
            () => Load(OtherLoader, Path.Combine(directory, "Other.xml"), Others),
            () => Load(SceneLoader, Path.Combine(directory, "Scene.xml"), Scenes),
            () => Load(GeomLoader, Path.Combine(directory, "Geom.xml"), Geoms),
            () => Load(CharacterLoader, Path.Combine(directory, "Character.xml"), Characters),

            () => Load(GameStringLoader, Path.Combine(directory, "GameString.xml"), GameStrings),
            () => Load(GraphLoader, Path.Combine(directory, "Graph.xml"), Graphs),
            () => Load(MindMapNodeContentLoader, Path.Combine(directory, "MindMapNodeContent.xml"), MindMapNodeContents),
            () => Load(MindMapNodeLoader, Path.Combine(directory, "MindMapNode.xml"), MindMapNodes),
            () => Load(ParameterLoader, Path.Combine(directory, "Parameter.xml"), Parameters),
            () => Load(PartConditionLoader, Path.Combine(directory, "PartCondition.xml"), PartConditions),
            () => Load(QuestLoader, Path.Combine(directory, "Quest.xml"), Quests),
            () => Load(ReplyLoader, Path.Combine(directory, "Reply.xml"), Replies),
            () => Load(SampleLoader, Path.Combine(directory, "Sample.xml"), Samples),
            () => Load(SpeechLoader, Path.Combine(directory, "Speech.xml"), Speeches),
            () => Load(TalkingLoader, Path.Combine(directory, "Talking.xml"), Talkings),
            
            () => Load(BlueprintLoader, Path.Combine(directory, "Blueprint.xml"), Blueprints),
            () => Load(GameModeLoader, Path.Combine(directory, "GameMode.xml"), GameModes),
            () => Load(CustomTypeLoader, Path.Combine(directory, "CustomType.xml"), CustomTypes),
            () => Load(MindMapLoader, Path.Combine(directory, "MindMap.xml"), MindMaps),
            () => Load(MindMapLinkLoader, Path.Combine(directory, "MindMapLink.xml"), MindMapLinks)
        );
    }
    
    protected static void Load<T>(IParser<T> loader, string path, List<T> target) where T : struct {
        loader.ProcessFile(path, target);
    }
}
