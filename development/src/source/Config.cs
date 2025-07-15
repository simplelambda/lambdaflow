using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.IO;

namespace LambdaFlow {
    internal class WindowConfig {
        [JsonPropertyName("title")] public string Title { get; set; } = "lambdaFlowApp";
        [JsonPropertyName("width")] public int Width { get; set; } = 800;
        [JsonPropertyName("height")] public int Height { get; set; } = 600;
        [JsonPropertyName("minWidth")] public int MinWidth { get; set; } = 0;
        [JsonPropertyName("minHeight")] public int MinHeight { get; set; } = 0;
        [JsonPropertyName("maxWidth")] public int MaxWidth { get; set; } = 0;
        [JsonPropertyName("maxHeight")] public int MaxHeight { get; set; } = 0;
    }

    internal class Config {
        #region Variables

            [JsonPropertyName("appName")] public string AppName { get; set; } = "lambdaFlowApp";
            [JsonPropertyName("appVersion")] public string AppVersion { get; set; } = "1.0.0";
            [JsonPropertyName("organizationName")] public string OrgName { get; set; } = "1.0.0";
            [JsonPropertyName("window")] public WindowConfig Window { get; set; } = new WindowConfig();
            [JsonPropertyName("frontendInitialHTML")] public string FrontendInitialHTML { get; set; } = "index.html";

            [JsonPropertyName("debugMode")] public bool DebugMode { get; set; } = false;
            [JsonPropertyName("securityMode")] public SecurityMode SecurityMode { get; set; } = SecurityMode.INTEGRITY;

        #endregion

        #region Internal methods

            internal static Config CreateConfig(Stream sream) {
                Config cfg;
                try {
                    cfg = JsonSerializer.Deserialize<Config>(stream) ?? throw new Exception("Embedded config.json malformed.");
                    return cfg
                }
                catch (Exception ex) {
                    Console.Error.WriteLine($"Error reading embedded config: {ex.Message}");
                    return null;
                }
            }
    
        #endregion
    }
}