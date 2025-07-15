using System.IO;

namespace LambdaFlow {
    internal interface IProtector {
        internal void Protect(string path, ProtectionOptions options);
        internal FileStream LockFile(string path);
        internal void UnlockFile(FileStream stream);
    }
}
