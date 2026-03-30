using System;
using System.Xml.Linq;
using P2XMLEditor.GameData.Templates.Abstract;

namespace P2XMLEditor.GameData.Templates;

public class WeatherSnapshot : TemplateObject {
	private XElement? _rawElement;

	public override void LoadFromXml(XElement element) {
		base.LoadFromXml(element);
		_rawElement = new XElement(element);
	}

	public override XElement ToXml() {
		return _rawElement ?? base.ToXml();
	}
}
