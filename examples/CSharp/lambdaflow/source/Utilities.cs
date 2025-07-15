using System;
using System.IO;
using System.Text;
using System.Security;
using System.Text.Json;
using System.Reflection;
using System.Security.Cryptography;
using System.Runtime.InteropServices;

namespace LambdaFlow {
    internal static class Utilities {
        #region Variables

            internal readonly static Platform Platform = GetPlatform();

            internal readonly static string signPath = "";

            internal readonly static SecurityMode securityMode = SecurityMode.INTEGRITY;

            internal readonly static byte[] RandomIntegrityKey = { };
            internal readonly static byte[] RandomConfigKey = { };

        #endregion

        #region Internal methods

            internal static Stream? GetEmbeddedResourceStream(string resourceName) {
                var assembly = Assembly.GetExecutingAssembly();
                var resName = $"lambdaflow.lambdaflow.TMP.{resourceName}";

                return assembly.GetManifestResourceStream(resName) ?? throw new FileNotFoundException($"Resource '{resName}' not found.");
            }

            public static string GetEmbeddedResourceString(string resourceName){
                var assembly = Assembly.GetExecutingAssembly();
                var resName = $"lambdaflow.lambdaflow.TMP.{resourceName}";
                var stream = assembly.GetManifestResourceStream(resName) ?? throw new FileNotFoundException($"Resource '{resName}' not found.");

                if (stream.CanSeek) stream.Seek(0, SeekOrigin.Begin);

                using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, leaveOpen: true);
                return reader.ReadToEnd();
            }

            public static T DecryptJsonFromStream<T>(Stream encryptedStream, bool integrity = true, bool config = false){
                if (encryptedStream == null) throw new ArgumentNullException(nameof(encryptedStream));

                byte[] key = integrity ? RandomIntegrityKey : RandomConfigKey;

                if (key == null || key.Length != 32) throw new ArgumentException("AES-GCM Key must have 32 bytes.", nameof(key));

                byte[] blob;
                using (var ms = new MemoryStream()){
                    encryptedStream.CopyTo(ms);
                    blob = ms.ToArray();
                }

                if (blob.Length < 12 + 16 + 1) throw new InvalidDataException("Blob too short AES-GCM.");

                ReadOnlySpan<byte> nonce = blob.AsSpan(0, 12);
                ReadOnlySpan<byte> ciphertext = blob.AsSpan(12, blob.Length - 12 - 16);
                ReadOnlySpan<byte> tag = blob.AsSpan(blob.Length - 16, 16);

                byte[] plain = new byte[ciphertext.Length];

                try{
                    using var aes = new AesGcm(key, 16);
                    aes.Decrypt(nonce, ciphertext, tag, plain);
                }
                catch (CryptographicException ex){
                    throw new SecurityException("AES-GCM decryption failed.", ex);
                }

                CryptographicOperations.ZeroMemory(blob);

                T result = JsonSerializer.Deserialize<T>(plain) ?? throw new InvalidDataException("No se pudo deserializar el JSON descifrado.");

                CryptographicOperations.ZeroMemory(plain);

                return result;
            }

        #endregion

        #region Private methods

            private static Platform GetPlatform(){
                if (OperatingSystem.IsBrowser())                                   return Platform.WEB;
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))           return Platform.WINDOWS;
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))             return Platform.LINUX;
                if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))               return Platform.MACOS;
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Create("ANDROID"))) return Platform.ANDROID;
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Create("IOS")))     return Platform.IOS;

                return Platform.UNKNOWN;
            }

        #endregion
    }
}