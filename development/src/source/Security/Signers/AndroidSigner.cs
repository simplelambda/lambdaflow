using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Reflection;
using System.Runtime.Versioning;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

using Android.App;
using Android.Content.PM;
using Android.Runtime;

namespace LambdaFlow {
    [SupportedOSPlatform("android")]
    internal class AndroidSigner : ISigner{
        #region Variables

            private const string PublicKeyPem = @"";
            private const string ExpectedApkCertSha256 = "";

        #endregion

        #region Public methods

            public bool Verify(){
                var exePath = GetSelfExecutablePath();

                return (VerifyCosignSignature(exePath) && VerifyAndroidApkSignature());
            }

        #endregion

        #region Private methods

            private bool VerifyCosignSignature(string path){
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

            private bool VerifyAndroidApkSignature(){
                try{
                    var ctx = Application.Context;
                    var pkgInfo = ctx.PackageManager.GetPackageInfo(ctx.PackageName, PackageInfoFlags.Signatures);

                    var raw = pkgInfo.Signatures[0].ToByteArray();
                    var cert = new X509Certificate2(raw);

                    string actual = BitConverter.ToString(cert.GetCertHash(HashAlgorithmName.SHA256)).Replace("-", "").ToUpperInvariant();

                    return normalized == ExpectedApkCertSha256;
                }
                catch{
                    return false;
                }
            }

            private string GetSelfExecutablePath() {
                var buffer = new byte[1024];
                int len = readlink("/proc/self/exe", buffer, buffer.Length);
                if (len <= 0) throw new InvalidOperationException("No pude resolver /proc/self/exe");
                return System.Text.Encoding.UTF8.GetString(buffer, 0, len);
            }

        #endregion
    }
}
