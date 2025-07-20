using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Diagnostics;
using System.Runtime.Versioning;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;

namespace LambdaFlow {
    [SupportedOSPlatform("windows")]
    internal class WindowsSigner : ISigner{
        #region Variables

            private const bool useAuthenticode = false;

        #endregion

        #region Public methods

            public bool Verify() {
                var exePath = Environment.ProcessPath!;
                var dllPath = Assembly.GetEntryAssembly()?.Location;

                return (VerifySignature([exePath, dllPath]) && (!useAuthenticode || VerifyAuthenticode(exePath)));
            }

        #endregion

        #region Private methods

            private bool VerifyAuthenticode(string path){
                /*Guid action = new Guid("00AAC56B-CD44-11d0-8CC2-00C04FC295EE");

                var fileInfo = new WINTRUST_FILE_INFO(
                    path,
                    IntPtr.Zero,
                    IntPtr.Zero,
                    IntPtr.Zero
                );

                using var wtd = new WinTrustData(fileInfo){
                    UnionChoice = WinTrustDataChoice.File,
                    UIChoice = WinTrustDataUI.None,
                    StateAction = WinTrustDataStateAction.Ignore,
                };

                uint result = WinVerifyTrust(IntPtr.Zero, action, wtd);
                return result == 0;*/
                return true;
            }

            private bool VerifySignature(string[] paths) {

                // CHECK EXECUTABLE SIGNATURE

                string sigPath = Path.Combine(Path.GetDirectoryName(paths[0])!, "integrity.sig");
                if (!File.Exists(sigPath)) return false;

                byte[] signature;
                byte[] data;

                try {
                    signature = File.ReadAllBytes(sigPath);

                    using var sha = SHA512.Create();
                    var buffer = new byte[65536];

                    foreach (string f in paths) {
                        using var fs = File.OpenRead(f);
                        int n;
                        while ((n = fs.Read(buffer)) > 0)
                            sha.TransformBlock(buffer, 0, n, null, 0);
                    }

                    sha.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
                    data = sha.Hash!;
                }
                catch {
                    return false;
                }

                var base64 = Config.PublicKeyPem
                    .Replace("-----BEGIN PUBLIC KEY-----", "")
                    .Replace("-----END PUBLIC KEY-----", "")
                    .Replace("\r", "")
                    .Replace("\n", "");

                byte[] spki = Convert.FromBase64String(base64);

                using var ecdsa = ECDsa.Create();
                ecdsa.ImportSubjectPublicKeyInfo(spki, out _);

                return ecdsa.VerifyHash(data, signature);
            }

        #endregion
    }
}
