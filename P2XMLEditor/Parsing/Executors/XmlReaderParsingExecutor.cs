using P2XMLEditor.Parsing.Element;
using P2XMLEditor.Parsing.Element.XmlReaderParsers;
using P2XMLEditor.Parsing.RawData;

namespace P2XMLEditor.Parsing.Executors;

public class XmlReaderParsingExecutor : ParsingExecutor {

    protected internal override IParser<RawActionLineData> ActionLineLoader => new XmlReaderActionLineLoader();
    protected internal override IParser<RawActionData> ActionLoader => new XmlReaderActionLoader();
    protected internal override IParser<RawBlueprintData> BlueprintLoader => new XmlReaderBlueprintLoader();
    protected internal override IParser<RawBranchData> BranchLoader => new XmlReaderBranchLoader();
    protected internal override IParser<RawConditionData> ConditionLoader => new XmlReaderConditionLoader();
    protected internal override IParser<RawCustomTypeData> CustomTypeLoader => new XmlReaderCustomTypeLoader();
    protected internal override IParser<RawEntryPointData> EntryPointLoader => new XmlReaderEntryPointLoader();
    protected internal override IParser<RawEventData> EventLoader => new XmlReaderEventLoader();
    protected internal override IParser<RawExpressionData> ExpressionLoader => new XmlReaderExpressionLoader();
    protected internal override IParser<RawFunctionalComponentData> FunctionalComponentLoader => new XmlReaderFunctionalComponentLoader();
    protected internal override IParser<RawGameModeData> GameModeLoader => new XmlReaderGameModeLoader();

    protected internal override IParser<RawGameObjectData> ItemLoader => new XmlReaderGameObjectLoader();
    protected internal override IParser<RawGameObjectData> OtherLoader => new XmlReaderGameObjectLoader();
    protected internal override IParser<RawGameObjectData> SceneLoader => new XmlReaderGameObjectLoader();
    protected internal override IParser<RawGameObjectData> GeomLoader => new XmlReaderGameObjectLoader();
    protected internal override IParser<RawGameObjectData> CharacterLoader => new XmlReaderGameObjectLoader();

    protected internal override IParser<RawGameRootData> GameRootLoader => new XmlReaderGameRootLoader();
    protected internal override IParser<RawGameStringData> GameStringLoader => new XmlReaderGameStringLoader();
    protected internal override IParser<RawGraphLinkData> GraphLinkLoader => new XmlReaderGraphLinkLoader();
    protected internal override IParser<RawGraphData> GraphLoader => new XmlReaderGraphLoader();
    protected internal override IParser<RawMindMapLinkData> MindMapLinkLoader => new XmlReaderMindMapLinkLoader();
    protected internal override IParser<RawMindMapData> MindMapLoader => new XmlReaderMindMapLoader();
    protected internal override IParser<RawMindMapNodeContentData> MindMapNodeContentLoader => new XmlReaderMindMapNodeContentLoader();
    protected internal override IParser<RawMindMapNodeData> MindMapNodeLoader => new XmlReaderMindMapNodeLoader();
    protected internal override IParser<RawParameterData> ParameterLoader => new XmlReaderParameterLoader();
    protected internal override IParser<RawPartConditionData> PartConditionLoader => new XmlReaderPartConditionLoader();
    protected internal override IParser<RawQuestData> QuestLoader => new XmlReaderQuestLoader();
    protected internal override IParser<RawReplyData> ReplyLoader => new XmlReaderReplyLoader();
    protected internal override IParser<RawSampleData> SampleLoader => new XmlReaderSampleLoader();
    protected internal override IParser<RawSpeechData> SpeechLoader => new XmlReaderSpeechLoader();
    protected internal override IParser<RawStateData> StateLoader => new XmlReaderStateLoader();
    protected internal override IParser<RawTalkingData> TalkingLoader => new XmlReaderTalkingLoader();
}
