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
using System.Collections;

namespace LambdaFlow {
    internal static class SecurityManager {

        private static Dictionary<string, FileStream> fileLocks = [];

        public static FileStream LockFile(string path) {
            var fileLock = new FileStream(
                path,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read);

            if (!fileLocks.ContainsKey(path)) fileLocks[path] = fileLock;
            else throw new InvalidOperationException($"File '{path}' is already locked.");

            return fileLock;
        }

        public static void UnlockFile(string path) {
            if (fileLocks.TryGetValue(path, out var fileLock)) {
                fileLock.Dispose();
                fileLocks.Remove(path);
            }
            else throw new InvalidOperationException($"File '{path}' is not locked.");
        }

        public static void DenyDeleteOnDirectory(string path) {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return;

            var di = new DirectoryInfo(path);
            var acl = di.GetAccessControl();
            var sid = WindowsIdentity.GetCurrent().User!;

            var rule = new FileSystemAccessRule(
                sid,
                FileSystemRights.Delete
                | FileSystemRights.DeleteSubdirectoriesAndFiles,
                InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
                PropagationFlags.None,
                AccessControlType.Deny);

            acl.AddAccessRule(rule);
            di.SetAccessControl(acl);
        }

        public static void RestoreDeleteOnDirectory(string path) {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return;

            var di = new DirectoryInfo(path);
            var acl = di.GetAccessControl();
            var sid = WindowsIdentity.GetCurrent().User!;

            var rule = new FileSystemAccessRule(
                sid,
                FileSystemRights.Delete
                | FileSystemRights.DeleteSubdirectoriesAndFiles,
                InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
                PropagationFlags.None,
                AccessControlType.Deny);

            acl.RemoveAccessRule(rule);
            di.SetAccessControl(acl);
        }

        public static void DenyWriteOnDirectory(string path) {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                var di = new DirectoryInfo(path);
                var acl = di.GetAccessControl();
                var sid = WindowsIdentity.GetCurrent().User!;
                var rights = FileSystemRights.WriteData
                           | FileSystemRights.CreateFiles
                           | FileSystemRights.CreateDirectories
                           | FileSystemRights.WriteAttributes
                           | FileSystemRights.WriteExtendedAttributes;
                var rule = new FileSystemAccessRule(
                    sid,
                    rights,
                    InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
                    PropagationFlags.None,
                    AccessControlType.Deny);
                acl.AddAccessRule(rule);
                di.SetAccessControl(acl);
            }
            else {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "chmod",
                    Arguments = $"-R a-w \"{path}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true
                })!.WaitForExit();
            }
        }

        public static void RestoreWriteOnDirectory(string path) {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                var di = new DirectoryInfo(path);
                var acl = di.GetAccessControl();
                var sid = WindowsIdentity.GetCurrent().User!;
                var rights = FileSystemRights.WriteData
                           | FileSystemRights.CreateFiles
                           | FileSystemRights.CreateDirectories
                           | FileSystemRights.WriteAttributes
                           | FileSystemRights.WriteExtendedAttributes;
                var rule = new FileSystemAccessRule(
                    sid,
                    rights,
                    InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
                    PropagationFlags.None,
                    AccessControlType.Deny);
                acl.RemoveAccessRule(rule);
                di.SetAccessControl(acl);
            }
            else {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "chmod",
                    Arguments = $"-R u+w \"{path}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true
                })!.WaitForExit();
            }
        }

        public static void VerifyIntegrity(Config cfg) {
            var asm = Assembly.GetExecutingAssembly();

            // Load integrity.json

            using var mStream = asm.GetManifestResourceStream("lambdaflow.integrity.json") ?? throw new FileNotFoundException("integrity.json missing");

            string app = cfg.AppName;
            string org = cfg.OrgName;

            byte[] manifestBytes = ReadAll(mStream);
            string secureDir;

            // Load secure path

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                if (RuntimeInformation.ProcessArchitecture == Architecture.X86) {
                    var pf86 = Environment.GetEnvironmentVariable("ProgramFiles(x86)") ?? Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
                    secureDir = Path.Combine(pf86, org, app);

                    if (!Directory.Exists(secureDir)) throw new DirectoryNotFoundException($"Cannot find secure folder under '{pf86}'");
                }
                else {
                    var pf64 = Environment.GetEnvironmentVariable("ProgramW6432") ?? Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
                    secureDir = Path.Combine(pf64, org, app);

                    if (!Directory.Exists(secureDir)) throw new DirectoryNotFoundException($"Cannot find secure folder under '{pf64}'");
                }
            }
            else {
                secureDir = Path.Combine("/var/lib", org, app);
            }

            // Load integrity.sig

            string sigPath = Path.Combine(secureDir, "integrity.sig");
            if (!File.Exists(sigPath)) throw new FileNotFoundException("integrity.sig missing in secure store", sigPath);

            byte[] signature = File.ReadAllBytes(sigPath);

            // Load public.pem

            Console.WriteLine(Path.Combine(secureDir, "public.pem"));
            string pemPath = Path.Combine(secureDir, "public.pem");
            if (!File.Exists(pemPath)) throw new FileNotFoundException("integrity.sig missing in secure store", pemPath);

            string publicPem = File.ReadAllText(pemPath);

            // Verify Ed25519 signature

            var base64 = publicPem
                .Replace("-----BEGIN PUBLIC KEY-----", "")
                .Replace("-----END PUBLIC KEY-----", "")
                .Replace("\r", "")
                .Replace("\n", "");

            byte[] spki = Convert.FromBase64String(base64);

            using var ecdsa = ECDsa.Create();
            ecdsa.ImportSubjectPublicKeyInfo(spki, out _);

            if (!ecdsa.VerifyData(manifestBytes, signature, HashAlgorithmName.SHA512)) throw new SecurityException("Signature invalid");

            // Load integrity.json content

            var manifest = JsonSerializer.Deserialize<Dictionary<string, string>>(manifestBytes) ?? throw new Exception("Malformed manifest");

            // Verify integrity.json hashes

            using var hasher = SHA512.Create();

            foreach (var kv in manifest) {
                var rel = kv.Key.Replace('/', Path.DirectorySeparatorChar);
                var path = Path.Combine(AppContext.BaseDirectory, rel);

                if (!File.Exists(path)) throw new SecurityException($"Missing file: {rel}");

                using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);

                var hash = BitConverter.ToString(hasher.ComputeHash(fs)).Replace("-", "").ToLowerInvariant();

                if (hash != kv.Value.ToLowerInvariant()) throw new SecurityException($"Hash mismatch: {rel}");
            }
        }

        public static byte[] ReadAll(Stream s) {
            using var ms = new MemoryStream();
            s.CopyTo(ms);
            return ms.ToArray();
        }
    }
}
