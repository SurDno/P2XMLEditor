using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using P2XMLEditor.Data;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.GameData.VirtualMachineElements.Enums;
using static P2XMLEditor.Helper.DemoXmlParsingHelper;

namespace P2XMLEditor.Writing.Element.AlphaXElementWriters;

/// <summary>
/// The inverse of <see cref="Parsing.Element.AlphaXElementParsers.AlphaXElementGameRootLoader"/>:
/// the two fields the alpha format packs into single strings are written back into those strings
/// rather than the &lt;.Dict&gt; forms the demo uses. The scene structure loses the interleaving of
/// direct and mounted children (the model keeps them in separate buckets), which the engine does
/// not depend on; everything else round-trips.
/// </summary>
public class AlphaXElementGameRootWriter : IAlphaXElementWriter<GameRoot> {
	private const string SceneSeparator = "&SCENE&INFO&";
	private const string MountPrefix = "Mounting_";

	public XElement ToXml(GameRoot element, WriterSettings settings) {
		var obj = new XElement("object");

		obj.Add(CreateDemoListElementAsLong("FunctionalComponents", element.FunctionalComponents.Select(c => c.Id)));

		if (element.EventGraph != null)
			obj.Add(new XElement("EventGraph", element.EventGraph.Id));

		obj.Add(CreateDemoDictAsLong("StandartParams", element.StandartParams?.ToDictionary(k => k.Key, v => v.Value.Id)));
		obj.Add(CreateDemoDictAsLong("CustomParams", element.CustomParams?.ToDictionary(k => k.Key, v => v.Value.Id)));
		obj.Add(CreateDemoStringElement("Name", element.Name));
		obj.Add(CreateDemoListElementAsLong("Events", element.Events?.Select(e => e.Id) ?? []));
		obj.Add(CreateDemoListElementAsLong("ChildObjects", element.ChildObjects?.Select(c => c.Id) ?? []));
		obj.Add(CreateDemoListElementAsLong("Samples", element.Samples?.Select(s => s.Id) ?? []));
		obj.Add(CreateDemoListElementAsLong("LogicMaps", element.LogicMaps?.Select(l => l.Id) ?? []));
		obj.Add(CreateDemoListElementAsLong("GameModes", element.GameModes?.Select(g => g.Id) ?? []));

		// A list of "<baseId>_<engineGuid>" rather than a keyed dictionary.
		var pairs = element.BaseToEngineGuidsTable?.Select(kvp => $"{kvp.Key}_{kvp.Value}");
		obj.Add(CreateDemoListElement("BaseToEngineGuidsTable", pairs ?? []));

		obj.Add(WriteScenesStructure(element.HierarchyScenesStructure));

		obj.Add(CreateDemoListElement("HierarchyEngineGuidsTable", element.HierarchyEngineGuidsTable ?? []));

		if (!settings.RemoveDefaultValueTypes || element.WorldObjectSaveOptimizeMode)
			obj.Add(CreateDemoBoolElement("WorldObjectSaveOptimizeMode", element.WorldObjectSaveOptimizeMode));

		obj.Add(CreateGuidElement(element.Id));
		return obj;
	}

	private static XElement WriteScenesStructure(Dictionary<ulong, Dictionary<ChildContainerType, ulong[]>>? structure) {
		if (structure == null || structure.Count == 0)
			return new XElement("HierarchyScenesStructure");

		var entries = structure.Select(scene => {
			var tokens = new List<string>();
			foreach (var (kind, ids) in scene.Value)
				foreach (var id in ids)
					// Only a mounted sub-scene carries the prefix; a direct child is written bare.
					tokens.Add(kind == ChildContainerType.Scenes ? MountPrefix + id : id.ToString());
			return $"{scene.Key}:{string.Join(",", tokens)}";
		});

		return new XElement("HierarchyScenesStructure", string.Join(SceneSeparator, entries));
	}

	private static XElement CreateDemoDictAsLong(string name, Dictionary<string, ulong>? items) {
		var dictName = name + ".Dict";
		if (items == null || !items.Any()) return new XElement(dictName);
		return new XElement(dictName, items.Select(x => new XElement(x.Key, x.Value)));
	}
}
