using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using SharpWebview;
using SharpWebview.Content;
using System.Text.Json;
using System.IO.Compression;

#pragma warning disable CS1998

namespace LambdaFlow {
    public class WebViewHost {
        private readonly Webview _view;
        private readonly IPCBridge _bridge;
        private readonly ZipArchive _zip;

        public WebViewHost(WindowConfig cfg, IPCBridge bridge, FileStream frontendPakStream, string initialHtmlFileName) {
            _bridge = bridge;


            // Check if the loopback exemption is needed on Windows

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && !IsLoopbackExempt())
                TryExemptLoopback();

            // Create zipArchive from the frontend.pak stream

            _zip = new ZipArchive(frontendPakStream, ZipArchiveMode.Read, leaveOpen: false);

            var entry = _zip.GetEntry(initialHtmlFileName) ?? throw new FileNotFoundException($"Initial HTML file '{initialHtmlFileName}' not found");

            string htmlContent;
            using (var sr = new StreamReader(entry.Open()))
                htmlContent = sr.ReadToEnd();

            const string cspMeta =
              "<meta http-equiv=\"Content-Security-Policy\" " +
              "content=\"default-src 'self'; script-src 'self' 'unsafe-inline'; " +
              "style-src 'self'; img-src 'self';\">";

            if (!htmlContent.Contains("Content-Security-Policy"))
                htmlContent = htmlContent.Replace("<head>", "<head>" + cspMeta);

            // JS script ot override fetch

            string loaderScript = @"
                <script>
                (function(){
                  const origFetch = window.fetch.bind(window);
                  window.fetch = async (input, init) => {
                    let url = (typeof input === 'string' ? input : input.url);
                    if (url.startsWith('app://')) {
                      const path = url.substring('app://'.length);
                      const b64 = await window.readResource(path);
                      if (!b64) return new Response(null, { status: 404 });
                      const bin = Uint8Array.from(atob(b64), c => c.charCodeAt(0));
                      const ext = path.split('.').pop().toLowerCase();
                      const mime = {
                        html: 'text/html',
                        js:   'application/javascript',
                        css:  'text/css',
                        png:  'image/png',
                        jpg:  'image/jpeg',
                        svg:  'image/svg+xml'
                      }[ext] || 'application/octet-stream';
                      return new Response(bin, { headers: { 'Content-Type': mime } });
                    }
                    return origFetch(input, init);
                  };
                })();
                </script>";

            var fullHtml = loaderScript + htmlContent;

            // Create the Webview instance

            _view = new Webview()
                .SetTitle(cfg.Title)
                .SetSize(cfg.Width, cfg.Height, WebviewHint.None);

            _view.Navigate(new HtmlContent(fullHtml));

            // Bind the resource read JS function

            _view.Bind("readResource", (id, resourcePath) => {

                var clean = resourcePath.Replace('\\', '/').TrimStart('/');

                if (clean.Contains("..") || Path.IsPathRooted(clean)) {
                    _view.Return(id, RPCResult.Error, JsonSerializer.Serialize(new { error = "Invalid path" }));
                    return;
                }

                var resourceEntry = _zip.GetEntry(clean);

                if (resourceEntry == null) {
                    var errJson = JsonSerializer.Serialize(new { error = "Resource not found: " + resourcePath });
                    _view.Return(id, RPCResult.Error, errJson);
                    return;
                }

                using var es = resourceEntry.Open();
                using var ms = new MemoryStream();
                es.CopyTo(ms);

                var jsonRes = JsonSerializer.Serialize(new { result = Convert.ToBase64String(ms.ToArray()) });
                _view.Return(id, RPCResult.Success, jsonRes);
            });

            // Register the communication bridge between JS and .NET
            _view.Bind("send", (id, reqJson) => {

                _ = _bridge.SendAsync(JsonSerializer.Deserialize<string[]>(reqJson)[0]) // Call the send method defined in IPCBridge
                    .ContinueWith(_ =>
                        _view.Return(id, RPCResult.Success, "{}")                       // Return success to the JS caller
                    );
            });

            // Start the read loop to handle messages from the backend
            _ = _bridge.StartReadLoopAsync(async msg => {
                var js = $"window.receive({JsonSerializer.Serialize(msg)});"; // Create a JavaScript snippet to call the receive function with the message
                _view.Dispatch(() => _view.Evaluate(js));                     // Execute the JavaScript in the Webview              
            });
        }

        public void Run() => _view.Run();

        public void Dispose() => _view.Dispose();

        // Check if the loopback exemption is needed on Windows
        private bool IsLoopbackExempt() {
            try {
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
            catch {
                return false;
            }
        }

        // Try to exempt the loopback for the WebViewHost on Windows
        private void TryExemptLoopback() {
            try {
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
            catch (Exception ex) {
                Console.Error.WriteLine("Could not exempt loopback: " + ex.Message);
            }
        }
    }
}
