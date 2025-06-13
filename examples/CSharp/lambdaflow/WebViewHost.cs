using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using SharpWebview;
using SharpWebview.Content;
using System.Text.Json;

namespace LambdaFlow
{
    public class WebViewHost
    {
        private readonly Webview _view;
        private readonly IPCBridge _bridge;

        public WebViewHost(WindowConfig cfg, IPCBridge bridge, string htmlPath)
        {
            _bridge = bridge;


            // Check if the loopback exemption is needed on Windows

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && !IsLoopbackExempt())
                TryExemptLoopback();


            // Create the Webview instance

            _view = new Webview()
                .SetTitle(cfg.Title)
                .SetSize(cfg.Width, cfg.Height, WebviewHint.None)
                .Navigate(new UrlContent(new Uri(Path.GetFullPath(htmlPath)).AbsoluteUri));


            // Register the communication bridge between JS and .NET
            _view.Bind("send", (id, reqJson) => {

                _ = _bridge.SendAsync(JsonSerializer.Deserialize<string[]>(reqJson)[0]) // Call the send method defined in IPCBridge
                    .ContinueWith(_ =>
                        _view.Return(id, RPCResult.Success, "{}")                       // Return success to the JS caller
                    );
            });

            // Start the read loop to handle messages from the backend
            _ = _bridge.StartReadLoopAsync(async msg =>
            {
                var js = $"window.receive({JsonSerializer.Serialize(msg)});"; // Create a JavaScript snippet to call the receive function with the message
                _view.Dispatch(() => _view.Evaluate(js));                     // Execute the JavaScript in the Webview              
            });
        }

        public void Run() => _view.Run();

        public void Dispose() => _view.Dispose();

        // Check if the loopback exemption is needed on Windows
        private bool IsLoopbackExempt()
        {
            try
            {
                var psi = new ProcessStartInfo("CheckNetIsolation.exe", $"LoopbackExempt -s -n=\"Microsoft.Win32WebViewHost_cw5n1h2txyewy\"")
                {
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var p = Process.Start(psi);
                var output = p?.StandardOutput.ReadToEnd() ?? "";
                p?.WaitForExit();

                return output.IndexOf("Microsoft.Win32WebViewHost_cw5n1h2txyewy", StringComparison.OrdinalIgnoreCase) != -1;
            }
            catch
            {
                return false;
            }
        }

        // Try to exempt the loopback for the WebViewHost on Windows
        private void TryExemptLoopback()
        {
            try
            {
                var psi = new ProcessStartInfo("CheckNetIsolation.exe", $"LoopbackExempt -a -n=\"Microsoft.Win32WebViewHost_cw5n1h2txyewy\"")
                {
                    UseShellExecute = true,
                    Verb = "runas",
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                };

                using var p = Process.Start(psi);
                p?.WaitForExit();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Could not exempt loopback: " + ex.Message);
            }
        }
    }
}
