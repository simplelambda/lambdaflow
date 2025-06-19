using System;
using System.IO;
using System.IO.Compression;
using System.Formats.Asn1;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text.Json;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace LambdaFlow {
    public static class Program {

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool AllocConsole();

        private static Config LoadConfig() {
            var asm = Assembly.GetExecutingAssembly();
            var resName = "lambdaflow.config.json";
            using var stream = asm.GetManifestResourceStream(resName) ?? throw new FileNotFoundException($"Resource '{resName}' not found.");

            Config cfg;
            try {
                cfg = JsonSerializer.Deserialize<Config>(stream) ?? throw new Exception("Embedded config.json malformed.");
                return cfg;
            }
            catch (Exception ex) {
                Console.Error.WriteLine($"Error reading embedded config: {ex.Message}");
                return null;
            }
        }

        [STAThread]
        static void Main(string[] args) {

            // Check parameters

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && args.Contains("--console", StringComparer.OrdinalIgnoreCase)) AllocConsole();


            // Load config from config.json

            Config cfg = LoadConfig();


            // Lock frontend.pak to avoid TOCTOU on frontend

            var frontendPath = Path.Combine(AppContext.BaseDirectory, "frontend.pak");
            var frontPakLock = SecurityManager.LockFile(frontendPath);


            // Verify signed integrity manifest

            SecurityManager.VerifyIntegrity(cfg);


            // Create temporary directory for backend files

            var tempRoot = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempRoot);

            Console.WriteLine($"Temporary directory created: {tempRoot}");

            SecurityManager.DenyDeleteOnDirectory(tempRoot);

            //Extract backend

            try {
                var backPak = Path.Combine(AppContext.BaseDirectory, "backend.pak");
                var tmpBackPak = Path.Combine(tempRoot, Path.GetFileName(backPak));

                File.Copy(backPak, tmpBackPak);
                ZipFile.ExtractToDirectory(tmpBackPak, tempRoot);

                SecurityManager.DenyWriteOnDirectory(tempRoot);

                // Lock exe

                var exePath = Path.Combine(tempRoot, "Backend" + (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? ".exe" : ""));
                SecurityManager.LockFile(exePath);

                // Create IPCBridge and WebViewHost

                var bridge = new IPCBridge(exePath, args);

                var process = bridge.Process;
                SecurityManager.UnlockFile(exePath);

                // Obtain the frontend initial HTML path

                var host = new WebViewHost(cfg.Window, bridge, frontPakLock, cfg.FrontendInitialHTML);


                // Run the frontend

                host.Run();

                SecurityManager.UnlockFile(frontendPath);
                bridge.Dispose();
            }
            finally {
                if (frontPakLock != null) SecurityManager.UnlockFile(frontendPath);
                SecurityManager.RestoreWriteOnDirectory(tempRoot);
                SecurityManager.RestoreDeleteOnDirectory(tempRoot);

                // Clear the temporary directory

                try {
                    Directory.Delete(tempRoot, true);
                }
                catch (Exception ex) {
                    Console.Error.WriteLine($"[Error] Could not delete temporary directory '{tempRoot}': {ex.Message}");
                }
            }
        }
    }
}
