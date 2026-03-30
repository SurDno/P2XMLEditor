using System;
using System.Linq;
using System.Xml.Linq;
using P2XMLEditor.Data;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using static P2XMLEditor.Helper.XmlParsingHelper;

namespace P2XMLEditor.Writing.Element.ReleaseXElementWriters;

public class ReleaseXElementParameterHolderWriter<T> : IReleaseXElementWriter<T> where T : ParameterHolder {
	public virtual XElement ToXml(T element, WriterSettings settings) {
		var xElement = CreateBaseElement(element.Id);
		if (element.Static != null)
			xElement.Add(CreateBoolElement("Static", (bool)element.Static));
		if (element.InheritanceInfo?.Any() == true)
			xElement.Add(CreateListElement("InheritanceInfo", element.InheritanceInfo));
		if (element.FunctionalComponents.Any())
			xElement.Add(CreateListElement("FunctionalComponents", element.FunctionalComponents.Select(f => f.Id.ToString())));
		if (element.EventGraph != null)
			xElement.Add(new XElement("EventGraph", element.EventGraph.Id));
		if (element.ChildObjects?.Any() == true)
			xElement.Add(CreateListElement("ChildObjects", element.ChildObjects.Select(c => c.Id.ToString())));
		if (element.Events?.Any() == true)
			xElement.Add(CreateListElement("Events", element.Events.Select(e => e.Id.ToString())));
		if (element.CustomParams.Any())
			xElement.Add(CreateDictionaryElement("CustomParams", element.CustomParams.ToDictionary(kv => kv.Key, kv => kv.Value.Id.ToString())));
		if (element.StandartParams.Any())
			xElement.Add(CreateDictionaryElement("StandartParams", element.StandartParams.ToDictionary(kv => kv.Key, kv => kv.Value.Id.ToString())));
		if (element.GameTimeContext != null)
			xElement.Add(new XElement("GameTimeContext", element.GameTimeContext));
		xElement.Add(new XElement("Name", element.Name));
		
		if (element.Parent != null)
			xElement.Add(new XElement("Parent", element.Parent.Id));
		else if (element is not GameRoot)
			throw new InvalidOperationException($"Parent is missing for {element.GetType().Name} {element.ParamId}");
		
		return xElement;
	}
}

public class ReleaseXElementParameterHolderWriter : ReleaseXElementParameterHolderWriter<ParameterHolder>;
