using System.Runtime.Versioning;

namespace LambdaFlow {
    [SupportedOSPlatform("linux")]
    internal class LinuxIPCBridge : IIPCBridge {
        #region Events

            public event Func<string, Task>? OnProcessStdOut;

        #endregion

        #region Constructors

            internal LinuxIPCBridge() {
                /* TODO */
            }

        #endregion

        #region Public methods

            public Task SendMessageToBackend(string message) {
                /* TODO */
            }

            public void Dispose() {
                /* TODO */
            }

        #endregion
    }
}