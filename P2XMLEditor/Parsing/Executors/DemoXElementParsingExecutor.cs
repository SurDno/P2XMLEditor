using P2XMLEditor.Parsing.Element;
using P2XMLEditor.Parsing.Element.DemoXElementParsers;
using P2XMLEditor.Parsing.Element.XElementParsers;
using P2XMLEditor.Parsing.RawData;

namespace P2XMLEditor.Parsing.Executors;

public class DemoXElementParsingExecutor : ParsingExecutor {
	protected override string MindMapLinkFileName => "MindMaLink";

	public override void ExecuteAll(string directory) {
		ExecuteAll(directory, ".xml.gz");
	}

	protected internal override IParser<RawActionData> ActionLoader => new DemoXElementActionLoader();
	protected internal override IParser<RawMindMapData> MindMapLoader => new DemoXElementMindMapLoader();
	protected internal override IParser<RawMindMapNodeData> MindMapNodeLoader => new DemoXElementMindMapNodeLoader();
	protected internal override IParser<RawActionLineData> ActionLineLoader => new DemoXElementActionLineLoader();
	protected internal override IParser<RawBlueprintData> BlueprintLoader => new DemoXElementBlueprintLoader();
	protected internal override IParser<RawBranchData> BranchLoader => new DemoXElementBranchLoader();
	protected internal override IParser<RawConditionData> ConditionLoader => new DemoXElementConditionLoader();
	protected internal override IParser<RawCustomTypeData> CustomTypeLoader => new DemoXElementCustomTypeLoader();
	protected internal override IParser<RawEntryPointData> EntryPointLoader => new DemoXElementEntryPointLoader();
	protected internal override IParser<RawEventData> EventLoader => new DemoXElementEventLoader();
	protected internal override IParser<RawExpressionData> ExpressionLoader => new DemoXElementExpressionLoader();
	protected internal override IParser<RawFunctionalComponentData> FunctionalComponentLoader => new DemoXElementFunctionalComponentLoader();
	protected internal override IParser<RawGameModeData> GameModeLoader => new DemoXElementGameModeLoader();

	protected internal override IParser<RawGameObjectData> ItemLoader => new DemoXElementGameObjectLoader();
	protected internal override IParser<RawGameObjectData> OtherLoader => new DemoXElementGameObjectLoader();
	protected internal override IParser<RawGameObjectData> SceneLoader => new DemoXElementGameObjectLoader();
	protected internal override IParser<RawGameObjectData> GeomLoader => new DemoXElementGameObjectLoader();
	protected internal override IParser<RawGameObjectData> CharacterLoader => new DemoXElementGameObjectLoader();

	protected internal override IParser<RawGameRootData> GameRootLoader => new DemoXElementGameRootLoader();
	protected internal override IParser<RawGameStringData> GameStringLoader => new DemoXElementGameStringLoader();
	protected internal override IParser<RawGraphLinkData> GraphLinkLoader => new DemoXElementGraphLinkLoader();
	protected internal override IParser<RawGraphData> GraphLoader => new DemoXElementGraphLoader();
	protected internal override IParser<RawMindMapLinkData> MindMapLinkLoader => new DemoXElementMindMapLinkLoader();
	protected internal override IParser<RawMindMapNodeContentData> MindMapNodeContentLoader => new DemoXElementMindMapNodeContentLoader();
	protected internal override IParser<RawParameterData> ParameterLoader => new DemoXElementParameterLoader();
	protected internal override IParser<RawPartConditionData> PartConditionLoader => new DemoXElementPartConditionLoader();
	protected internal override IParser<RawQuestData> QuestLoader => new DemoXElementQuestLoader();
	protected internal override IParser<RawReplyData> ReplyLoader => new DemoXElementReplyLoader();
	protected internal override IParser<RawSampleData> SampleLoader => new DemoXElementSampleLoader();
	protected internal override IParser<RawSpeechData> SpeechLoader => new DemoXElementSpeechLoader();
	protected internal override IParser<RawStateData> StateLoader => new DemoXElementStateLoader();
	protected internal override IParser<RawTalkingData> TalkingLoader => new DemoXElementTalkingLoader();
}
