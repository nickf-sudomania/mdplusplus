using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using MDPlus.Core;

namespace MDPlus.Models
{
    public class AppSettings
    {
        private static readonly string SettingsFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "MDPlus");

        private static readonly string SettingsFile = Path.Combine(SettingsFolder, "settings.json");

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ThemePreset Theme { get; set; } = ThemePreset.GitHubDark;

        public bool ShowMenuBar { get; set; } = false;
        public bool ShowToc { get; set; } = true;
        public bool ShowLineNumbers { get; set; } = true;
        public bool AutoReload { get; set; } = true;
        public bool WordWrap { get; set; } = true;
        public double WindowWidth { get; set; } = 1100;
        public double WindowHeight { get; set; } = 750;
        public bool WindowMaximized { get; set; } = false;
        public List<string> RecentFiles { get; set; } = new List<string>();

        public void AddRecentFile(string path)
        {
            if (string.IsNullOrEmpty(path)) return;
            string full = Path.GetFullPath(path);

            RecentFiles.RemoveAll(p => p.Equals(full, StringComparison.OrdinalIgnoreCase));
            RecentFiles.Insert(0, full);

            if (RecentFiles.Count > 15)
            {
                RecentFiles.RemoveRange(15, RecentFiles.Count - 15);
            }
        }

        public static AppSettings Load()
        {
            try
            {
                if (File.Exists(SettingsFile))
                {
                    string json = File.ReadAllText(SettingsFile);
                    var settings = JsonSerializer.Deserialize<AppSettings>(json);
                    if (settings != null) return settings;
                }
            }
            catch
            {
                // Fallback to defaults
            }
            return new AppSettings();
        }

        public void Save()
        {
            try
            {
                if (!Directory.Exists(SettingsFolder))
                {
                    Directory.CreateDirectory(SettingsFolder);
                }
                string json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
                string tempFile = SettingsFile + ".tmp";
                File.WriteAllText(tempFile, json);
                File.Move(tempFile, SettingsFile, overwrite: true);
            }
            catch
            {
                // Ignore save errors
            }
        }
    }
}
