using System;
using System.Collections.Generic;
using System.Xml.Linq;
using P2XMLEditor.GameData.Templates.Abstract;

namespace P2XMLEditor.GameData.Templates;

public class SceneObject : TemplateObject {
	public List<SceneObjectItem> Items { get; set; } = new();

	public override void LoadFromXml(XElement element) {
		base.LoadFromXml(element);
		Items.Clear();
		var itemsElement = element.Element("Items");
		if (itemsElement != null) {
			foreach (var itemElement in itemsElement.Elements("Item")) {
				var item = new SceneObjectItem();
				item.LoadFromXml(itemElement);
				Items.Add(item);
			}
		}
	}

	public override XElement ToXml() {
		var element = base.ToXml();
		var itemsElement = new XElement("Items");
		foreach (var item in Items) {
			itemsElement.Add(item.ToXml());
		}
		element.Add(itemsElement);
		return element;
	}
}

public class SceneObjectItem {
	public Guid Id { get; set; }
	public string PreserveName { get; set; }
	public Guid OriginId { get; set; }
	public Guid TemplateId { get; set; }
	public List<SceneObjectItem> Items { get; set; } = new();

	public void LoadFromXml(XElement element) {
		Id = new Guid(element.Element("Id")?.Value ?? Guid.Empty.ToString());
		PreserveName = element.Element("PreserveName")?.Value ?? string.Empty;
		
		var originIdStr = element.Element("Origin")?.Element("Id")?.Value;
		OriginId = string.IsNullOrEmpty(originIdStr) ? Guid.Empty : new Guid(originIdStr);
		
		var templateIdStr = element.Element("Template")?.Element("Id")?.Value;
		TemplateId = string.IsNullOrEmpty(templateIdStr) ? Guid.Empty : new Guid(templateIdStr);

		var itemsElement = element.Element("Items");
		if (itemsElement != null) {
			foreach (var subItemElement in itemsElement.Elements("Item")) {
				var subItem = new SceneObjectItem();
				subItem.LoadFromXml(subItemElement);
				Items.Add(subItem);
			}
		}
	}

	public XElement ToXml() {
		var element = new XElement("Item");
		element.Add(new XElement("Id", Id));
		element.Add(new XElement("PreserveName", PreserveName));
		element.Add(new XElement("Origin", new XElement("Id", OriginId == Guid.Empty ? "" : OriginId.ToString())));
		element.Add(new XElement("Template", new XElement("Id", TemplateId == Guid.Empty ? "" : TemplateId.ToString())));
		
		var itemsElement = new XElement("Items");
		foreach (var item in Items) {
			itemsElement.Add(item.ToXml());
		}
		element.Add(itemsElement);
		
		return element;
	}
}
