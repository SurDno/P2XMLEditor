using System.Xml.Linq;
using P2XMLEditor.GameData.Enums;
using P2XMLEditor.GameData.Templates.InternalTypes.Abstract;
using P2XMLEditor.Helper;
using static P2XMLEditor.Helper.XmlParsingHelper;

namespace P2XMLEditor.GameData.Templates.InternalTypes.Components;

public struct InteractableComponent() : ITemplateComponent, IEnableable {
	public struct InteractItem {
		public InteractType Type;
		public string? BlueprintId;
		public GameActionType Action;
		public string Title;
	}

	public bool IsEnabled { get; set; }
	public List<InteractItem> Items { get; } = [];

	public void LoadFromXml(XElement element) {
		IsEnabled = ParseBool(element.Element("IsEnabled")!);

		var itemsEl = element.Element("Items");
		if (itemsEl == null) return;
		foreach (var item in itemsEl.Elements("Item")) {
			var bpId = item.Element("Blueprint")!.Element("Id")!.Value;
			Items.Add(new InteractItem {
				Type = item.Element("Type")!.Value.Deserialize<InteractType>(),
				BlueprintId =  string.IsNullOrEmpty(bpId) ? null : bpId,
				Action = item.Element("Action")!.Value.Deserialize<GameActionType>(),
				Title = item.Element("Title")!.Value
			});
		}
	}

	public XElement ToXml(XElement baseElement) {
		baseElement.Add(CreateBoolElement("IsEnabled", IsEnabled));

		var itemsEl = new XElement("Items");
		foreach (var it in Items) {
			var e = new XElement("Item");
			e.Add(new XElement("Type", it.Type.Serialize()));
			e.Add(new XElement("Blueprint", new XElement("Id", it.BlueprintId ?? string.Empty)));
			e.Add(new XElement("Action", it.Action.Serialize()));
			e.Add(new XElement("Title", it.Title));
			itemsEl.Add(e);
		}

		baseElement.Add(itemsEl);
		return baseElement;
	}
}