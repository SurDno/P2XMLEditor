using P2XMLEditor.GameData.Templates;
using P2XMLEditor.GameData.Templates.Abstract;
using P2XMLEditor.Helper;
using System.IO.Compression;
using System.Text;
using System.Xml.Linq;

namespace P2XMLEditor.Core;

public class TemplateManager(string templatesPath) {
    public Dictionary<Guid, TemplateObject> Templates { get; } = new();

    public void LoadTemplates() {
        Templates.Clear();

        var templateFiles = Directory.GetFiles(templatesPath, "*.xml.gz");

        foreach (var file in templateFiles) {
            try {
                LoadTemplateFile(file);
            } catch (Exception ex) {
                Logger.LogError($"Error loading template file {file}: {ex.Message}");
            }
        }

        Logger.LogInfo($"Loaded {Templates.Count} templates from {templateFiles.Length} files");
    }

    private void LoadTemplateFile(string filePath) {
        using var fileStream = new FileStream(filePath, FileMode.Open);
        using var gzipStream = new GZipStream(fileStream, CompressionMode.Decompress);
        using var reader = new StreamReader(gzipStream, Encoding.UTF8);

        var content = reader.ReadToEnd();
        var document = XDocument.Parse(content);

        foreach (var objElement in document.Root?.Elements("Object") ?? Enumerable.Empty<XElement>()) {
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