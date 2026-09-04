using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using P2XMLEditor.GameData.Templates;
using P2XMLEditor.GameData.Templates.Abstract;
using P2XMLEditor.Logging;

namespace P2XMLEditor.Core;

public class TemplateManager(string templatesPath) {
	static readonly XName ObjectName = "Object";
	static readonly XName TypeAttr = "type";
	public Dictionary<Guid, TemplateObject> Templates { get; } = new();
	// Diagnostics for the last load, per manager instead of shared across every VM's templates.
	public ConcurrentDictionary<string,int> InvalidTemplates { get; } = new();

	// Built on first use from Templates, so it follows a reload.
	private Dictionary<string, TemplateObject>? _byEngineGuid;

	/// <summary>
	/// The template an engine GUID names, or null. The VM writes those GUIDs without hyphens
	/// ("61afc953e5ef44748c8b3477d4ee52d3") while a template stores its own with them
	/// ("61afc953-e5ef-4474-8c8b-3477d4ee52d3"), so a plain string compare between the two never
	/// matches; both sides are reduced to the hyphenless, lower-case form before lookup.
	/// </summary>
	public TemplateObject? FindByEngineGuid(string? engineGuid) {
		if (string.IsNullOrEmpty(engineGuid)) return null;
		_byEngineGuid ??= BuildEngineGuidIndex();
		return _byEngineGuid.GetValueOrDefault(NormalizeGuid(engineGuid));
	}

	private Dictionary<string, TemplateObject> BuildEngineGuidIndex() {
		var index = new Dictionary<string, TemplateObject>();
		foreach (var template in Templates.Values)
			index[template.Id.ToString("N")] = template; // "N" is the hyphenless, lower-case form
		return index;
	}

	private static string NormalizeGuid(string guid) =>
		Guid.TryParse(guid, out var parsed) ? parsed.ToString("N") : guid.Replace("-", "").ToLowerInvariant();

	[PerformanceLogHook]
	public void LoadTemplates() {
		Templates.Clear();
		InvalidTemplates.Clear();
		_byEngineGuid = null;

		var templateFiles = Directory.GetFiles(templatesPath, "*.xml.gz");
		var localTemplates = new ConcurrentDictionary<Guid, TemplateObject>();

		Parallel.ForEach(templateFiles, file => {
			try {
				using var fileStream = File.OpenRead(file);
				using var gzipStream = new GZipStream(fileStream, CompressionMode.Decompress);
				var document = XDocument.Load(gzipStream, LoadOptions.None);

				foreach (var objElement in document.Root?.Elements(ObjectName) ?? []) {
					var type = objElement.Attribute(TypeAttr)?.Value;
					if (string.IsNullOrEmpty(type)) continue;

					var templateObject = CreateTemplateObject(type);
					if (templateObject != null) {
						templateObject.LoadFromXml(objElement);
						localTemplates[templateObject.Id] = templateObject;
					}
					else {
						if (!InvalidTemplates.TryAdd(type, 1))
							InvalidTemplates[type]++;
					}
				}
			} catch (Exception ex) {
				Logger.Log(LogLevel.Error, $"Error loading template file {file}: {ex.Message}");
			}
		});

		foreach (var kvp in localTemplates)
			Templates[kvp.Key] = kvp.Value;

		foreach (var invalidType in InvalidTemplates)
			Logger.Log(LogLevel.Info, $"Invalid template type {invalidType.Key}: {invalidType.Value}");
		var invalidComponents = new Dictionary<string, int>();
		foreach (var entity in localTemplates.Values.OfType<Entity>())
			foreach (var kvp in entity.invalidComponent)
				invalidComponents[kvp.Key] = invalidComponents.GetValueOrDefault(kvp.Key) + kvp.Value;
		foreach (var invalidType in invalidComponents)
			Logger.Log(LogLevel.Info, $"Invalid entity component type {invalidType.Key}: {invalidType.Value}");

		Logger.Log(LogLevel.Info, $"Loaded {Templates.Count} templates from {templateFiles.Length} files");
	}

	private static TemplateObject? CreateTemplateObject(string type) {
		return type switch {
			nameof(Entity) => new Entity(),
			nameof(MMPlaceholder) => new MMPlaceholder(),
			nameof(SceneObject) => new SceneObject(),
			nameof(WeatherSnapshot) => new WeatherSnapshot(),
			_ => null
		};
	}
}
