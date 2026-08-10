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

	public bool UseParallel { get; set; } = true;

	protected virtual string MindMapLinkFileName => "MindMapLink";

	/// <summary>
	/// The path a type's file sits at, given the base type name. Flat by default —
	/// "&lt;dir&gt;/Event.xml" — which is how release and demo lay their files out. The alpha format
	/// puts each type in its own upper-cased directory, so it overrides this.
	/// </summary>
	protected virtual string ResolveFile(string directory, string baseName, string extension) =>
		Path.Combine(directory, baseName + extension);

    public virtual void ExecuteAll(string directory)
	{
		ExecuteAll(directory, ".xml");
	}

	protected void ExecuteAll(string directory, string extension)
	{
        System.Action[] actions = [
			() => Load(EventLoader, ResolveFile(directory, "Event", extension), Events),
			() => Load(GraphLinkLoader, ResolveFile(directory, "GraphLink", extension), GraphLinks),
			() => Load(GameRootLoader, ResolveFile(directory, "GameRoot", extension), GameRoots),
			() => Load(StateLoader, ResolveFile(directory, "State", extension), States),
			() => Load(FunctionalComponentLoader, ResolveFile(directory, "FunctionalComponent", extension), FunctionalComponents),
			
			() => Load(ActionLineLoader, ResolveFile(directory, "ActionLine", extension), ActionLines),
			() => Load(ActionLoader, ResolveFile(directory, "Action", extension), Actions),
			() => Load(BranchLoader, ResolveFile(directory, "Branch", extension), Branches),
			() => Load(ConditionLoader, ResolveFile(directory, "Condition", extension), Conditions),
			() => Load(EntryPointLoader, ResolveFile(directory, "EntryPoint", extension), EntryPoints),
			() => Load(ExpressionLoader, ResolveFile(directory, "Expression", extension), Expressions),

			() => Load(ItemLoader, ResolveFile(directory, "Item", extension), Items),
			() => Load(OtherLoader, ResolveFile(directory, "Other", extension), Others),
			() => Load(SceneLoader, ResolveFile(directory, "Scene", extension), Scenes),
			() => Load(GeomLoader, ResolveFile(directory, "Geom", extension), Geoms),
			() => Load(CharacterLoader, ResolveFile(directory, "Character", extension), Characters),

			() => Load(GameStringLoader, ResolveFile(directory, "GameString", extension), GameStrings),
			() => Load(GraphLoader, ResolveFile(directory, "Graph", extension), Graphs),
			() => Load(MindMapNodeContentLoader, ResolveFile(directory, "MindMapNodeContent", extension), MindMapNodeContents),
			() => Load(MindMapNodeLoader, ResolveFile(directory, "MindMapNode", extension), MindMapNodes),
			() => Load(ParameterLoader, ResolveFile(directory, "Parameter", extension), Parameters),
			() => Load(PartConditionLoader, ResolveFile(directory, "PartCondition", extension), PartConditions),
			() => Load(QuestLoader, ResolveFile(directory, "Quest", extension), Quests),
			() => Load(ReplyLoader, ResolveFile(directory, "Reply", extension), Replies),
			() => Load(SampleLoader, ResolveFile(directory, "Sample", extension), Samples),
			() => Load(SpeechLoader, ResolveFile(directory, "Speech", extension), Speeches),
			() => Load(TalkingLoader, ResolveFile(directory, "Talking", extension), Talkings),
			
			() => Load(BlueprintLoader, ResolveFile(directory, "Blueprint", extension), Blueprints),
			() => Load(GameModeLoader, ResolveFile(directory, "GameMode", extension), GameModes),
			() => Load(CustomTypeLoader, ResolveFile(directory, "CustomType", extension), CustomTypes),
			() => Load(MindMapLoader, ResolveFile(directory, "MindMap", extension), MindMaps),
			() => Load(MindMapLinkLoader, ResolveFile(directory, MindMapLinkFileName, extension), MindMapLinks)
        ];

        if (UseParallel) {
            Parallel.Invoke(actions);
        } else {
            foreach (var action in actions) {
                action();
            }
        }
	}
	
	protected static void Load<T>(IParser<T> loader, string path, List<T> target) where T : struct {
		loader.ProcessFile(path, target);
	}
}
