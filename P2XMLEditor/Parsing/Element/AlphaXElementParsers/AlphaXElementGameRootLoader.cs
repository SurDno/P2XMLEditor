using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using P2XMLEditor.GameData.VirtualMachineElements.Enums;
using P2XMLEditor.Logging;
using P2XMLEditor.Parsing.Helpers;
using P2XMLEditor.Parsing.RawData;
using static P2XMLEditor.Parsing.Helpers.XElementExtensions;

namespace P2XMLEditor.Parsing.Element.AlphaXElementParsers;

/// <summary>
/// The one type whose alpha shape is not the demo shape with the id moved. Two of its fields are
/// packed into single strings there rather than the &lt;.Dict&gt;/&lt;.List&gt; forms the later formats use:
///
/// * <b>HierarchyScenesStructure</b> is one scalar, scene entries joined by "&amp;SCENE&amp;INFO&amp;", each
///   "&lt;sceneId&gt;:&lt;child&gt;,&lt;child&gt;,…". A child written bare is a direct child of the scene; one
///   written "Mounting_&lt;id&gt;" is a scene mounted into it. The later formats split those two into
///   the Childs and Scenes container lists, which is the mapping used here. (The demo writes the
///   same scalar but its loader reads a &lt;.Dict&gt; that is not there, so the structure is empty on a
///   demo load — alpha is the one place it is actually read.)
/// * <b>BaseToEngineGuidsTable</b> is a &lt;.List&gt; of "&lt;baseId&gt;_&lt;engineGuid&gt;" rather than a keyed
///   dictionary; the first underscore splits the pair.
/// </summary>
public class AlphaXElementGameRootLoader : IParser<RawGameRootData> {
	[PerformanceLogHook]
	public void ProcessFile(string filePath, List<RawGameRootData> raws) {
		using var xr = AlphaFormat.OpenReader(filePath);

		xr.MoveToContent();
		xr.ReadStartElement();

		while (xr.NodeType == XmlNodeType.Element) {
			var element = (XElement)XNode.ReadFrom(xr);
			var id = ulong.Parse(element.Element("Guid")!.Value);

			var raw = new RawGameRootData {
				Id = id,
				FunctionalComponentIds = ParseDemoListAsUlong(element, "FunctionalComponents").ToArray(),
				EventGraphId = element.Element("EventGraph") != null ?
					ulong.Parse(element.Element("EventGraph")!.Value) : null,
				StandartParamIds = ParseDemoDictAsUlong(element, "StandartParams"),
				CustomParamIds = ParseDemoDictAsUlong(element, "CustomParams"),
				Name = element.Element("Name")!.Value,
				EventIds = ParseDemoListAsUlong(element, "Events").ToArray(),
				ChildObjectIds = ParseDemoListAsUlong(element, "ChildObjects").ToArray(),
				SampleIds = ParseDemoListAsUlong(element, "Samples").ToArray(),
				LogicMapIds = ParseDemoListAsUlong(element, "LogicMaps").ToArray(),
				GameModeIds = ParseDemoListAsUlong(element, "GameModes").ToArray(),
				BaseToEngineGuidsTable = ParseBaseToEngineGuids(element),
				HierarchyScenesStructure = ParseScenesStructure(element),
				HierarchyEngineGuidsTable = ParseDemoList(element, "HierarchyEngineGuidsTable").ToArray(),
				WorldObjectSaveOptimizeMode = element.Element("WorldObjectSaveOptimizeMode")?.Let(ParseBool) ?? false
			};

			raws.Add(raw);
		}
	}

	private static Dictionary<string, string> ParseBaseToEngineGuids(XElement element) {
		var table = new Dictionary<string, string>();
		foreach (var value in ParseDemoList(element, "BaseToEngineGuidsTable")) {
			var split = value.IndexOf('_');
			if (split <= 0) continue;
			table[value[..split]] = value[(split + 1)..];
		}
		return table;
	}

	private const string SceneSeparator = "&SCENE&INFO&";
	private const string MountPrefix = "Mounting_";

	private static Dictionary<ulong, Dictionary<ChildContainerType, ulong[]>> ParseScenesStructure(XElement element) {
		var structure = new Dictionary<ulong, Dictionary<ChildContainerType, ulong[]>>();
		var raw = element.Element("HierarchyScenesStructure")?.Value;
		if (string.IsNullOrEmpty(raw)) return structure;

		foreach (var entry in raw.Split(SceneSeparator, StringSplitOptions.RemoveEmptyEntries)) {
			var colon = entry.IndexOf(':');
			if (colon < 0 || !ulong.TryParse(entry[..colon], out var sceneId)) continue;

			var direct = new List<ulong>();
			var mounted = new List<ulong>();
			foreach (var token in entry[(colon + 1)..].Split(',', StringSplitOptions.RemoveEmptyEntries)) {
				if (token.StartsWith(MountPrefix)) {
					if (ulong.TryParse(token[MountPrefix.Length..], out var m)) mounted.Add(m);
				} else if (ulong.TryParse(token, out var d)) {
					direct.Add(d);
				}
			}

			var byKind = new Dictionary<ChildContainerType, ulong[]>();
			if (direct.Count > 0) byKind[ChildContainerType.Childs] = direct.ToArray();
			if (mounted.Count > 0) byKind[ChildContainerType.Scenes] = mounted.ToArray();
			structure[sceneId] = byKind;
		}

		return structure;
	}
}
