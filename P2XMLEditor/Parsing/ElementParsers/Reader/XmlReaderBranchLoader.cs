using System.Xml;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.GameData.VirtualMachineElements.Enums;
using P2XMLEditor.Helper;
using P2XMLEditor.Logging;
using P2XMLEditor.Parsing.RawData;
using static P2XMLEditor.Helper.XmlReaderExtensions;

namespace P2XMLEditor.Parsing.ElementParsers.Reader;

public class XmlReaderBranchLoader : IParser<RawBranchData> {
	[PerformanceLogHook]
	public void ProcessFile(string filePath, List<RawBranchData> raws) {
		using var xr = InitializeFullFileReader(filePath);
		xr.SkipDeclarationAndRoot();

		while (xr.Read()) {
			if (xr.EndOfContainerReached()) break;

			var raw = new RawBranchData {
				Id = xr.GetIdAndEnter(),
				BranchConditionIds = xr.Name == "BranchConditions" ? xr.GetULongListAndAdvance() : null,
				BranchType = xr.GetStringValueAndAdvance().Deserialize<BranchType>(),
				BranchVariantInfo = xr.Name == "BranchVariantInfo" ? ReadBranchVariantInfo(xr) : null,
				EntryPointIds = xr.GetULongListAndAdvance(),
				IgnoreBlock = xr.Name == "IgnoreBlock" ? xr.GetBoolValueAndAdvance() : null,
				OwnerId = xr.GetULongValueAndAdvance(),
				InputLinkIds = xr.Name == "InputLinks" ? xr.GetULongListAndAdvance() : null,
				OutputLinkIds = xr.Name == "OutputLinks" ? xr.GetULongListAndAdvance() : null,
				Initial = xr.Name == "Initial" ? xr.GetBoolValueAndAdvance() : null,
				Name = xr.GetStringValueAndAdvance(),
				ParentId = xr.GetULongValueAndAdvance()
			};

			raws.Add(raw);
		}
	}

	private static List<BranchVariantInfo>? ReadBranchVariantInfo(XmlReader xr) {
		xr.Read();
		if (xr.EndOfContainerReached()) {
			xr.Read();
			return null;
		}

		var list = new List<BranchVariantInfo>();
		while (!xr.EndOfContainerReached()) {
			xr.Read();
			var info = new BranchVariantInfo {
				Name = xr.GetStringValueAndAdvance(),
				Type = xr.GetStringValueAndAdvance()
			};
			list.Add(info);
			xr.Read();
		}

		xr.Read();
		return list;
	}
}