using System.IO;
using System.Text.Json;
using P2XMLEditor.Forms.PathSelection;

namespace P2XMLEditor.Core;

public class AppSettings {
    public PathSelectionForm.Paths? LastPaths { get; set; }
}

public static class SettingsManager {
    private const string SettingsFile = "settings.json";
    
    public static AppSettings Settings { get; private set; } = new AppSettings();

    public static void Load() {
        if (!File.Exists(SettingsFile)) return;
        try {
            var json = File.ReadAllText(SettingsFile);
            var settings = JsonSerializer.Deserialize<AppSettings>(json);
            if (settings != null) {
                Settings = settings;
            }
        } catch {}
    }

    public static void Save() {
        try {
            var json = JsonSerializer.Serialize(Settings, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(SettingsFile, json);
        } catch {}
    }
}
