using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using P2XMLEditor.Data;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.Helper;
using static P2XMLEditor.Helper.DemoXmlParsingHelper;

namespace P2XMLEditor.Writing.Element.DemoXElementWriters;

public class DemoXElementSpeechWriter : IDemoXElementWriter<Speech> {
	public XElement ToXml(Speech element, WriterSettings settings) {
		var obj = CreateDemoBaseElement(element.Id);

		obj.Add(CreateDemoListElementAsLong("Replyes", element.Replies.Select(r => r.Id)));
		obj.Add(new XElement("Text", element.Text.Id));
		obj.Add(new XElement("AuthorGuid", element.AuthorGuid.Id));

		if (!settings.RemoveDefaultValueTypes || element.OnlyOnce) obj.Add(CreateDemoBoolElement("OnlyOnce", element.OnlyOnce));

		if (!settings.RemoveDefaultValueTypes || element.IsTrade) obj.Add(CreateDemoBoolElement("IsTrade", element.IsTrade));

		obj.Add(CreateDemoListElementAsLong("EntryPoints", element.EntryPoints.Select(e => e.Id)));

		if (!settings.RemoveDefaultValueTypes || element.IgnoreBlock) obj.Add(CreateDemoBoolElement("IgnoreBlock", element.IgnoreBlock));

		obj.Add(new XElement("Owner", element.Owner.Id));

		obj.Add(
			CreateDemoListElementAsLong("InputLinks", element.InputLinks?.Select(l => l.Id) ?? []),
			CreateDemoListElementAsLong("OutputLinks", element.OutputLinks?.Select(l => l.Id) ?? [])
		);

		if (!settings.RemoveDefaultValueTypes || element.Initial) obj.Add(CreateDemoBoolElement("Initial", element.Initial));

		obj.Add(
			settings.StripNames ? null : CreateDemoStringElement("Name", element.Name),
			new XElement("Parent", element.Parent.Id),
			CreateGuidElement(element.Id)
		);

		return obj;
	}
}
