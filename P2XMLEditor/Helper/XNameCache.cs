using System.Xml.Linq;

namespace P2XMLEditor.Helper;

public static class XNameCache {
	// ROOT
	public static readonly XName Item = "Item";
	public static readonly XName IdAttribute = "id";
	
	// COMMON
	public static readonly XName LocalContext = "LocalContext";
	public static readonly XName OrderIndex = "OrderIndex";
	public static readonly XName Name = "Name";
	public static readonly XName Parent = "Parent";
	public static readonly XName TargetObject = "TargetObject";
	public static readonly XName TargetParam = "TargetParam";
	public static readonly XName SourceParams = "SourceParams";
	public static readonly XName Events = "Events";
	public static readonly XName EntryPoints = "EntryPoints";
	
	// PARAMETER HOLDER
	public static readonly XName Static = "Static";
	public static readonly XName FunctionalComponents = "FunctionalComponents";
	public static readonly XName EventGraph = "EventGraph";
	public static readonly XName StandartParams = "StandartParams";
	public static readonly XName CustomParams = "CustomParams";
	public static readonly XName GameTimeContext = "GameTimeContext";
	public static readonly XName InheritanceInfo = "InheritanceInfo";
	public static readonly XName ChildObjects = "ChildObjects";
	
	// GAME OBJECT
	public static readonly XName WorldPositionGuid = "WorldPositionGuid";
	public static readonly XName EngineTemplateId = "EngineTemplateID";
	public static readonly XName EngineBaseTemplateId = "EngineBaseTemplateID";
	public static readonly XName Instantiated = "Instantiated";
	
	// NODE
	public static readonly XName InputLinks = "InputLinks";
	public static readonly XName OutputLinks = "OutputLinks";
	
	// LINK
	public static readonly XName Source = "Source";
	public static readonly XName Destination = "Destination";
	
	// DIALOGUE
	public static readonly XName Text = "Text";
	public static readonly XName OnlyOnce = "OnlyOnce";
	
	// BRANCH AND SPEECH?
	public static readonly XName IgnoreBlock = "IgnoreBlock";
	public static readonly XName Owner = "Owner";
	public static readonly XName Initial = "Initial";
	
	// ACTION
	public static readonly XName ActionType = "ActionType";
	public static readonly XName MathOperationType = "MathOperationType";
	public static readonly XName TargetFuncName = "TargetFuncName";
	public static readonly XName SourceExpression = "SourceExpression";
	
	// ACTION LINE
	public static readonly XName Actions = "Actions";
	public static readonly XName ActionLineType = "ActionLineType";
	public static readonly XName ActionLoopInfo = "ActionLoopInfo";
	public static readonly XName Start = "Start";
	public static readonly XName End = "End";
	public static readonly XName Random = "Random";
	
	// BRANCH
	public static readonly XName BranchConditions = "BranchConditions";
	public static readonly XName BranchType = "BranchType";
	public static readonly XName BranchVariantInfo = "BranchVariantInfo";
	public static readonly XName Type = "Type";
	
	// CONDITION
	public static readonly XName Predicates = "Predicates";
	public static readonly XName Operation = "Operation";
	
	// ENTRY POINT
	public static readonly XName ActionLine = "ActionLine";
	
	// EVENT
	public static readonly XName EventTime = "EventTime";
	public static readonly XName Manual = "Manual";
	public static readonly XName EventRaisingType = "EventRaisingType";
	public static readonly XName EventParameter = "EventParameter";
	public static readonly XName Condition = "Condition";
	public static readonly XName ChangeTo = "ChangeTo";
	public static readonly XName Repeated = "Repeated";
	public static readonly XName MessagesInfo = "MessagesInfo";
	
	// EXPRESSION
	public static readonly XName ExpressionType = "ExpressionType";
	public static readonly XName TargetFunctionName = "TargetFunctionName";
	public static readonly XName Const = "Const";
	public static readonly XName Inversion = "Inversion";
	public static readonly XName FormulaChilds = "FormulaChilds";
	public static readonly XName FormulaOperations = "FormulaOperations";
	
