using System.Linq;
using System.Xml.Linq;
using P2XMLEditor.Data;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.Helper;
using static P2XMLEditor.Helper.XmlParsingHelper;

namespace P2XMLEditor.Writing.Element.ReleaseXElementWriters;

public class ReleaseXElementBranchWriter : IReleaseXElementWriter<Branch> {
	public XElement ToXml(Branch element, WriterSettings settings) {
		var xElement = CreateBaseElement(element.Id);
		if (element.BranchConditions.Any())
			xElement.Add(CreateListElementUnsorted("BranchConditions", element.BranchConditions.Select(c => c.Id.ToString())));
		xElement.Add(new XElement("BranchType", element.BranchType.Serialize()));
		
		if (element.BranchVariantInfo?.Count > 0) {
			var variantInfo = new XElement("BranchVariantInfo");
			variantInfo.Add(new XAttribute("count", element.BranchVariantInfo.Count));
			variantInfo.Add(element.BranchVariantInfo.Select(info => 
				new XElement("Item",
					new XElement("Name", info.Name),
					new XElement("Type", info.Type)
				)
			));
			xElement.Add(variantInfo);
		}
		xElement.Add(CreateListElement("EntryPoints", element.EntryPoints.Select(e => e.Id.ToString())));
		if (!settings.RemoveDefaultValueTypes || element.IgnoreBlock)
			xElement.Add(CreateBoolElement("IgnoreBlock", element.IgnoreBlock));
		xElement.Add(new XElement("Owner", element.Owner.Id));
		if (element.InputLinks?.Any() == true)
			xElement.Add(CreateListElement("InputLinks", element.InputLinks.Select(l => l.Id.ToString())));
		if (element.OutputLinks?.Any() == true)
			xElement.Add(CreateListElement("OutputLinks", element.OutputLinks.Select(l => l.Id.ToString())));
		if (!settings.RemoveDefaultValueTypes || element.Initial)
			xElement.Add(CreateBoolElement("Initial", element.Initial));
		if (!settings.StripNames)
			xElement.Add(new XElement("Name", element.Name));
		xElement.Add(new XElement("Parent", element.Parent.Id));
		return EnsureFullClosingTag(xElement);
	}
}
