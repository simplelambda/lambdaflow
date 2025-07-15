using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;

namespace LambdaFlow {
    internal class WindowsSigner : ISigner{
        #region Imports

            [DllImport("wintrust.dll", ExactSpelling = true, CharSet = CharSet.Unicode)]
            private static extern uint WinVerifyTrust(
               IntPtr hwnd,
               [MarshalAs(UnmanagedType.LPStruct)] Guid pgActionID,
               WinTrustData pWVTData
            );

        #endregion

        #region Variables

            private const byte[] ExpectedPublicKeyToken = new { };
            private const bool useAuthenticode = false;
            private const string CosignPublicKeyPem = @"";

        #endregion

        #region Internal methods

            internal bool Verify() {
                var exePath = Assembly.GetEntryAssembly()!.Location;

                return (VerifyStrongName(exePath) && VerifyCosignSignature(exePath) && (!useAuthenticode || VerifyAuthenticode(exePath)));
            }

        #endregion

        #region Private methods

            private bool VerifyAuthenticode(string path){
                Guid action = new Guid("00AAC56B-CD44-11d0-8CC2-00C04FC295EE");

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
                return result == 0;
            }

            private bool VerifyStrongName(string path) {
                try {
                    var asmName = AssemblyName.GetAssemblyName(path);
                    var token = asmName.GetPublicKeyToken();
                    return token != null && token.SequenceEqual(ExpectedPublicKeyToken);
                }
                catch{
                    return false;
                }
            }

            private bool VerifyCosignSignature(string path) {
                byte[] signature;
                byte[] data;

                try
                {
                    using Stream sigStream = Utilities.GetEmbeddedResourceStream("cosign.sig");
                    using var ms = new MemoryStream();
                    sigStream.CopyTo(ms);
                    signature = ms.ToArray();

                    data = File.ReadAllBytes(path);
                }
                catch{
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

        #region Private enums

            private enum WinTrustDataUI : uint {
                All = 1,
                None = 2,
                NoBad = 3,
                NoGood = 4
            }

            private enum WinTrustDataChoice : uint {
                File = 1,
                Catalog = 2,
                Blob = 3,
                Signer = 4,
                Certificate = 5
            }

            private enum WinTrustDataStateAction : uint {
                Ignore = 0x00000000,
                Verify = 0x00000001,
                Close = 0x00000002,
                AutoCache = 0x00000003,
                AutoCacheFlush = 0x00000004
            }

        #endregion

        #region Private classes

            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
            private class WINTRUST_FILE_INFO{
                public uint StructSize = (uint)Marshal.SizeOf<WINTRUST_FILE_INFO>();
                public IntPtr pszFilePath;
                public IntPtr hFile = IntPtr.Zero;
                public IntPtr pgKnownSubject = IntPtr.Zero;
                public WINTRUST_FILE_INFO(string filePath, IntPtr hFile, IntPtr pgKnownSubject, IntPtr reserved) {
                    pszFilePath = Marshal.StringToCoTaskMemAuto(filePath);
                    this.hFile = hFile;
                    this.pgKnownSubject = pgKnownSubject;
                }
            }

            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
            private class WinTrustData : IDisposable {
                public uint StructSize = (uint)Marshal.SizeOf<WinTrustData>();
                public IntPtr PolicyCallbackData = IntPtr.Zero;
                public IntPtr SIPClientData = IntPtr.Zero;
                public WinTrustDataUI UIChoice = WinTrustDataUI.None;
                public WinTrustDataChoice UnionChoice;
                public IntPtr FileInfoPtr = IntPtr.Zero;
                public WinTrustDataStateAction StateAction;

                public WinTrustData(WINTRUST_FILE_INFO fileInfo) {
                    UnionChoice = WinTrustDataChoice.File;
                    StateAction = WinTrustDataStateAction.Verify;

                    FileInfoPtr = Marshal.AllocCoTaskMem(
                        (int)Marshal.SizeOf<WINTRUST_FILE_INFO>());
                    Marshal.StructureToPtr(
                        fileInfo, FileInfoPtr, false);
                }

                public void Dispose() {
                    if (FileInfoPtr != IntPtr.Zero) {
                        Marshal.FreeCoTaskMem(FileInfoPtr);
                        FileInfoPtr = IntPtr.Zero;
                    }
                    if (pszFilePath != IntPtr.Zero) {
                        Marshal.FreeCoTaskMem(pszFilePath);
                        pszFilePath = IntPtr.Zero;
                    }
                }
            }

        #endregion
    }
}
