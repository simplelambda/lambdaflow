using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Text.Json;

namespace LambdaFlow {
    internal static class Program {
        [STAThread]
        static void Main(string[] args) {

            // Load the embedded config.json resource

            var asm = Assembly.GetExecutingAssembly();
            var resName = "lambdaflow.config.json";
            using var stream = asm.GetManifestResourceStream(resName) ?? throw new FileNotFoundException($"Resource '{resName}' not found.");


            // Obtain the Config object from the 

            Config cfg;
            try {
                cfg = JsonSerializer.Deserialize<Config>(stream) ?? throw new Exception("Embedded config.json malformed.");
            }
            catch (Exception ex) {
                Console.Error.WriteLine($"Error reading embedded config: {ex.Message}");
                return;
            }


            // Obtain backend exe path

            var backendRelativePath = Path.Combine(cfg.BackendFolder, "bck.exe");
            var exePath = Path.GetFullPath(backendRelativePath, AppContext.BaseDirectory);

            var bargs = args.Length > 0 ? args[0..] : Array.Empty<string>();


            // Create IPCBridge and WebViewHost

            var bridge = new IPCBridge(exePath, bargs);


            // Obtain the frontend initial HTML path

            var frontendRelativePath = Path.Combine(cfg.FrontendFolder, cfg.FrontendInitialHTML);
            var htmlPath = Path.GetFullPath(frontendRelativePath, AppContext.BaseDirectory);

            var host = new WebViewHost(cfg.Window, bridge, htmlPath);


            // Run the frontend

            host.Run();
        }
    }
}
