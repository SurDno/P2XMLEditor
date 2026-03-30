using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.Helper;
using P2XMLEditor.Logging;
using P2XMLEditor.Parsing;
using P2XMLEditor.Parsing.Executors;
using P2XMLEditor.Parsing.RawData;
using Action = P2XMLEditor.GameData.VirtualMachineElements.Action;

namespace P2XMLEditor.Core;

public class VirtualMachineReader {
	private readonly string _vmPath;
	private readonly VirtualMachine _vm;
	private readonly ParsingExecutor _executor;
	
	private Dictionary<byte, Type[]> _typeChains;

	public VirtualMachineReader(string vmPath, string templatesPath, ParsingMode mode, bool parallel = true) {
		_vmPath = vmPath;
		var type = DetectVmType(vmPath);
		_vm = new VirtualMachine(ReadDataCapacity(vmPath, type), GetTemplateManager(templatesPath), type);

		_executor = type == VmType.Demo ? new DemoXElementParsingExecutor() : mode switch {
			ParsingMode.Fastest => new FastestParsingExecutor(),
			ParsingMode.XElement => new XElementParsingExecutor(),
			_ => new XmlReaderParsingExecutor()
		};
		_executor.UseParallel = parallel;
	}

	private static VmType DetectVmType(string vmPath) {
		if (File.Exists(Path.Combine(vmPath, "Version.xml")))
			return VmType.Release;
		
		if (Directory.GetFiles(vmPath, "*.xml.gz").Length > 0)
			return VmType.Demo;
			
		return VmType.Release;
	}
	
	public static TemplateManager GetTemplateManager(string templatesPath) {
		var templateManager = new TemplateManager(templatesPath);
		templateManager.LoadTemplates();
		return templateManager;
	}

