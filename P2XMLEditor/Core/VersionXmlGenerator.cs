using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using P2XMLEditor.Data;

namespace P2XMLEditor.Core;

public static class VersionXmlGenerator {
	public static void Generate(string path, VmVersionSettings settings, int dataCapacity) {

		var root = new XElement("Root",
			new XElement("DataCapacity", dataCapacity),
			new XElement("ProjectVersion", "1337"),
			new XElement("XmlDataFormatVersion", "14"),
			new XElement("GameDataInfo",
				new XElement("GameName", settings.GameName),
				new XElement("Scene", settings.Scene),
				new XElement("WeatherSnapshot", settings.WeatherSnapshot),
				new XElement("SolarTime", $"{settings.SolarTime.Day}.{settings.SolarTime:HH:mm:ss}"),
				new XElement("SkyRotation", settings.SkyRotation),
				new XElement("LoadingWindowGameDay", settings.LoadingWindowGameDay),
				new XElement("HideLoadingWindow", settings.HideLoadingWindow),
				new XElement("LoadingScreenName", settings.LoadingScreenName)
			)
		);

		var settingsXml = new XmlWriterSettings {
			Indent = true,
			Encoding = new UpperCaseUtf8Encoding(),
			OmitXmlDeclaration = false
		};

		var versionPath = Path.Combine(path, "Version.xml");
		using var writer = XmlWriter.Create(versionPath, settingsXml);
		root.Save(writer);
	}

	private class UpperCaseUtf8Encoding : UTF8Encoding {
		public override string WebName => "UTF-8";
	}
}
