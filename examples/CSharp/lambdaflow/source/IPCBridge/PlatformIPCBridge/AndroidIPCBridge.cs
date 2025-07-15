namespace Lambdaflow {
    internal class AndroidIPCBridge : IIPCBridge {
        #region Events

            internal event Func<string, Task> OnProcessStdOut;

        #endregion

        #region Constructors

            internal AndroidIPCBridge() {
                /* TODO */
            }

        #endregion

        #region Public methods

            public void Dispose() {
                /* TODO */
            }

        #endregion

        #region Internal methods

            internal Task SendMessageToBackend(string message) {
                /* TODO */
            }

        #endregion
    }
}