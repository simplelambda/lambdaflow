using System;
using System.IO;
using System.Security;
using System.Text.Json;
using System.IO.Compression;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace LambdaFlow {
    internal class RunStrategy : IStrategy {
        #region Public methods

            public void ApplySecurity() {
                var backendPath = Path.Combine(AppContext.BaseDirectory, "backend.pak");

                var backendExtractPath = Path.Combine(AppContext.BaseDirectory, "backend");
                Directory.CreateDirectory(backendExtractPath);

                ZipFile.ExtractToDirectory(backendPath, backendExtractPath, overwriteFiles: true);
            }

        #endregion
    }
}