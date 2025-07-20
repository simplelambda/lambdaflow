using System;
using System.IO;
using System.Text;
using System.Security;
using System.Text.Json;
using System.Reflection;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Runtime.InteropServices;

namespace LambdaFlow {
    internal static class Utilities {
        #region Variables            

            internal readonly static string signPath = "";

            internal readonly static byte[] RandomIntegrityKey = { };

        #endregion

        #region Internal methods

            internal static Stream? GetEmbeddedResourceStream(string resourceName) {
                var assembly = Assembly.GetExecutingAssembly();
                var resName = $"lambdaflow.lambdaflow.TMP.{resourceName}";

                return assembly.GetManifestResourceStream(resName) ?? throw new FileNotFoundException($"Resource '{resName}' not found.");
            }

            internal static string GetEmbeddedResourceString(string resourceName){
                var assembly = Assembly.GetExecutingAssembly();
                var resName = $"lambdaflow.lambdaflow.TMP.{resourceName}";
                var stream = assembly.GetManifestResourceStream(resName) ?? throw new FileNotFoundException($"Resource '{resName}' not found.");

                if (stream.CanSeek) stream.Seek(0, SeekOrigin.Begin);

                using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, leaveOpen: true);
                return reader.ReadToEnd();
            }

            internal static T DecryptJsonFromStream<T>(Stream encryptedStream, bool integrity = true, bool config = false){
                /*if (encryptedStream == null) throw new ArgumentNullException(nameof(encryptedStream));

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

                return result;*/

                return JsonSerializer.Deserialize<T>(encryptedStream) ?? throw new InvalidDataException("Failed to deserialize JSON from stream.");
        }

            internal static string GetMimeType(string path) {
                var ext = Path.GetExtension(path);

                return ext switch{
                    ".html"  => "text/html",
                    ".htm"   => "text/html",
                    ".css"   => "text/css",
                    ".js"    => "application/javascript",
                    ".json"  => "application/json",
                    ".otf"   => "application/vnd.ms-fontobject",
                    ".xml"   => "application/xml",
                    ".png"   => "image/png",
                    ".jpg"   => "image/jpeg",
                    ".jpeg"  => "image/jpeg",
                    ".gif"   => "image/gif",
                    ".svg"   => "image/svg+xml",
                    ".ico"   => "image/x-icon",
                    ".txt"   => "text/plain",
                    ".woff"  => "font/woff",
                    ".woff2" => "font/woff2",
                    ".ttf"   => "font/ttf",
                    ".mp3"   => "audio/mpeg",
                    ".mp4"   => "video/mp4",
                    ".webm"  => "video/webm",
                };
            }

            internal static byte[]? ReadPAK(ZipArchive pak, string relativePath) {
                try {
                    var entry = pak.GetEntry(relativePath.Replace('\\', '/'));

                    if (entry is null) return null;

                    using Stream entryStream = entry.Open();
                    using var ms = new MemoryStream();
                    entryStream.CopyTo(ms);
                    return ms.ToArray();
                }
                catch {
                    return null;
                }
            }

        #endregion


    }
}