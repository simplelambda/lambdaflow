using System.Runtime.Versioning;

namespace LambdaFlow {
    [SupportedOSPlatform("android")]
    internal class AndroidIPCBridge : IIPCBridge {
        #region Events

            public event Func<string, Task>? OnProcessStdOut;

        #endregion

        #region Constructors

            internal AndroidIPCBridge() {
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