using System;
using System.Linq;
using System.Xml.Linq;
using P2XMLEditor.Data;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.Helper;
using static P2XMLEditor.Helper.XmlParsingHelper;

namespace P2XMLEditor.Writing.Element.ReleaseXElementWriters;

public class ReleaseXElementMindMapNodeWriter : IReleaseXElementWriter<MindMapNode> {
	public XElement ToXml(MindMapNode element, WriterSettings settings) {
		var xElement = CreateBaseElement(element.Id);
		xElement.Add(
			new XElement("LogicMapNodeType", element.LogicMapNodeType.Serialize())
		);
		if (element.Content.Count != 0)
			xElement.Add(CreateListElement("NodeContent", element.Content.Select(c => c.Id.ToString())));
		xElement.Add(
			new XElement("GameScreenPosX", FormatPos(element.GameScreenPosX)),
			new XElement("GameScreenPosY", FormatPos(element.GameScreenPosY))
		);
		if (element.InputLinks.Count != 0)
			xElement.Add(CreateListElement("InputLinks", element.InputLinks.Select(l => l.Id.ToString())));
		if (element.OutputLinks.Count != 0)
			xElement.Add(CreateListElement("OutputLinks", element.OutputLinks.Select(l => l.Id.ToString())));
		xElement.Add(
			new XElement("Name", element.Name),
			new XElement("Parent", element.Parent.Id)
		);
		return xElement;
	}

	private static string FormatPos(float value) {
		const float posStep = 0.0025f;
		var str = (MathF.Round(value / posStep) * posStep).ToString("0.#####");
		return str.Contains('.') ? str.TrimEnd('0').TrimEnd('.') : str;
	}
}
