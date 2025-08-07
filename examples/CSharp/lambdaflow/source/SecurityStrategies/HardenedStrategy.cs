using System;
using System.IO;
using System.Security;
using System.Text.Json;
using System.IO.Compression;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace LambdaFlow {
    internal class HardenedStrategy : IStrategy {
        #region Variables

            private readonly ISigner _signer;
            private readonly IProtector _protector;

        #endregion

        #region Constructors

            internal HardenedStrategy(ISigner signer, IProtector protector) {
                _signer = signer;
                _protector = protector;
            }

        #endregion

        #region Public methods

            public void ApplySecurity() {

                // Verify executable integrity

                if (!_signer.Verify()) throw new SecurityException($"Integrity failure. Executable modified.");

                // Lock frontend.pak and backend.pak to avoid TOCTOU

                var frontendPath = Path.Combine(AppContext.BaseDirectory, "frontend.pak");
                _protector.Protect(frontendPath, new ProtectionOptions { AllowRead = true, RequireElevation = true });
                var frontfs = _protector.LockFile(frontendPath);
                Utilities.FrontFS = frontfs;

                var backendPath = Path.Combine(AppContext.BaseDirectory, "backend.pak");
                _protector.Protect(backendPath, new ProtectionOptions { AllowRead = true, RequireElevation = true });
                var backfs = _protector.LockFile(backendPath);


                // Obtain injected integrity.json

                var integrity = JsonSerializer.Deserialize<Dictionary<string, string>>(Config.Integrity);


                // Verify integrity.json hashes

                using var hasher = SHA512.Create();

                foreach (var kv in integrity) {
                    var rel = kv.Key.Replace('/', Path.DirectorySeparatorChar);
                    var path = Path.Combine(AppContext.BaseDirectory, rel);

                    if (!File.Exists(path)) throw new SecurityException($"Missing file: {rel}");

                    FileStream fs;

                    if (path.Contains("backend.pak")) fs = backfs;
                    else if (path.Contains("frontend.pak")) fs = frontfs;
                    else fs = new(path, FileMode.Open, FileAccess.Read, FileShare.None);

                    var hash = BitConverter.ToString(hasher.ComputeHash(fs)).Replace("-", "").ToLowerInvariant();
                    if (hash != kv.Value.ToLowerInvariant()) throw new SecurityException($"Hash mismatch: {rel}");
                }

                integrity = null;


                // Create backend extraction folder

                var backendExtractPath = Path.Combine(AppContext.BaseDirectory, "backend");
                Directory.CreateDirectory(backendExtractPath);


                // Change permissions for backend extraction folder

                _protector.Protect(backendExtractPath, new ProtectionOptions { AllowRead = true, AllowWrite = true, RequireElevation = true });
                _protector.UnlockFile(backfs);
                ZipFile.ExtractToDirectory(backendPath, backendExtractPath, overwriteFiles: true);
                _protector.Protect(backendExtractPath, new ProtectionOptions { AllowRead = true, RequireElevation = true });
            }

        #endregion
    }
}