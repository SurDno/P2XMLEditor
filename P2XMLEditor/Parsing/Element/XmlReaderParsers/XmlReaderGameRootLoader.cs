using System.Collections.Generic;
using System.Xml;
using P2XMLEditor.GameData.VirtualMachineElements.Enums;
using P2XMLEditor.Helper;
using P2XMLEditor.Logging;
using P2XMLEditor.Parsing.RawData;
using static P2XMLEditor.Parsing.Helpers.XmlReaderExtensions;

namespace P2XMLEditor.Parsing.Element.XmlReaderParsers;

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

	private static Dictionary<ulong, Dictionary<ChildContainerType, ulong[]>> ReadHierarchyScenesStructure(XmlReader xr) {
	    xr.Read();

	    var result = new Dictionary<ulong, Dictionary<ChildContainerType, ulong[]>>();

	    while (!xr.EndOfContainerReached()) {

	        var key = ulong.Parse(xr.GetAttribute("key")!);
	        xr.Read();

	        var containers = new Dictionary<ChildContainerType, ulong[]>();

	        while (!xr.EndOfContainerReached()) {

	            var containerType = xr.Name.Deserialize<ChildContainerType>();
	            var count = int.Parse(xr.GetAttribute("count")!);
	            xr.Read();
	            var array = new ulong[count];
	            for (var i = 0; i < count; i++) 
	                array[i] = xr.GetULongValueAndAdvance();
	            xr.Read();
	            containers[containerType] = array;
	        }

	        result[key] = containers;
	        xr.Read();
	    }

	    xr.Read();
	    return result;
	}


	private static List<ulong> ReadContainerList(XmlReader xr) {
		xr.Read();
		var list = new List<ulong>();
		while (!xr.EndOfContainerReached()) list.Add(xr.GetULongValueAndAdvance());
		xr.Read();
		return list;
	}
}
