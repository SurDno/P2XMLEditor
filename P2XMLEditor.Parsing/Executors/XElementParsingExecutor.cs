using P2XMLEditor.Parsing.Element;
using P2XMLEditor.Parsing.Element.XElementParsers;
using P2XMLEditor.Parsing.RawData;

namespace P2XMLEditor.Parsing.Executors;

public class XElementParsingExecutor : ParsingExecutor {

    protected internal override IParser<RawActionLineData> ActionLineLoader => new XElementActionLineLoader();
    protected internal override IParser<RawActionData> ActionLoader => new XElementActionLoader();
    protected internal override IParser<RawBlueprintData> BlueprintLoader => new XElementBlueprintLoader();
    protected internal override IParser<RawBranchData> BranchLoader => new XElementBranchLoader();
    protected internal override IParser<RawConditionData> ConditionLoader => new XElementConditionLoader();
    protected internal override IParser<RawCustomTypeData> CustomTypeLoader => new XElementCustomTypeLoader();
    protected internal override IParser<RawEntryPointData> EntryPointLoader => new XElementEntryPointLoader();
    protected internal override IParser<RawEventData> EventLoader => new XElementEventLoader();
    protected internal override IParser<RawExpressionData> ExpressionLoader => new XElementExpressionLoader();
    protected internal override IParser<RawFunctionalComponentData> FunctionalComponentLoader => new XElementFunctionalComponentLoader();
    protected internal override IParser<RawGameModeData> GameModeLoader => new XElementGameModeLoader();

    protected internal override IParser<RawGameObjectData> ItemLoader => new XElementGameObjectLoader();
    protected internal override IParser<RawGameObjectData> OtherLoader => new XElementGameObjectLoader();
    protected internal override IParser<RawGameObjectData> SceneLoader => new XElementGameObjectLoader();
    protected internal override IParser<RawGameObjectData> GeomLoader => new XElementGameObjectLoader();
    protected internal override IParser<RawGameObjectData> CharacterLoader => new XElementGameObjectLoader();

    protected internal override IParser<RawGameRootData> GameRootLoader => new XElementGameRootLoader();
    protected internal override IParser<RawGameStringData> GameStringLoader => new XElementGameStringLoader();
    protected internal override IParser<RawGraphLinkData> GraphLinkLoader => new XElementGraphLinkLoader();
    protected internal override IParser<RawGraphData> GraphLoader => new XElementGraphLoader();
    protected internal override IParser<RawMindMapLinkData> MindMapLinkLoader => new XElementMindMapLinkLoader();
    protected internal override IParser<RawMindMapData> MindMapLoader => new XElementMindMapLoader();
    protected internal override IParser<RawMindMapNodeContentData> MindMapNodeContentLoader => new XElementMindMapNodeContentLoader();
    protected internal override IParser<RawMindMapNodeData> MindMapNodeLoader => new XElementMindMapNodeLoader();
    protected internal override IParser<RawParameterData> ParameterLoader => new XElementParameterLoader();
    protected internal override IParser<RawPartConditionData> PartConditionLoader => new XElementPartConditionLoader();
    protected internal override IParser<RawQuestData> QuestLoader => new XElementQuestLoader();
    protected internal override IParser<RawReplyData> ReplyLoader => new XElementReplyLoader();
    protected internal override IParser<RawSampleData> SampleLoader => new XElementSampleLoader();
    protected internal override IParser<RawSpeechData> SpeechLoader => new XElementSpeechLoader();
    protected internal override IParser<RawStateData> StateLoader => new XElementStateLoader();
    protected internal override IParser<RawTalkingData> TalkingLoader => new XElementTalkingLoader();
}
