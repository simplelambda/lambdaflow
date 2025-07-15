using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Diagnostics;
using System.Runtime.Versioning;

namespace LambdaFlow {
    [SupportedOSPlatform("linux")]
    internal class LinuxSigner : ISigner{
        #region Variables

            private const string PublicKeyPem = @"";

        #endregion

        #region Public methods

            public bool Verify(){
                var exePath = Assembly.GetEntryAssembly()!.Location;

                return VerifyCosignSignature(exePath);
            }

        #endregion

        #region Private methods

            private bool VerifyCosignSignature(string path) {
                byte[] signature;
                byte[] data;

                try {
                    using Stream sigStream = Utilities.GetEmbeddedResourceStream("cosign.sig");
                    using var ms = new MemoryStream();
                    sigStream.CopyTo(ms);
                    signature = ms.ToArray();

                    data = File.ReadAllBytes(path);
                }
                catch {
                    return false;
                }

                byte[] hash = SHA256.HashData(data);

                using var rsa = RSA.Create();
                rsa.ImportFromPem(PublicKeyPem.ToCharArray());

                return rsa.VerifyHash(
                    hash,
                    signature,
                    HashAlgorithmName.SHA256,
                    RSASignaturePadding.Pss
                );
            }

        #endregion
    }
}
