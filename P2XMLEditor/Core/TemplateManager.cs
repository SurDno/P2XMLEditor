using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
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
    public static ConcurrentDictionary<string,int> InvalidTemplates = new();

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
        foreach (var invalidType in Entity.invalidComponent) 
            Logger.Log(LogLevel.Info, $"Invalid entity component type {invalidType.Key}: {invalidType.Value}");

        Logger.Log(LogLevel.Info, $"Loaded {Templates.Count} templates from {templateFiles.Length} files");
    }

    private static TemplateObject? CreateTemplateObject(string type) {
        return type switch {
            nameof(Entity) => new Entity(),
            _ => null
        };
    }
}