using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;

namespace LambdaFlow {
    public class IPCBridge : IDisposable {
        public readonly Process Process;
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();

        public IPCBridge(string executablePath, string[] args) {

            // Create a new process that executes the specifies executable. 

            Process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = executablePath,
                    WorkingDirectory = Path.GetDirectoryName(executablePath),
                    RedirectStandardInput = true,  // allows us to send messages to the backend
                    RedirectStandardOutput = true, // allows us to read messages from the backend
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };


            // Add arguments to the process start info if any are provided.

            foreach (var arg in args) Process.StartInfo.ArgumentList.Add(arg);


            // Start the process. This will launch the backend executable.

            Process.Start();
        }

        //This method read the stdout of the backend process in a loop, and calls the provided callback function for each line read.
        public async Task StartReadLoopAsync(Func<string, Task> onMessageAsync) {
            try {
                var reader = Process.StandardOutput; // Get the standard output stream of the process

                while (!_cts.IsCancellationRequested && !Process.HasExited) {
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
            if (Process.HasExited)
                throw new InvalidOperationException("Backend process has already exited.");

            await Process.StandardInput.WriteLineAsync(message).ConfigureAwait(false); // Write the message to the standard input stream of the backend process
            await Process.StandardInput.FlushAsync().ConfigureAwait(false);            // Ensure the message is sent immediately
        }

        // This method disposes the IPCBridge, cancelling any ongoing operations and closing the backend process gracefully.
        public void Dispose() {
            _cts.Cancel();
            if (!Process.HasExited) {
                Process.CloseMainWindow();
                if (!Process.WaitForExit(2000)) Process.Kill();
            }
            Process.Dispose();
            _cts.Dispose();
        }
    }
}