	private static int ReadDataCapacity(string vmPath, VmType type) {
		const int fallbackCapacity = 131072;
		var versionPath = Path.Combine(vmPath, "Version.xml");
		if (!File.Exists(versionPath)) {
			if (type == VmType.Release) {
				Logger.Log(LogLevel.Error, $"No Version.xml is found, cannot infer data capacity! Loading the " +
										   $"virtual machine will be significantly slower, and it can't be loaded by the " +
										   $"game. Please ensure a valid Version.xml file exists in the VM folder.");
			}
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

	[PerformanceLogHook]
	public VirtualMachine LoadVirtualMachine() {
		CallExecutors();
		CreateMinimalInstances();
		FillFromRawData();
		LoadLocalizations();

		return _vm;
	}

	[PerformanceLogHook]
	private void CallExecutors() {
		//try {
			_executor.ExecuteAll(_vmPath);
		//} catch (Exception e) {
		//	Logger.Log(LogLevel.Error, $"Error while executing VirtualMachine: {e.Message}");
		//}
	}

	[PerformanceLogHook]
	private void CreateMinimalInstances() {
		AddElements(_executor.GameRoots);
		AddElements(_executor.CustomTypes);
		AddElements(_executor.GameStrings);
		AddElements(_executor.Blueprints);
		AddElementsCharacter(_executor.Characters);
		AddElementsItem(_executor.Items);
		AddElementsOther(_executor.Others);
		AddElementsGeom(_executor.Geoms);
		AddElementsScene(_executor.Scenes);
		AddElements(_executor.FunctionalComponents);
		AddElements(_executor.GameModes);
		AddElements(_executor.Parameters);
		AddElements(_executor.Expressions);
		AddElements(_executor.PartConditions);
		AddElements(_executor.Conditions);
		AddElements(_executor.Branches);
		AddElements(_executor.Replies);
		AddElements(_executor.Speeches);
		AddElements(_executor.States);
		AddElements(_executor.Talkings);
		AddElements(_executor.Events);
		AddElements(_executor.ActionLines);
		AddElements(_executor.Actions);
		AddElements(_executor.EntryPoints);
		AddElements(_executor.GraphLinks);
		AddElements(_executor.Graphs);
		AddElements(_executor.MindMaps);
		AddElements(_executor.MindMapLinks);
		AddElements(_executor.MindMapNodes);
		AddElements(_executor.MindMapNodeContents);
		AddElements(_executor.Samples);
		AddElements(_executor.Quests);
	}

	private void AddElements(List<RawGameRootData> raws) {
		Span<RawGameRootData> span = CollectionsMarshal.AsSpan(raws);
		for (int i = 0; i < span.Length; i++) {
			ref RawGameRootData raw = ref span[i];
			var id = raw.Id;
			var elem = new GameRoot(id);
			_vm.ElementsById[id] = elem;
			_vm.ElementsByType[typeof(VmElement)].Add(elem);
			_vm.ElementsByType[typeof(ParameterHolder)].Add(elem);
			_vm.ElementsByType[typeof(GameRoot)].Add(elem);
		}
	}

	private void AddElements(List<RawCustomTypeData> raws) {
		Span<RawCustomTypeData> span = CollectionsMarshal.AsSpan(raws);
		for (int i = 0; i < span.Length; i++) {
			ref RawCustomTypeData raw = ref span[i];
			var id = raw.Id;
			var elem = new CustomType(id);
			_vm.ElementsById[id] = elem;
			_vm.ElementsByType[typeof(VmElement)].Add(elem);
			_vm.ElementsByType[typeof(CustomType)].Add(elem);
		}
	}

	private void AddElements(List<RawGameStringData> raws) {
		Span<RawGameStringData> span = CollectionsMarshal.AsSpan(raws);
		for (int i = 0; i < span.Length; i++) {
			ref RawGameStringData raw = ref span[i];
			var id = raw.Id;
			var elem = new GameString(id);
			_vm.ElementsById[id] = elem;
			_vm.ElementsByType[typeof(VmElement)].Add(elem);
			_vm.ElementsByType[typeof(GameString)].Add(elem);
		}
	}

	private void AddElements(List<RawBlueprintData> raws) {
		Span<RawBlueprintData> span = CollectionsMarshal.AsSpan(raws);
		for (int i = 0; i < span.Length; i++) {
			ref RawBlueprintData raw = ref span[i];
			var id = raw.Id;
			var elem = new Blueprint(id);
			_vm.ElementsById[id] = elem;
			_vm.ElementsByType[typeof(VmElement)].Add(elem);
			_vm.ElementsByType[typeof(ParameterHolder)].Add(elem);
			_vm.ElementsByType[typeof(Blueprint)].Add(elem);
		}
	}

	private void AddElementsCharacter(List<RawGameObjectData> raws) {
		Span<RawGameObjectData> span = CollectionsMarshal.AsSpan(raws);
		for (int i = 0; i < span.Length; i++) {
			ref RawGameObjectData raw = ref span[i];
			var id = raw.Id;
			var elem = new Character(id);
			_vm.ElementsById[id] = elem;
			_vm.ElementsByType[typeof(VmElement)].Add(elem);
			_vm.ElementsByType[typeof(ParameterHolder)].Add(elem);
			_vm.ElementsByType[typeof(GameObject)].Add(elem);
			_vm.ElementsByType[typeof(Character)].Add(elem);
		}
	}

	private void AddElementsItem(List<RawGameObjectData> raws) {
		Span<RawGameObjectData> span = CollectionsMarshal.AsSpan(raws);
		for (int i = 0; i < span.Length; i++) {
			ref RawGameObjectData raw = ref span[i];
			var id = raw.Id;
			var elem = new Item(id);
			_vm.ElementsById[id] = elem;
			_vm.ElementsByType[typeof(VmElement)].Add(elem);
			_vm.ElementsByType[typeof(ParameterHolder)].Add(elem);
			_vm.ElementsByType[typeof(GameObject)].Add(elem);
			_vm.ElementsByType[typeof(Item)].Add(elem);
		}
	}

	private void AddElementsOther(List<RawGameObjectData> raws) {
		Span<RawGameObjectData> span = CollectionsMarshal.AsSpan(raws);
		for (int i = 0; i < span.Length; i++) {
			ref RawGameObjectData raw = ref span[i];
			var id = raw.Id;
			var elem = new Other(id);
			_vm.ElementsById[id] = elem;
			_vm.ElementsByType[typeof(VmElement)].Add(elem);
			_vm.ElementsByType[typeof(ParameterHolder)].Add(elem);
			_vm.ElementsByType[typeof(GameObject)].Add(elem);
			_vm.ElementsByType[typeof(Other)].Add(elem);
		}
	}

	private void AddElementsScene(List<RawGameObjectData> raws) {
		Span<RawGameObjectData> span = CollectionsMarshal.AsSpan(raws);
		for (int i = 0; i < span.Length; i++) {
			ref RawGameObjectData raw = ref span[i];
			var id = raw.Id;
			var elem = new Scene(id);
			_vm.ElementsById[id] = elem;
			_vm.ElementsByType[typeof(VmElement)].Add(elem);
			_vm.ElementsByType[typeof(ParameterHolder)].Add(elem);
			_vm.ElementsByType[typeof(GameObject)].Add(elem);
			_vm.ElementsByType[typeof(Scene)].Add(elem);
		}
	}

	private void AddElementsGeom(List<RawGameObjectData> raws) {
		Span<RawGameObjectData> span = CollectionsMarshal.AsSpan(raws);
		for (int i = 0; i < span.Length; i++) {
			ref RawGameObjectData raw = ref span[i];
			var id = raw.Id;
			var elem = new Geom(id);
			_vm.ElementsById[id] = elem;
			_vm.ElementsByType[typeof(VmElement)].Add(elem);
			_vm.ElementsByType[typeof(ParameterHolder)].Add(elem);
			_vm.ElementsByType[typeof(GameObject)].Add(elem);
			_vm.ElementsByType[typeof(Geom)].Add(elem);
		}
	}

	private void AddElements(List<RawFunctionalComponentData> raws) {
		Span<RawFunctionalComponentData> span = CollectionsMarshal.AsSpan(raws);
		for (int i = 0; i < span.Length; i++) {
			ref RawFunctionalComponentData raw = ref span[i];
			var id = raw.Id;
			var elem = new FunctionalComponent(id);
			_vm.ElementsById[id] = elem;
			_vm.ElementsByType[typeof(VmElement)].Add(elem);
			_vm.ElementsByType[typeof(FunctionalComponent)].Add(elem);
		}
	}

	private void AddElements(List<RawGameModeData> raws) {
		Span<RawGameModeData> span = CollectionsMarshal.AsSpan(raws);
		for (int i = 0; i < span.Length; i++) {
			ref RawGameModeData raw = ref span[i];
			var id = raw.Id;
			var elem = new GameMode(id);
			_vm.ElementsById[id] = elem;
			_vm.ElementsByType[typeof(VmElement)].Add(elem);
			_vm.ElementsByType[typeof(GameMode)].Add(elem);
		}
	}

	private void AddElements(List<RawParameterData> raws) {
		Span<RawParameterData> span = CollectionsMarshal.AsSpan(raws);
		for (int i = 0; i < span.Length; i++) {
			ref RawParameterData raw = ref span[i];
			var id = raw.Id;
			var elem = new Parameter(id);
			_vm.ElementsById[id] = elem;
			_vm.ElementsByType[typeof(VmElement)].Add(elem);
			_vm.ElementsByType[typeof(Parameter)].Add(elem);
		}
	}

	private void AddElements(List<RawExpressionData> raws) {
		Span<RawExpressionData> span = CollectionsMarshal.AsSpan(raws);
		for (int i = 0; i < span.Length; i++) {
			ref RawExpressionData raw = ref span[i];
			var id = raw.Id;
			var elem = new Expression(id);
			_vm.ElementsById[id] = elem;
			_vm.ElementsByType[typeof(VmElement)].Add(elem);
			_vm.ElementsByType[typeof(Expression)].Add(elem);
		}
	}

	private void AddElements(List<RawPartConditionData> raws) {
		Span<RawPartConditionData> span = CollectionsMarshal.AsSpan(raws);
		for (int i = 0; i < span.Length; i++) {
			ref RawPartConditionData raw = ref span[i];
			var id = raw.Id;
			var elem = new PartCondition(id);
			_vm.ElementsById[id] = elem;
			_vm.ElementsByType[typeof(VmElement)].Add(elem);
			_vm.ElementsByType[typeof(PartCondition)].Add(elem);
		}
	}

	private void AddElements(List<RawConditionData> raws) {
		Span<RawConditionData> span = CollectionsMarshal.AsSpan(raws);
		for (int i = 0; i < span.Length; i++) {
			ref RawConditionData raw = ref span[i];
			var id = raw.Id;
			var elem = new Condition(id);
			_vm.ElementsById[id] = elem;
			_vm.ElementsByType[typeof(VmElement)].Add(elem);
			_vm.ElementsByType[typeof(Condition)].Add(elem);
		}
	}

	private void AddElements(List<RawBranchData> raws) {
		Span<RawBranchData> span = CollectionsMarshal.AsSpan(raws);
		for (int i = 0; i < span.Length; i++) {
			ref RawBranchData raw = ref span[i];
			var id = raw.Id;
			var elem = new Branch(id);
			_vm.ElementsById[id] = elem;
			_vm.ElementsByType[typeof(VmElement)].Add(elem);
			_vm.ElementsByType[typeof(Branch)].Add(elem);
		}
	}

	private void AddElements(List<RawReplyData> raws) {
		Span<RawReplyData> span = CollectionsMarshal.AsSpan(raws);
		for (int i = 0; i < span.Length; i++) {
			ref RawReplyData raw = ref span[i];
			var id = raw.Id;
			var elem = new Reply(id);
			_vm.ElementsById[id] = elem;
			_vm.ElementsByType[typeof(VmElement)].Add(elem);
			_vm.ElementsByType[typeof(Reply)].Add(elem);
		}
	}

	private void AddElements(List<RawSpeechData> raws) {
		Span<RawSpeechData> span = CollectionsMarshal.AsSpan(raws);
		for (int i = 0; i < span.Length; i++) {
			ref RawSpeechData raw = ref span[i];
			var id = raw.Id;
			var elem = new Speech(id);
			_vm.ElementsById[id] = elem;
			_vm.ElementsByType[typeof(VmElement)].Add(elem);
			_vm.ElementsByType[typeof(Speech)].Add(elem);
		}
	}

	private void AddElements(List<RawStateData> raws) {
		Span<RawStateData> span = CollectionsMarshal.AsSpan(raws);
		for (int i = 0; i < span.Length; i++) {
			ref RawStateData raw = ref span[i];
			var id = raw.Id;
			var elem = new State(id);
			_vm.ElementsById[id] = elem;
			_vm.ElementsByType[typeof(VmElement)].Add(elem);
			_vm.ElementsByType[typeof(State)].Add(elem);
		}
	}

	private void AddElements(List<RawTalkingData> raws) {
		Span<RawTalkingData> span = CollectionsMarshal.AsSpan(raws);
		for (int i = 0; i < span.Length; i++) {
			ref RawTalkingData raw = ref span[i];
			var id = raw.Id;
			var elem = new Talking(id);
			_vm.ElementsById[id] = elem;
			_vm.ElementsByType[typeof(VmElement)].Add(elem);
			_vm.ElementsByType[typeof(Talking)].Add(elem);
		}
	}

	private void AddElements(List<RawEventData> raws) {
		Span<RawEventData> span = CollectionsMarshal.AsSpan(raws);
		for (int i = 0; i < span.Length; i++) {
			ref RawEventData raw = ref span[i];
			var id = raw.Id;
			var elem = new Event(id);
			_vm.ElementsById[id] = elem;
			_vm.ElementsByType[typeof(VmElement)].Add(elem);
			_vm.ElementsByType[typeof(Event)].Add(elem);
		}
	}

	private void AddElements(List<RawActionLineData> raws) {
		Span<RawActionLineData> span = CollectionsMarshal.AsSpan(raws);
		for (int i = 0; i < span.Length; i++) {
			ref RawActionLineData raw = ref span[i];
			var id = raw.Id;
			var elem = new ActionLine(id);
			_vm.ElementsById[id] = elem;
			_vm.ElementsByType[typeof(VmElement)].Add(elem);
			_vm.ElementsByType[typeof(ActionLine)].Add(elem);
		}
	}

	private void AddElements(List<RawActionData> raws) {
		Span<RawActionData> span = CollectionsMarshal.AsSpan(raws);
		for (int i = 0; i < span.Length; i++) {
			ref RawActionData raw = ref span[i];
			var id = raw.Id;
			var elem = new Action(id);
			_vm.ElementsById[id] = elem;
			_vm.ElementsByType[typeof(VmElement)].Add(elem);
			_vm.ElementsByType[typeof(Action)].Add(elem);
		}
	}

	private void AddElements(List<RawEntryPointData> raws) {
		Span<RawEntryPointData> span = CollectionsMarshal.AsSpan(raws);
		for (int i = 0; i < span.Length; i++) {
			ref RawEntryPointData raw = ref span[i];
			var id = raw.Id;
			var elem = new EntryPoint(id);
			_vm.ElementsById[id] = elem;
			_vm.ElementsByType[typeof(VmElement)].Add(elem);
			_vm.ElementsByType[typeof(EntryPoint)].Add(elem);
		}
	}

	private void AddElements(List<RawGraphLinkData> raws) {
		Span<RawGraphLinkData> span = CollectionsMarshal.AsSpan(raws);
		for (int i = 0; i < span.Length; i++) {
			ref RawGraphLinkData raw = ref span[i];
			var id = raw.Id;
			var elem = new GraphLink(id);
			_vm.ElementsById[id] = elem;
			_vm.ElementsByType[typeof(VmElement)].Add(elem);
			_vm.ElementsByType[typeof(GraphLink)].Add(elem);
		}
	}

	private void AddElements(List<RawGraphData> raws) {
		Span<RawGraphData> span = CollectionsMarshal.AsSpan(raws);
		for (int i = 0; i < span.Length; i++) {
			ref RawGraphData raw = ref span[i];
			var id = raw.Id;
			var elem = new Graph(id);
			_vm.ElementsById[id] = elem;
			_vm.ElementsByType[typeof(VmElement)].Add(elem);
			_vm.ElementsByType[typeof(Graph)].Add(elem);
		}
	}

	private void AddElements(List<RawMindMapData> raws) {
		Span<RawMindMapData> span = CollectionsMarshal.AsSpan(raws);
		for (int i = 0; i < span.Length; i++) {
			ref RawMindMapData raw = ref span[i];
			var id = raw.Id;
			var elem = new MindMap(id);
			_vm.ElementsById[id] = elem;
			_vm.ElementsByType[typeof(VmElement)].Add(elem);
			_vm.ElementsByType[typeof(MindMap)].Add(elem);
		}
	}

	private void AddElements(List<RawMindMapLinkData> raws) {
		Span<RawMindMapLinkData> span = CollectionsMarshal.AsSpan(raws);
		for (int i = 0; i < span.Length; i++) {
			ref RawMindMapLinkData raw = ref span[i];
			var id = raw.Id;
			var elem = new MindMapLink(id);
			_vm.ElementsById[id] = elem;
			_vm.ElementsByType[typeof(VmElement)].Add(elem);
			_vm.ElementsByType[typeof(MindMapLink)].Add(elem);
		}
	}

	private void AddElements(List<RawMindMapNodeData> raws) {
		Span<RawMindMapNodeData> span = CollectionsMarshal.AsSpan(raws);
		for (int i = 0; i < span.Length; i++) {
			ref RawMindMapNodeData raw = ref span[i];
			var id = raw.Id;
			var elem = new MindMapNode(id);
			_vm.ElementsById[id] = elem;
			_vm.ElementsByType[typeof(VmElement)].Add(elem);
			_vm.ElementsByType[typeof(MindMapNode)].Add(elem);
		}
	}

	private void AddElements(List<RawMindMapNodeContentData> raws) {
		Span<RawMindMapNodeContentData> span = CollectionsMarshal.AsSpan(raws);
		for (int i = 0; i < span.Length; i++) {
			ref RawMindMapNodeContentData raw = ref span[i];
			var id = raw.Id;
			var elem = new MindMapNodeContent(id);
			_vm.ElementsById[id] = elem;
			_vm.ElementsByType[typeof(VmElement)].Add(elem);
			_vm.ElementsByType[typeof(MindMapNodeContent)].Add(elem);
		}
	}

	private void AddElements(List<RawSampleData> raws) {
		Span<RawSampleData> span = CollectionsMarshal.AsSpan(raws);
		for (int i = 0; i < span.Length; i++) {
			ref RawSampleData raw = ref span[i];
			var id = raw.Id;
			var elem = new Sample(id);
			_vm.ElementsById[id] = elem;
			_vm.ElementsByType[typeof(VmElement)].Add(elem);
			_vm.ElementsByType[typeof(Sample)].Add(elem);
		}
	}

	private void AddElements(List<RawQuestData> raws) {
		Span<RawQuestData> span = CollectionsMarshal.AsSpan(raws);
		for (int i = 0; i < span.Length; i++) {
			ref RawQuestData raw = ref span[i];
			var id = raw.Id;
			var elem = new Quest(id);
			_vm.ElementsById[id] = elem;
			_vm.ElementsByType[typeof(VmElement)].Add(elem);
			_vm.ElementsByType[typeof(ParameterHolder)].Add(elem);
			_vm.ElementsByType[typeof(Quest)].Add(elem);
		}
	}


	[PerformanceLogHook]
	private void FillGameRoots() {
		foreach (var raw in _executor.GameRoots)
			_vm.GetElement<GameRoot>(raw.Id).FillFromRawData(raw, _vm);
	}

	private void FillCustomTypes() {
		foreach (var raw in _executor.CustomTypes)
			_vm.GetElement<CustomType>(raw.Id).FillFromRawData(raw, _vm);
	}

	[PerformanceLogHook]
	private void FillGameStrings() {
		foreach (var raw in _executor.GameStrings)
			_vm.GetElement<GameString>(raw.Id).FillFromRawData(raw, _vm);
	}

	[PerformanceLogHook]
	private void FillBlueprints() {
		foreach (var raw in _executor.Blueprints)
			_vm.GetElement<Blueprint>(raw.Id).FillFromRawData(raw, _vm);
	}

	[PerformanceLogHook]
	private void FillCharacters() {
		foreach (var raw in _executor.Characters)
			_vm.GetElement<Character>(raw.Id).FillFromRawData(raw, _vm);
	}

	[PerformanceLogHook]
	private void FillItems() {
		foreach (var raw in _executor.Items)
			_vm.GetElement<Item>(raw.Id).FillFromRawData(raw, _vm);
	}

	[PerformanceLogHook]
	private void FillOthers() {
		foreach (var raw in _executor.Others)
			_vm.GetElement<Other>(raw.Id).FillFromRawData(raw, _vm);
	}

	[PerformanceLogHook]
	private void FillGeoms() {
		foreach (var raw in _executor.Geoms)
			_vm.GetElement<Geom>(raw.Id).FillFromRawData(raw, _vm);
	}

	[PerformanceLogHook]
	private void FillScenes() {
		foreach (var raw in _executor.Scenes)
			_vm.GetElement<Scene>(raw.Id).FillFromRawData(raw, _vm);
	}

	[PerformanceLogHook]
	private void FillFunctionalComponents() {
		foreach (var raw in _executor.FunctionalComponents)
			_vm.GetElement<FunctionalComponent>(raw.Id).FillFromRawData(raw, _vm);
	}

	private void FillGameModes() {
		foreach (var raw in _executor.GameModes)
			_vm.GetElement<GameMode>(raw.Id).FillFromRawData(raw, _vm);
	}

	[PerformanceLogHook]
	private void FillParameters() {
		foreach (var raw in _executor.Parameters)
			_vm.GetElement<Parameter>(raw.Id).FillFromRawData(raw, _vm);
	}

	[PerformanceLogHook]
	private void FillExpressions() {
		foreach (var raw in _executor.Expressions)
			_vm.GetElement<Expression>(raw.Id).FillFromRawData(raw, _vm);
	}

	[PerformanceLogHook]
	private void FillPartConditions() {
		foreach (var raw in _executor.PartConditions)
			_vm.GetElement<PartCondition>(raw.Id).FillFromRawData(raw, _vm);
	}

	[PerformanceLogHook]
	private void FillConditions() {
		foreach (var raw in _executor.Conditions)
			_vm.GetElement<Condition>(raw.Id).FillFromRawData(raw, _vm);
	}

	[PerformanceLogHook]
	private void FillBranches() {
		foreach (var raw in _executor.Branches)
			_vm.GetElement<Branch>(raw.Id).FillFromRawData(raw, _vm);
	}

	[PerformanceLogHook]
	private void FillReplies() {
		foreach (var raw in _executor.Replies)
			_vm.GetElement<Reply>(raw.Id).FillFromRawData(raw, _vm);
	}

	[PerformanceLogHook]
	private void FillSpeeches() {
		foreach (var raw in _executor.Speeches)
			_vm.GetElement<Speech>(raw.Id).FillFromRawData(raw, _vm);
	}

	[PerformanceLogHook]
	private void FillStates() {
		foreach (var raw in _executor.States)
			_vm.GetElement<State>(raw.Id).FillFromRawData(raw, _vm);
	}

	[PerformanceLogHook]
	private void FillTalkings() {
		foreach (var raw in _executor.Talkings)
			_vm.GetElement<Talking>(raw.Id).FillFromRawData(raw, _vm);
	}

	[PerformanceLogHook]
	private void FillEvents() {
		foreach (var raw in _executor.Events)
			_vm.GetElement<Event>(raw.Id).FillFromRawData(raw, _vm);
	}

	[PerformanceLogHook]
	private void FillActionLines() {
		foreach (var raw in _executor.ActionLines)
			_vm.GetElement<ActionLine>(raw.Id).FillFromRawData(raw, _vm);
	}

	[PerformanceLogHook]
	private void FillActions() {
		foreach (var raw in _executor.Actions)
			_vm.GetElement<Action>(raw.Id).FillFromRawData(raw, _vm);
	}

	[PerformanceLogHook]
	private void FillEntryPoints() {
		foreach (var raw in _executor.EntryPoints)
			_vm.GetElement<EntryPoint>(raw.Id).FillFromRawData(raw, _vm);
	}

	[PerformanceLogHook]
	private void FillGraphLinks() {
		foreach (var raw in _executor.GraphLinks)
			_vm.GetElement<GraphLink>(raw.Id).FillFromRawData(raw, _vm);
	}

	[PerformanceLogHook]
	private void FillGraphs() {
		foreach (var raw in _executor.Graphs)
			_vm.GetElement<Graph>(raw.Id).FillFromRawData(raw, _vm);
	}

	private void FillMindMaps() {
		foreach (var raw in _executor.MindMaps)
			_vm.GetElement<MindMap>(raw.Id).FillFromRawData(raw, _vm);
	}

	private void FillMindMapLinks() {
		foreach (var raw in _executor.MindMapLinks)
			_vm.GetElement<MindMapLink>(raw.Id).FillFromRawData(raw, _vm);
	}

	private void FillMindMapNodes() {
		foreach (var raw in _executor.MindMapNodes)
			_vm.GetElement<MindMapNode>(raw.Id).FillFromRawData(raw, _vm);
	}

	private void FillMindMapNodeContents() {
		foreach (var raw in _executor.MindMapNodeContents)
			_vm.GetElement<MindMapNodeContent>(raw.Id).FillFromRawData(raw, _vm);
	}

	private void FillSamples() {
		foreach (var raw in _executor.Samples)
			_vm.GetElement<Sample>(raw.Id).FillFromRawData(raw, _vm);
	}

	[PerformanceLogHook]
	private void FillQuests() {
		foreach (var raw in _executor.Quests)
			_vm.GetElement<Quest>(raw.Id).FillFromRawData(raw, _vm);
	}

	[PerformanceLogHook]
	private void FillFromRawData() {
		FillGameRoots();
		FillCustomTypes();
		FillGameStrings();
		FillBlueprints();
		FillCharacters();
		FillItems();
		FillOthers();
		FillGeoms();
		FillScenes();
		FillFunctionalComponents();
		FillGameModes();
		FillParameters();
		FillExpressions();
		FillPartConditions();
		FillConditions();
		FillBranches();
		FillReplies();
		FillSpeeches();
		FillStates();
		FillTalkings();
		FillEvents();
		FillActionLines();
		FillActions();
		FillEntryPoints();
		FillGraphLinks();
		FillGraphs();
		FillMindMaps();
		FillMindMapLinks();
		FillMindMapNodes();
		FillMindMapNodeContents();
		FillSamples();
		FillQuests();
	}

	[PerformanceLogHook]
	private void LoadLocalizations() {
		var dir = Path.Combine(_vmPath, "Localizations");
		if (!Directory.Exists(dir))
			return;

		foreach (var file in Directory.GetFiles(dir, "*.txt")) {
			var lang = Path.GetFileNameWithoutExtension(file);
			_vm.AddLanguage(lang);

			foreach (var line in File.ReadLines(file)) {
				int idx = line.IndexOf(' ');
				if (idx <= 0) continue;

				ulong id = ulong.Parse(line[..idx]);
				string value = line[(idx + 1)..];

				if (_vm.ElementsById.TryGetValue(id, out var elem) &&
					elem is GameString gs)
					gs.SetText(value, lang);
			}
		}
	}
}