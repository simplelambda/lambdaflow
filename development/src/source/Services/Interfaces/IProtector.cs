using System.IO;

namespace LambdaFlow {
    internal interface IProtector {
        void Protect(string path, ProtectionOptions options);
        FileStream LockFile(string path);
        void UnlockFile(FileStream stream);
    }
}
