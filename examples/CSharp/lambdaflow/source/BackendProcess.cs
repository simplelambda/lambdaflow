using System;
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.IO.Compression;
using System.Threading.Tasks;
using System.Runtime.Versioning;
using System.Threading.Channels;

namespace LambdaFlow {
    internal class BackendProcess : IDisposable {
        #region Variables

            private readonly Process _process;

            internal event Func<string, Task>? OnStdOut;

            internal bool HasExited => _process.HasExited;

        #endregion

        #region Constructors

            internal BackendProcess() {

                var backendPath = Path.Combine(AppContext.BaseDirectory, "backend", "Backend.exe");

                _process = new Process{
                    StartInfo = new ProcessStartInfo{
                        FileName = backendPath,
                        WorkingDirectory = Path.GetDirectoryName(backendPath),
                        RedirectStandardInput = true,
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    },
                    EnableRaisingEvents = true
                };

                _process.OutputDataReceived += StdOutHandler;
                //_process.Exited += (_, _) => OnStdOut?.Invoke(this, new DataReceivedEventArgs("__BACKEND_EXITED__")); REVISAR

                _process.Start();
                _process.BeginOutputReadLine();
                _process.StandardInput.AutoFlush = true;
            }

        #endregion

        #region Public methods

            public void Dispose() {
                if (!_process.HasExited) {
                    _process.CloseMainWindow();
                    if (!_process.WaitForExit(2_000))
                        _process.Kill();
                }
                _process.Dispose();
            }

        #endregion

        #region Internal methods

            internal Task WriteLineAsync(string line, CancellationToken ct = default) {
                return _process.StandardInput.WriteLineAsync(line.AsMemory(), ct);
            }

        #endregion

        #region Private methods

            private void StdOutHandler(object sender, DataReceivedEventArgs e) {
                if (e.Data is null) return;
                OnStdOut?.Invoke(e.Data);
            }

        #endregion
    }
}