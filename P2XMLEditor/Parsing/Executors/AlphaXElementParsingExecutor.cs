using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using P2XMLEditor.Parsing.Element;
using P2XMLEditor.Parsing.Element.AlphaXElementParsers;
using P2XMLEditor.Parsing.RawData;

namespace P2XMLEditor.Parsing.Executors;

/// <summary>
/// Reads the alpha corpus: one plain-xml file per type, each in its own upper-cased directory
/// (ACTION/Action.xml, GRAPHLINK/GraphLink.xml, …) with GameRoot.xml at the root. The files are
/// the demo shape with the object id moved out of the id attribute into a &lt;Guid&gt; child and a few
/// tags under older names, so the loaders are the demo loaders with those two changes; GameRoot
/// alone needs its own loader — see <see cref="AlphaXElementGameRootLoader"/>.
/// </summary>
public class AlphaXElementParsingExecutor : ParsingExecutor {
	protected override string MindMapLinkFileName => "MindMaLink";

	public override void ExecuteAll(string directory) {
		ExecuteAll(directory, ".xml");
		ReconstructMissingParents();
	}

	/// <summary>
	/// A few alpha elements are stored without the Parent that the model needs, though something
	/// else names them: a constant parameter written with no owner but named by an expression's
	/// Const, an entry point written with no owner but listed by a node. The demo and release
	/// formats always carry the Parent, so their loaders read it outright; here it is put back from
	/// the one thing that does point at these elements, once every file has been read. Elements
	/// nothing points at keep a 0 parent and load as orphans, which is what they are.
	/// </summary>
	private void ReconstructMissingParents() {
		if (Parameters.Count > 0) {
			var constOwner = new Dictionary<ulong, ulong>();
			foreach (var expression in Expressions)
				if (expression.ConstId is { } constId)
					constOwner[constId] = expression.Id;

			var span = CollectionsMarshal.AsSpan(Parameters);
			for (var i = 0; i < span.Length; i++)
				if (span[i].ParentId == 0 && constOwner.TryGetValue(span[i].Id, out var owner))
					span[i].ParentId = owner;
		}

		if (EntryPoints.Count > 0) {
			var pointOwner = new Dictionary<ulong, ulong>();
			foreach (var s in States) AddOwners(pointOwner, s.EntryPointIds, s.Id);
			foreach (var g in Graphs) AddOwners(pointOwner, g.EntryPointIds, g.Id);
			foreach (var b in Branches) AddOwners(pointOwner, b.EntryPointIds, b.Id);
			foreach (var s in Speeches) AddOwners(pointOwner, s.EntryPointIds, s.Id);
			foreach (var t in Talkings) AddOwners(pointOwner, t.EntryPointIds, t.Id);

			var span = CollectionsMarshal.AsSpan(EntryPoints);
			for (var i = 0; i < span.Length; i++)
				if (span[i].ParentId == 0 && pointOwner.TryGetValue(span[i].Id, out var owner))
					span[i].ParentId = owner;
		}
	}

	private static void AddOwners(Dictionary<ulong, ulong> owners, ulong[]? pointIds, ulong nodeId) {
		foreach (var pointId in pointIds ?? [])
			owners.TryAdd(pointId, nodeId);
	}

	/// <summary>
	/// The alpha layout: "&lt;dir&gt;/&lt;TYPE&gt;/&lt;File&gt;.xml". GameRoot sits at the root instead of in a
	/// directory, and two files keep a spelling the upper-casing would not produce — BluePrint and
	/// MindMaLink (the latter arriving here already as MindMaLink via MindMapLinkFileName).
	/// </summary>
	protected override string ResolveFile(string directory, string baseName, string extension) {
		if (baseName == "GameRoot")
			return Path.Combine(directory, baseName + extension);
		var file = baseName == "Blueprint" ? "BluePrint" : baseName;
		return Path.Combine(directory, baseName.ToUpperInvariant(), file + extension);
	}

	protected internal override IParser<RawActionData> ActionLoader => new AlphaXElementActionLoader();
	protected internal override IParser<RawMindMapData> MindMapLoader => new AlphaXElementMindMapLoader();
	protected internal override IParser<RawMindMapNodeData> MindMapNodeLoader => new AlphaXElementMindMapNodeLoader();
	protected internal override IParser<RawActionLineData> ActionLineLoader => new AlphaXElementActionLineLoader();
	protected internal override IParser<RawBlueprintData> BlueprintLoader => new AlphaXElementBlueprintLoader();
	protected internal override IParser<RawBranchData> BranchLoader => new AlphaXElementBranchLoader();
	protected internal override IParser<RawConditionData> ConditionLoader => new AlphaXElementConditionLoader();
	protected internal override IParser<RawCustomTypeData> CustomTypeLoader => new AlphaXElementCustomTypeLoader();
	protected internal override IParser<RawEntryPointData> EntryPointLoader => new AlphaXElementEntryPointLoader();
	protected internal override IParser<RawEventData> EventLoader => new AlphaXElementEventLoader();
	protected internal override IParser<RawExpressionData> ExpressionLoader => new AlphaXElementExpressionLoader();
	protected internal override IParser<RawFunctionalComponentData> FunctionalComponentLoader => new AlphaXElementFunctionalComponentLoader();
	protected internal override IParser<RawGameModeData> GameModeLoader => new AlphaXElementGameModeLoader();

	protected internal override IParser<RawGameObjectData> ItemLoader => new AlphaXElementGameObjectLoader();
	protected internal override IParser<RawGameObjectData> OtherLoader => new AlphaXElementGameObjectLoader();
	protected internal override IParser<RawGameObjectData> SceneLoader => new AlphaXElementGameObjectLoader();
	protected internal override IParser<RawGameObjectData> GeomLoader => new AlphaXElementGameObjectLoader();
	protected internal override IParser<RawGameObjectData> CharacterLoader => new AlphaXElementGameObjectLoader();

	protected internal override IParser<RawGameRootData> GameRootLoader => new AlphaXElementGameRootLoader();
	protected internal override IParser<RawGameStringData> GameStringLoader => new AlphaXElementGameStringLoader();
	protected internal override IParser<RawGraphLinkData> GraphLinkLoader => new AlphaXElementGraphLinkLoader();
	protected internal override IParser<RawGraphData> GraphLoader => new AlphaXElementGraphLoader();
	protected internal override IParser<RawMindMapLinkData> MindMapLinkLoader => new AlphaXElementMindMapLinkLoader();
	protected internal override IParser<RawMindMapNodeContentData> MindMapNodeContentLoader => new AlphaXElementMindMapNodeContentLoader();
	protected internal override IParser<RawParameterData> ParameterLoader => new AlphaXElementParameterLoader();
	protected internal override IParser<RawPartConditionData> PartConditionLoader => new AlphaXElementPartConditionLoader();
	protected internal override IParser<RawQuestData> QuestLoader => new AlphaXElementQuestLoader();
	protected internal override IParser<RawReplyData> ReplyLoader => new AlphaXElementReplyLoader();
	protected internal override IParser<RawSampleData> SampleLoader => new AlphaXElementSampleLoader();
	protected internal override IParser<RawSpeechData> SpeechLoader => new AlphaXElementSpeechLoader();
	protected internal override IParser<RawStateData> StateLoader => new AlphaXElementStateLoader();
	protected internal override IParser<RawTalkingData> TalkingLoader => new AlphaXElementTalkingLoader();
}
