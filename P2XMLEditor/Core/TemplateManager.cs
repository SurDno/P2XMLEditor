using System.Collections.Concurrent;
using P2XMLEditor.GameData.Templates;
using P2XMLEditor.GameData.Templates.Abstract;
using P2XMLEditor.Helper;
using System.IO.Compression;
using System.Text;
using System.Xml.Linq;
using P2XMLEditor.Logging;

namespace P2XMLEditor.Core;

public class TemplateManager(string templatesPath) {
    static readonly XName ObjectName = "Object";
    static readonly XName TypeAttr = "type";
    public Dictionary<Guid, TemplateObject> Templates { get; } = new();

    [PerformanceLogHook]
    public void LoadTemplates() {
        Templates.Clear();

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
                    if (templateObject == null) continue;

                    templateObject.LoadFromXml(objElement);
                    localTemplates[templateObject.Id] = templateObject;
                }
            } catch (Exception ex) {
                Logger.Log(LogLevel.Error, $"Error loading template file {file}: {ex.Message}");
            }
        });

        foreach (var kvp in localTemplates)
            Templates[kvp.Key] = kvp.Value;

        foreach (var invalidType in EntityObject.invalidComponent) 
            Logger.Log(LogLevel.Info, $"Invalid component type {invalidType.Key}: {invalidType.Value}");

        Logger.Log(LogLevel.Info, $"Loaded {Templates.Count} templates from {templateFiles.Length} files");
    }


    private void LoadTemplateFile(string filePath) {
        using var fileStream = new FileStream(filePath, FileMode.Open);
        using var gzipStream = new GZipStream(fileStream, CompressionMode.Decompress);
        using var reader = new StreamReader(gzipStream, Encoding.UTF8);

        var content = reader.ReadToEnd();
        var document = XDocument.Parse(content);

        foreach (var objElement in document.Root?.Elements("Object") ?? []) {
            var type = objElement.Attribute("type")?.Value;
            if (string.IsNullOrEmpty(type))
                continue;

            var templateObject = CreateTemplateObject(type);
            if (templateObject == null)
                continue;

            templateObject.LoadFromXml(objElement);
            Templates[templateObject.Id] = templateObject;
        }
    }

    private static TemplateObject? CreateTemplateObject(string type) {
        return type switch {
            "Entity" => new EntityObject(),
            _ => null
        };
    }
}