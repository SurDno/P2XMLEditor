using System.Xml;
using P2XMLEditor.GameData.VirtualMachineElements.Enums;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes;
using P2XMLEditor.Helper;
using P2XMLEditor.Logging;
using P2XMLEditor.Parsing.RawData;
using static P2XMLEditor.Helper.XmlReaderExtensions;

namespace P2XMLEditor.Parsing.ElementParsers.Reader;

public class XmlReaderGameRootLoader : IParser<RawGameRootData> {
	[PerformanceLogHook]
	public void ProcessFile(string filePath, List<RawGameRootData> raws) {
		using var xr = InitializeFullFileReader(filePath);
		xr.SkipDeclarationAndRoot();

		while (xr.Read()) {
			if (xr.EndOfContainerReached()) break;

			var raw = new RawGameRootData {
				Id = xr.GetIdAndEnter(),
				SampleIds = xr.GetULongListAndAdvance(),
				LogicMapIds = xr.GetULongListAndAdvance(),
				GameModeIds = xr.GetULongListAndAdvance(),
				BaseToEngineGuidsTable = xr.GetStringDictAndAdvance(),
				HierarchyScenesStructure = ReadHierarchyScenesStructure(xr),
				HierarchyEngineGuidsTable = xr.GetStringListAndAdvance(),
				WorldObjectSaveOptimizeMode = xr.Name == "WorldObjectSaveOptimizeMode" ? xr.GetBoolValueAndAdvance() : null,
				FunctionalComponentIds = xr.GetULongListAndAdvance(),
				EventGraphId = xr.GetULongValueAndAdvance(),
				ChildObjectIds = xr.GetULongListAndAdvance(),
				EventIds = xr.Name == "Events" ? xr.GetULongListAndAdvance() : null,
				CustomParamIds = xr.GetULongDictAndAdvance(),
				StandartParamIds = xr.GetULongDictAndAdvance(),
				Name = xr.GetStringValueAndAdvance(),
			};

			raws.Add(raw);
		}
	}

	private static Dictionary<ulong, SceneStructureEntry> ReadHierarchyScenesStructure(XmlReader xr) {
		xr.Read();
		var dict = new Dictionary<ulong, SceneStructureEntry>();

		while (!xr.EndOfContainerReached()) {
			var key = ulong.Parse(xr.GetAttribute("key")!);
			xr.Read();

			var entry = new SceneStructureEntry();

			while (!xr.EndOfContainerReached()) {
				var containerType = xr.Name.Deserialize<ChildContainerType>();
				entry.Containers[containerType] = ReadContainerList(xr);
			}

			dict[key] = entry;
			xr.Read();
		}

		xr.Read();
		return dict;
	}

	private static List<ulong> ReadContainerList(XmlReader xr) {
		xr.Read();
		var list = new List<ulong>();
		while (!xr.EndOfContainerReached()) list.Add(xr.GetULongValueAndAdvance());
		xr.Read();
		return list;
	}
}
