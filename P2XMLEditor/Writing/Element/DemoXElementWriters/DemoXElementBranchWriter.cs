using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using P2XMLEditor.Data;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.Helper;
using static P2XMLEditor.Helper.DemoXmlParsingHelper;

namespace P2XMLEditor.Writing.Element.DemoXElementWriters;

public class DemoXElementBranchWriter : IDemoXElementWriter<Branch> {
	public XElement ToXml(Branch element, WriterSettings settings) {
		var obj = CreateDemoBaseElement(element.Id);

		obj.Add(CreateDemoListElementAsLong("BranchConditions", element.BranchConditions.Select(c => c.Id)));
		obj.Add(new XElement("BranchType", element.BranchType.Serialize()));

		if (element.BranchVariantInfo?.Count > 0) {
			var list = new XElement("BranchVariantInfo.List");
			for (var i = 0; i < element.BranchVariantInfo.Count; i++) {
				var info = element.BranchVariantInfo[i];
				var itemObj = new XElement("object", new XAttribute("id", i));
				itemObj.Add(
					new XElement("Name", info.Name),
					new XElement("Type", info.Type),
					CreateGuidElement((ulong)i) // Mini-elements usually have index as Guid in Demo
				);
				list.Add(itemObj);
			}
			obj.Add(list);
		}

		obj.Add(CreateDemoListElementAsLong("EntryPoints", element.EntryPoints.Select(e => e.Id)));

		if (element.IgnoreBlock.HasValue)
			obj.Add(CreateDemoBoolElement("IgnoreBlock", element.IgnoreBlock.Value));

		obj.Add(new XElement("Owner", element.Owner.Id));

		obj.Add(
			CreateDemoListElementAsLong("InputLinks", element.InputLinks?.Select(l => l.Id) ?? []),
			CreateDemoListElementAsLong("OutputLinks", element.OutputLinks?.Select(l => l.Id) ?? [])
		);

		if (element.Initial.HasValue)
			obj.Add(CreateDemoBoolElement("Initial", element.Initial.Value));

		obj.Add(new XElement("Comments.List"));

		obj.Add(
			CreateDemoStringElement("Name", element.Name),
			new XElement("Parent", element.Parent.Id),
			CreateGuidElement(element.Id)
		);

		return obj;
	}
}
