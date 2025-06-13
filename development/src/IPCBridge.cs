using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LambdaFlow {
    public class IPCBridge : IDisposable {
        private readonly Process _process;
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();

        public IPCBridge(string executablePath, string[] args) {

            // Create a new process that executes the specifies executable. 

            _process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = executablePath,
                    RedirectStandardInput = true,  // allows us to send messages to the backend
                    RedirectStandardOutput = true, // allows us to read messages from the backend
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };


            // Add arguments to the process start info if any are provided.

            foreach (var arg in args) _process.StartInfo.ArgumentList.Add(arg);


            // Start the process. This will launch the backend executable.

            _process.Start();
        }

        //This method read the stdout of the backend process in a loop, and calls the provided callback function for each line read.
        public async Task StartReadLoopAsync(Func<string, Task> onMessageAsync) {
            try {
                var reader = _process.StandardOutput; // Get the standard output stream of the process

                while (!_cts.IsCancellationRequested && !_process.HasExited) {
                    var line = await reader.ReadLineAsync().ConfigureAwait(false); // Read a line from the standard output stream asynchronously
                    if (line is null)
                        break;

                    await onMessageAsync(line).ConfigureAwait(false);              // Call the provided callback function with the line read
                }
            }
            catch (OperationCanceledException) { }
        }

        // This method sends a message to the backend process via its standard input stream.
        public async Task SendAsync(string message) {
            if (_process.HasExited)
                throw new InvalidOperationException("Backend process has already exited.");

            await _process.StandardInput.WriteLineAsync(message).ConfigureAwait(false); // Write the message to the standard input stream of the backend process
            await _process.StandardInput.FlushAsync().ConfigureAwait(false);            // Ensure the message is sent immediately
        }

        // This method disposes the IPCBridge, cancelling any ongoing operations and closing the backend process gracefully.
        public void Dispose() {
            _cts.Cancel();
            if (!_process.HasExited) {
                _process.CloseMainWindow();
                if (!_process.WaitForExit(2000)) _process.Kill();
            }
            _process.Dispose();
            _cts.Dispose();
        }
    }
}