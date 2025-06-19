using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace LambdaFlow {
    public class WindowConfig {
        [JsonPropertyName("title")] public string Title { get; set; } = "lambdaFlowApp";
        [JsonPropertyName("width")] public int Width { get; set; } = 800;
        [JsonPropertyName("height")] public int Height { get; set; } = 600;
    }

    public class Config {
        [JsonPropertyName("appName")] public string AppName { get; set; } = "lambdaFlowApp";
        [JsonPropertyName("appVersion")] public string AppVersion { get; set; } = "1.0.0";
        [JsonPropertyName("organizationName")] public string OrgName { get; set; } = "1.0.0";
        [JsonPropertyName("window")] public WindowConfig Window { get; set; } = new WindowConfig();
        [JsonPropertyName("frontendInitialHTML")] public string FrontendInitialHTML { get; set; } = "index.html";
    }
}