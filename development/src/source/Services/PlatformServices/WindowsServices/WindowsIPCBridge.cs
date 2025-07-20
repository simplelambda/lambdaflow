using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Threading.Channels;
using System.Runtime.Versioning;

namespace LambdaFlow {
    [SupportedOSPlatform("windows")]
    internal class WindowsIPCBridge : IIPCBridge {

        #region Variables

            private BackendProcess _backend;

            private readonly CancellationTokenSource _cts = new CancellationTokenSource();
            private readonly Channel<string> _sendQueue = Channel.CreateUnbounded<string>();

            private bool _stdinEmpty = true;
            private readonly object _flagLock = new object();

        #endregion

        #region Events

            public event Func<string, Task>? OnProcessStdOut;

        #endregion

        #region Public methods

            public void Initialize() {
                _backend = new BackendProcess();
                _backend.OnStdOut += HandleBackendStdOut;
            }

            public Task SendMessageToBackend(string message) {
                if (_backend.HasExited) throw new InvalidOperationException("Backend already exited.");

                lock (_flagLock) {
                    if (_stdinEmpty) {
                        _stdinEmpty = false;
                        return _backend.WriteLineAsync(message, _cts.Token);
                    }
                    else 
                        return _sendQueue.Writer.WriteAsync(message, _cts.Token).AsTask(); // Queue the message to be sent later
                }
            }

            public void Dispose() {
                _cts.Cancel();
                _backend.Dispose();
                _cts.Dispose();
            }

        #endregion

        #region Private methods

            private async Task HandleBackendStdOut(string message) {
                if (message == null) return;

                if (message == "__ACK__") {
                    lock (_flagLock) {
                        if (_sendQueue.Reader.TryRead(out var next)) {
                            _stdinEmpty = false;
                            _ = _backend.WriteLineAsync(next);
                        }
                        else
                            _stdinEmpty = true;
                    }
                }
                else if (OnProcessStdOut != null) {
                    try {
                        await OnProcessStdOut(message).ConfigureAwait(false);
                    }
                    catch { }
                }
            }

        #endregion
    }
}