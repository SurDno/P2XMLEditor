using System.Collections.Generic;
using System.Xml;
using P2XMLEditor.Logging;
using P2XMLEditor.Parsing.RawData;
using static P2XMLEditor.Parsing.Helpers.XmlReaderExtensions;

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
				GraphType = xr.Name == "GraphType" ? xr.GetStringValueAndAdvance() : "GRAPH_TYPE_EVENTGRAPH",
				InputParamsInfo = xr.Name == "InputParamsInfo" ? ReadInputParamsInfo(xr) : null,
				EntryPointIds = xr.Name == "EntryPoints" ? xr.GetULongListAndAdvance() : null,
				IgnoreBlock = xr.Name == "IgnoreBlock" ? xr.GetBoolValueAndAdvance() : false,
				OwnerId = xr.GetULongValueAndAdvance(),
				InputLinkIds = xr.Name == "InputLinks" ? xr.GetULongListAndAdvance() : null,
				OutputLinkIds = xr.Name == "OutputLinks" ?  xr.GetULongListAndAdvance() : null,
				Initial = xr.Name == "Initial" ? xr.GetBoolValueAndAdvance() : false,
				Name = xr.GetStringValueAndAdvance(),
				ParentId = xr.GetULongValueAndAdvance()
			};

			raws.Add(raw);
		}
	}

	private static (string, string)[] ReadInputParamsInfo(XmlReader xr) {
		var length = int.Parse(xr.GetAttribute("count")!);
		xr.Read();

		var list = new (string, string)[length];
		for (var i = 0; i < length; i++) {
			xr.Read();
			var name = xr.GetStringValueAndAdvance();
			var type = xr.GetStringValueAndAdvance();
			list[i] = new(name, type);
			xr.Read();
		}

		xr.Read();
		return list;
	}
}
