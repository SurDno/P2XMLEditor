using System.Xml;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes;
using P2XMLEditor.Helper;
using P2XMLEditor.Logging;
using P2XMLEditor.Parsing.RawData;
using static P2XMLEditor.Helper.XmlReaderExtensions;

namespace P2XMLEditor.Parsing.Element.XmlReaderParsers;

public class XmlReaderGraphLoader : IParser<RawGraphData> {
	[PerformanceLogHook]
	public void ProcessFile(string filePath, List<RawGraphData> raws) {
		using var xr = InitializeFullFileReader(filePath);
		xr.SkipDeclarationAndRoot();

		while (xr.Read()) {
			if (xr.EndOfContainerReached()) break;

			var raw = new RawGraphData {
				Id = xr.GetIdAndEnter(),
				StateIds = xr.Name == "States" ? xr.GetULongListAndAdvance() : null,
				EventLinkIds = xr.Name == "EventLinks" ? xr.GetULongListAndAdvance() : null,
				SubstituteGraphId = xr.Name == "SubstituteGraph" ? xr.GetULongValueAndAdvance() : null,
				GraphType = xr.GetStringValueAndAdvance(),
				InputParamsInfo = xr.Name == "InputParamsInfo" ? ReadInputParamsInfo(xr) : null,
				EntryPointIds = xr.Name == "EntryPoints" ? xr.GetULongListAndAdvance() : null,
				IgnoreBlock = xr.Name == "IgnoreBlock" ? xr.GetBoolValueAndAdvance() : null,
				OwnerId = xr.GetULongValueAndAdvance(),
				InputLinkIds = xr.Name == "InputLinks" ? xr.GetULongListAndAdvance() : null,
				OutputLinkIds = xr.Name == "OutputLinks" ?  xr.GetULongListAndAdvance() : null,
				Initial = xr.Name == "Initial" ? xr.GetBoolValueAndAdvance() : null,
				Name = xr.GetStringValueAndAdvance(),
				ParentId = xr.GetULongValueAndAdvance()
			};

			raws.Add(raw);
		}
	}

	private static List<GraphParamInfo>? ReadInputParamsInfo(XmlReader xr) {
		xr.Read();
		if (xr.EndOfContainerReached()) {
			xr.Read();
			return null;
		}

		var list = new List<GraphParamInfo>();
		while (!xr.EndOfContainerReached()) {
			xr.Read();
			var name = xr.GetStringValueAndAdvance();
			var type = xr.GetStringValueAndAdvance();
			list.Add(new GraphParamInfo(name, type));
			xr.Read();
		}

		xr.Read();
		return list;
	}
}