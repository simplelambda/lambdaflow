using System;
using System.IO;
using System.Runtime.Versioning;
using System.Security.Principal;
using System.Security.AccessControl;

namespace LambdaFlow {
    [SupportedOSPlatform("windows")]
    internal class WindowsProtector : IProtector {

        #region Public methods

            public void Protect(string path, ProtectionOptions options){
                if (options.RequireElevation && !new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator))
                    throw new UnauthorizedAccessException("Administrator privileges are required");

                var fileInfo = new FileInfo(path);
                var sec = fileInfo.GetAccessControl();


                sec.SetAccessRuleProtection(isProtected: true, preserveInheritance: false);
                var user = WindowsIdentity.GetCurrent().User!;

                FileSystemRights rights = 0;
                if (options.AllowRead) rights |= FileSystemRights.Read;
                if (options.AllowWrite) rights |= FileSystemRights.Write;
                if (options.AllowExecute) rights |= FileSystemRights.ExecuteFile;
                if (options.AllowDelete) rights |= FileSystemRights.Delete;

                var rule = new FileSystemAccessRule(
                    user,
                    rights,
                    InheritanceFlags.None,
                    PropagationFlags.NoPropagateInherit,
                    AccessControlType.Allow);

                sec.AddAccessRule(rule);
                fileInfo.SetAccessControl(sec);
            }

            public FileStream LockFile(string path){
                var fileLock = new FileStream(
                    path,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.None
                );

                fileLock.Lock(0, 0);

                return fileLock;
            }

            public void UnlockFile(FileStream stream){
                if (stream == null) throw new ArgumentNullException(nameof(stream));

                stream.Unlock(0, 0);
                stream.Dispose();
            }

        #endregion

        #region Private methods

            private bool IsAdministrator(){
                using var identity = WindowsIdentity.GetCurrent();
                var principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }

        #endregion

    }
}
