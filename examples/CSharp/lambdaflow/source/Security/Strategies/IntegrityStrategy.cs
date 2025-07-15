using System;
using System.IO;
using System.Security;
using System.IO.Compression;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace LambdaFlow {
    internal class IntegrityStrategy : IStrategy {
        #region Variables

            private readonly ISigner _signer = SignerFactory.GetSigner();
            private readonly IProtector _protector = ProtectorFactory.GetProtector();

        #endregion

        #region Properties

            internal Config Config { get; private set; }

        #endregion

        #region Constructors

            internal IntegrityStrategy() { }

        #endregion

        #region Internal methods

            internal void ApplySecurity() {
                if (!_signer.Verify()) throw new SecurityException($"Integrity failure. Executable modified.");


                // Lock frontend.pak and backend.pak to avoid TOCTOU

                var frontendPath = Path.Combine(AppContext.BaseDirectory, "frontend.pak");
                var frontPakLock = _protector.Protect(frontendPath, new ProtectionOptions { AllowRead = true });
                var frontfs = _protector.LockFile(frontendPath);

                var backendPath = Path.Combine(AppContext.BaseDirectory, "backend.pak");
                var backPakLock = _protector.Protect(backendPath, new ProtectionOptions { AllowRead = true });
                var backfs = _protector.LockFile(backendPath);


                // Decrypt integrity.json

                var integrityEncrypted = Utilities.GetEmbeddedResourceString("integrity.json");
                var integrity = Utilities.DecryptJsonFromStream<Dictionary<string, string>>(integrityEncrypted, integrity = true);


                // Verify integrity.json hashes

                using var hasher = SHA512.Create();

                foreach (var kv in integrity) {
                    var rel = kv.Key.Replace('/', Path.DirectorySeparatorChar);
                    var path = Path.Combine(AppContext.BaseDirectory, rel);

                    if (!File.Exists(path)) throw new SecurityException($"Missing file: {rel}");

                    FileStream fs;

                    if(path.Contains("backend.pak"))        fs = backfs;
                    else if (path.Contains("frontend.pak")) fs = frontfs;
                    else                                    fs = new(path, FileMode.Open, FileAccess.Read, FileShare.None);

                    var hash = BitConverter.ToString(hasher.ComputeHash(fs)).Replace("-", "").ToLowerInvariant();
                    if (hash != kv.Value.ToLowerInvariant()) throw new SecurityException($"Hash mismatch: {rel}");
                }

                integrity = null;


                // Decrypt config.json

                var configEncrypted = Utilities.GetEmbeddedResourceString("config.json");
                Config = Utilities.DecryptJsonFromStream<Config>(integrityEncrypted, integrity = false, config = true);


                // Create backend extraction folder

                var backendExtractPath = Path.Combine(AppContext.BaseDirectory, "backend");
                Directory.CreateDirectory(backendExtractPath);


                // Change permissions for backend extraction folder

                _protector.Protect(backendExtractPath, new ProtectionOptions{ AllowRead = true, AllowWrite = true });
                ZipFile.ExtractToDirectory(backendPath, backendExtractPath, overwriteFiles: true);
                _protector.Protect(backendExtractPath, new ProtectionOptions { AllowRead = true});

                // Unprotect backend.pak

                _protector.UnlockFile(backfs);
            }

        #region
    }
}