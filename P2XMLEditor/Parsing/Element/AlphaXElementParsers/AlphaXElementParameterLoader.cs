using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;
using P2XMLEditor.Logging;
using P2XMLEditor.Parsing.Helpers;
using P2XMLEditor.Parsing.RawData;
using static P2XMLEditor.Parsing.Helpers.XElementExtensions;

namespace P2XMLEditor.Parsing.Element.AlphaXElementParsers;

public class AlphaXElementParameterLoader : IParser<RawParameterData> {
	
	[PerformanceLogHook]
	public void ProcessFile(string filePath, List<RawParameterData> raws) {
		using var xr = AlphaFormat.OpenReader(filePath);

		xr.MoveToContent();
		xr.ReadStartElement();
		
		while (xr.NodeType == XmlNodeType.Element) {
			var element = (XElement)XNode.ReadFrom(xr);
			var id = ulong.Parse(element.Element("Guid")!.Value);

			var raw = new RawParameterData();
			raw.Id = id;
			raw.OwnerComponentId = element.Element("OwnerComponent") != null ?
				ulong.Parse(element.Element("OwnerComponent")!.Value) : null;
			raw.Type = element.Element("Type")!.Value;
			if (raw.Type.EndsWith('%'))
				raw.Type = raw.Type[..^1];
			raw.Value = element.Element("Value") != null ? element.Element("Value")!.Value : string.Empty;
			raw.Implicit = element.Element("Implicit")?.Let(ParseBool) ?? false;
			raw.Name = element.Element("Name") != null ? element.Element("Name")!.Value : string.Empty;
			// A handful of alpha constants carry no Parent tag though an expression names them as
			// its Const; that owner is put back in AlphaXElementParsingExecutor once every file is
			// read. Left 0 here rather than dereferenced, which would throw on those few.
			raw.ParentId = element.Element("Parent") != null ? ulong.Parse(element.Element("Parent")!.Value) : 0;
			// The demo has no <Custom> flag — it says the same thing with ParamType. Without this
			// every parameter loaded as standard, so IsCustom answered no for all of them and the
			// editor could not tell a custom parameter from a built-in one.
			raw.Custom = element.Element("ParamType")?.Value == "PARAM_TYPE_CUSTOM";

			raws.Add(raw);
		}
	}
}
