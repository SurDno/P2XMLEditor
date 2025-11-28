using System.Collections.Generic;
using System.IO;
using P2XMLEditor.Parsing.RawData;
using static P2XMLEditor.Helper.XmlReaderExtensions;

namespace P2XMLEditor.Parsing.Element.XmlReaderParsers;

public class XmlReaderCustomTypeLoader : IParser<RawCustomTypeData> {
	public void ProcessFile(string filePath, List<RawCustomTypeData> raws) {
		if (!File.Exists(filePath)) return;
		using var xr = InitializeFullFileReader(filePath);
		xr.SkipDeclarationAndRoot();

		while (xr.Read()) {
			if (xr.EndOfContainerReached()) break;

			var raw = new RawCustomTypeData {
				Id = xr.GetIdAndEnter(),
				Name = xr.GetStringValueAndAdvance(),
				ParentId = xr.GetULongValueAndAdvance()
			};

			raws.Add(raw);
		}
	}
}