	// FUNCTIONAL COMPONENT
	public static readonly XName Main = "Main";
	public static readonly XName LoadPriority = "LoadPriority";
	
	// GAME MODE 
	public static readonly XName IsMain = "IsMain";
	public static readonly XName StartGameTime = "StartGameTime";
	public static readonly XName GameTimeSpeed = "GameTimeSpeed";
	public static readonly XName StartSolarTime = "StartSolarTime";
	public static readonly XName SolarTimeSpeed = "SolarTimeSpeed";
	public static readonly XName PlayerRef = "PlayerRef";
	
	// GAME ROOT
	public static readonly XName Samples = "Samples";
	public static readonly XName LogicMaps = "LogicMaps";
	public static readonly XName GameModes = "GameModes";
	public static readonly XName BaseToEngineGuidsTable = "BaseToEngineGuidsTable";
	public static readonly XName HierarchyScenesStructure = "HierarchyScenesStructure";
	public static readonly XName HierarchyEngineGuidsTable = "HierarchyEngineGuidsTable";
	public static readonly XName WorldObjectSaveOptimizeMode = "WorldObjectSaveOptimizeMode";
	public static readonly XName KeyAttribute = "key";
	
	// GRAPH
	public static readonly XName States = "States";
	public static readonly XName EventLinks = "EventLinks";
	public static readonly XName GraphType = "GraphType";
	public static readonly XName InputParamsInfo = "InputParamsInfo";
	public static readonly XName SubstituteGraph = "SubstituteGraph";
	
	// GRAPH LINK
	public static readonly XName Event = "Event";
	public static readonly XName EventObject = "EventObject";
	public static readonly XName SourceExitPointIndex = "SourceExitPointIndex";
	public static readonly XName DestEntryPointIndex = "DestEntryPointIndex";
	public static readonly XName Enabled = "Enabled";
	
	// MIND MAP
	public static readonly XName LogicMapType = "LogicMapType";
	public static readonly XName Title = "Title";
	public static readonly XName Nodes = "Nodes";
	public static readonly XName Links = "Links";
	
	// MIND MAP NODE
	public static readonly XName NodeContent = "NodeContent";
	public static readonly XName GameScreenPosX = "GameScreenPosX";
	public static readonly XName GameScreenPosY = "GameScreenPosY";
	public static readonly XName LogicMapNodeType = "LogicMapNodeType";
	
	// MIND MAP NODE CONTENT
	public static readonly XName ContentType = "ContentType";
	public static readonly XName Number = "Number";
	public static readonly XName ContentDescriptionText = "ContentDescriptionText";
	public static readonly XName ContentPicture = "ContentPicture";
	public static readonly XName ContentCondition = "ContentCondition";
	
	// PARAMETER
	public static readonly XName OwnerComponent = "OwnerComponent";
	public static readonly XName Value = "Value";
	public static readonly XName Implicit = "Implicit";
	public static readonly XName Custom = "Custom";
	
	// PART CONDITION
	public static readonly XName ConditionType = "ConditionType";
	public static readonly XName FirstExpression = "FirstExpression";
	public static readonly XName SecondExpression = "SecondExpression";
	
	// QUEST
	public static readonly XName StartEvent = "StartEvent";
	
	// REPLY
	public static readonly XName OnlyOneReply = "OnlyOneReply";
	public static readonly XName Default = "Default";
	public static readonly XName EnableCondition = "EnableCondition";
	
	// SAMPLE
	public static readonly XName SampleType = "SampleType";
	public static readonly XName EngineID = "EngineID";
	
	// SPEECH
	public static readonly XName Replyes = "Replyes";
	public static readonly XName AuthorGuid = "AuthorGuid";
	public static readonly XName IsTrade = "IsTrade";
}