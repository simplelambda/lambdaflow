using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Diagnostics;

namespace LambdaFlow {
    internal class LinuxSigner : ISigner{
        #region Variables

            private const byte[] ExpectedPublicKeyToken = new { };
            private const bool useAuthenticode = false;
            private const string CosignPublicKeyPem = @"";

        #endregion

        #region Internal methods

            internal bool Verify(){
                var exePath = Assembly.GetEntryAssembly()!.Location;

                return (VerifyStrongName(exePath) && VerifyCosignSignature(exePath));
            }

        #endregion

        #region Private methods

            private bool VerifyStrongName(string path) {
                try {
                    var asmName = AssemblyName.GetAssemblyName(path);
                    var token = asmName.GetPublicKeyToken();
                    return token != null && token.SequenceEqual(ExpectedPublicKeyToken);
                }
                catch {
                    return false;
                }
            }

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
                rsa.ImportFromPem(CosignPublicKeyPem.ToCharArray());

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
