using System;
using System.IO;
using System.Runtime.Versioning;
using System.Runtime.InteropServices;

namespace LambdaFlow {
    [SupportedOSPlatform("android")]
    internal class AndroidProtector : IProtector {
        #region Imports

            [DllImport("libc", SetLastError = true)]
            static extern int chmod(string path, ushort mode);

        #endregion

        #region Public methods

            public void Protect(string path, ProtectionOptions options) {
                if (!File.Exists(path)) throw new FileNotFoundException(path);

                uint newMode = 0;

                if (options.AllowRead) newMode |= 0b100_100_100;
                if (options.AllowWrite) newMode |= 0b010_010_010;
                if (options.AllowExecute) newMode |= 0b001_001_001;

                if (chmod(path, newMode) != 0) throw new IOException($"chmod failed: {Marshal.GetLastWin32Error()}");
            }

            public FileStream LockFile(string path) {
                var fileLock = new FileStream(
                    path,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.None
                );

                fileLock.Lock(0, 0);

                return fileLock;
            }

            public void UnlockFile(FileStream stream) {
                if (stream == null) throw new ArgumentNullException(nameof(stream));

                stream.Unlock(0, 0);
                stream.Dispose();
            }

        #endregion

        #region Private methods

            private void ChmodRecursive(string dir, Stat.st_mode_t mode) {
                foreach (var f in Directory.EnumerateFiles(dir)) Syscall.chmod(f, mode).ThrowIfError();

                foreach (var d in Directory.EnumerateDirectories(dir)) {
                    Syscall.chmod(d, mode).ThrowIfError();
                    ChmodRecursive(d, mode);
                }
            }

        #endregion
    }
}
