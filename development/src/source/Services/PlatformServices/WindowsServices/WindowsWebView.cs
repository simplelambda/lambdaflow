using System;
using System.IO;
using System.Diagnostics;
using System.IO.Compression;
using System.Threading.Tasks;
using System.Runtime.Versioning;

using System.Windows.Forms;
using Microsoft.Web.WebView2.WinForms;
using Microsoft.Web.WebView2.Core;

namespace LambdaFlow {
    [SupportedOSPlatform("windows")]
    internal class WindowsWebView : IWebView {
        #region Variables

            private WebView2? _view;
            private Form? _host;
            private ZipArchive? _pak;

        #endregion

        #region Public methods

            
            public async void Initialize(IIPCBridge ipcBridge) {
                // Create the host form and WebView2 control
                _host = new Form
                {
                    Text = Config.Window.Title ?? "LambdaFlow app",
                    WindowState = FormWindowState.Maximized,
                    StartPosition = FormStartPosition.CenterScreen
                };

                // Create webview control
                _view = new WebView2 { Dock = DockStyle.Fill };
                _host.Controls.Add(_view);

                // Initialize the WebView2 environment
                var env = await CoreWebView2Environment.CreateAsync();
                await _view.EnsureCoreWebView2Async(env);

                // Bind frontend methods send(msg) and receive(msg)
                await BindFrontendMethods();

                // Set the action for when a message is received from the frontend
                _view.CoreWebView2.WebMessageReceived += (_, e) => {
                    string msg = e.TryGetWebMessageAsString();
                    ipcBridge.SendMessageToBackend(msg);
                };

                // Open the frontend.pak file
                _pak = new ZipArchive(File.OpenRead("frontend.pak"), ZipArchiveMode.Read, leaveOpen: false);

                // Virtual origin that maps to the frontend .pak
                _view.CoreWebView2.AddWebResourceRequestedFilter(uri: "https://app/*", CoreWebView2WebResourceContext.All);
                _view.CoreWebView2.WebResourceRequested += HandlePakRequest;

                // Navigate to the initial HTML
                Navigate(Config.FrontendInitialHTML ?? "index.html");
            }

            public void Start() {
                if (_host is null) throw new InvalidOperationException("Initialize must be called first.");
                Console.WriteLine("Starting WebView application...");

                Application.EnableVisualStyles();       
                Application.Run(_host);
            }

            public bool CheckAvailability() => !string.IsNullOrEmpty(CoreWebView2Environment.GetAvailableBrowserVersionString());

            public void InstallPrerequisites() {
                var installerUrl = "https://go.microsoft.com/fwlink/p/?LinkId=2124703";
                var tmp = Path.Combine(Path.GetTempPath(), "MicrosoftEdgeWebView2Bootstrapper.exe");
                using (var wc = new System.Net.WebClient()) wc.DownloadFile(installerUrl, tmp);

                var psi = new ProcessStartInfo(tmp, "/silent /install") {
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                Process.Start(psi)!.WaitForExit();
            }

            public void Navigate(string url) {
                if (_view is null || _view.CoreWebView2 is null) return;

                if (url.TrimStart().StartsWith("<", StringComparison.Ordinal)){
                    _view.NavigateToString(url);
                    return;
                }

                if (Uri.TryCreate(url, UriKind.Absolute, out var abs)) {
                    _view.CoreWebView2.Navigate(abs.ToString());
                    return;
                }

                var safePath = url.TrimStart('/');

                _view.CoreWebView2.Navigate($"https://app/{safePath}");

                Console.WriteLine($"Navigating to: https://app/{safePath}");
            }

            public void SendMessageToFrontend(string message) => _view?.CoreWebView2?.ExecuteScriptAsync($"window.receive(\"{message}\");");

            public void ModifyTitle(string title) {

            }
            public void ModifySize(int width, int height) {

            }
            public void ModfyMinSize(int width, int height) {

            }
            public void ModifyMaxSize(int width, int height) {

            }
            public void ModifyPosition(int x, int y) {

            }
            public void Minimize() {

            }
            public void Maximize() {

            }

        #endregion

        #region Private methods

            private async Task BindFrontendMethods() {
                await _view.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(@"
                    window.send = function(msg) {
                        window.chrome.webview.postMessage(msg);
                    };

                    window.receive = function(msg) {
                        console.warn('LambdaFlow: receive(msg) not implemented.');
                    };
                ");
            }

            private void HandlePakRequest(object? _, CoreWebView2WebResourceRequestedEventArgs e) {
                var uri = new Uri(e.Request.Uri); 
                var path = uri.AbsolutePath;

                if (path == "/" || string.IsNullOrEmpty(path)) path = "/index.html";  // If thr path is empty or just "/", serve index.html
                else if (path.EndsWith("/")) path += "index.html";                    // If the path ends with "/", append "index.html"

                // Normalize the path to remove any URL encoding and leading slashes
                var relPath = Uri.UnescapeDataString(path.TrimStart('/'));

                // Obtain the requested file from the .pak file, if it exists, otherwise 404 error will be shown
                byte[]? bytes = Utilities.ReadPAK(_pak, relPath);
                if (bytes == null) return;

                string contentType = Utilities.GetMimeType(relPath);

                Console.WriteLine($"PAK request for: {relPath} (Content-Type: {contentType})");

                var stream = new MemoryStream(bytes);
                e.Response = _view!.CoreWebView2.Environment.CreateWebResourceResponse(stream, 200, "OK", $"Content-Type: {contentType}");
            }

        #endregion
    }
}