using System;
using System.Threading.Tasks;
using System.Runtime.Versioning;

namespace LambdaFlow {
    [SupportedOSPlatform("windows")]
    internal class WindowsIPCBridge : IIPCBridge {
        #region Events

            public event Func<string, Task>? OnProcessStdOut;

        #endregion

        #region Constructors

            internal WindowsIPCBridge() {
                /* TODO */
            }

        #endregion

        #region Public methods

            public Task SendMessageToBackend(string message) {
                /* TODO */
                return Task.CompletedTask;
            }

            public void Dispose() {
                /* TODO */
            }

        #endregion
    }
}