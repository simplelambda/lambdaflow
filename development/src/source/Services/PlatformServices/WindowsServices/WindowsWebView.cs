using System;
using System.IO;
using System.Text.Json;
using System.Diagnostics;
using System.IO.Compression;
using System.Threading.Tasks;
using System.Runtime.Versioning;
using System.Drawing;

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
                    StartPosition = FormStartPosition.CenterScreen,
                    Width = Config.Window.Width,
                    Height = Config.Window.Height,
                    MinimumSize = new System.Drawing.Size(Config.Window.MinWidth, Config.Window.MinHeight),
                    MaximumSize = new System.Drawing.Size(Config.Window.MaxWidth, Config.Window.MaxHeight),
                    Icon = new Icon("app.ico")
                };

                // Create webview control
                _view = new WebView2 { Dock = DockStyle.Fill };
                _host.Controls.Add(_view);

                // Initialize the WebView2 environment
                var browserArgs = DetermineFastResolverArgs();
                var options = new CoreWebView2EnvironmentOptions(browserArgs);

                var env = await CoreWebView2Environment.CreateAsync(
                              browserExecutableFolder: null,
                              userDataFolder: null,
                              options: options);

                await _view.EnsureCoreWebView2Async(env);

                #if !DEBUG 
                    var settings = _view.CoreWebView2.Settings;
                    settings.AreBrowserAcceleratorKeysEnabled = false;
                    settings.AreDefaultContextMenusEnabled = false;
                    settings.IsStatusBarEnabled = false;
                #endif

                // Bind frontend methods send(msg) and receive(msg)
                await BindFrontendMethods();

                // Set the action for when a message is received from the frontend
                _view.CoreWebView2.WebMessageReceived += (_, e) => {
                    string msg = e.TryGetWebMessageAsString();
                    Console.WriteLine($"Message from frontend: {msg}");
                    ipcBridge.SendMessageToBackend(msg);
                };

                // Open the frontend.pak file

                if (Utilities.FrontFS is not null) _pak = new ZipArchive(Utilities.FrontFS, ZipArchiveMode.Read, leaveOpen: false);         
                else                               _pak = new ZipArchive(File.OpenRead("frontend.pak"), ZipArchiveMode.Read, leaveOpen: false);

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
            }

            public void SendMessageToFrontend(string message) {
                string jsArg = JsonSerializer.Serialize(message);
                Console.WriteLine($"Sending message to frontend: {jsArg}");
                _view?.CoreWebView2?.ExecuteScriptAsync($"window.receive({jsArg});");
            }

            public void ModifyTitle(string title) {
                if (_host is not null) _host.Text = title;
            }

            public void ModifySize(int width, int height) {
                if (_host is not null) {
                    _host.Width = width;
                    _host.Height = height;
                }
            }

            public void ModfyMinSize(int width, int height) {
                if (_host is not null) {
                    _host.MinimumSize = new Size(width, height);
                }
            }

            public void ModifyMaxSize(int width, int height) {
                if (_host is not null) {
                    _host.MaximumSize = new Size(width, height);
                }
            }

            public void ModifyPosition(int x, int y) {
                if (_host is not null) {
                    _host.StartPosition = FormStartPosition.Manual;
                    _host.Location = new Point(x, y);
                }
            }

            public void Minimize() {
                if (_host is not null) {
                    _host.WindowState = FormWindowState.Minimized;
                }
            }

            public void Maximize() {
                if (_host is not null) {
                    _host.WindowState = FormWindowState.Maximized;
                }
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

            private string? DetermineFastResolverArgs() {
                var verStr = CoreWebView2Environment.GetAvailableBrowserVersionString();
                if (int.TryParse(verStr.Split('.')[0], out int major) && major >= 118) {
                    return $"--host-resolver-rules=\"MAP app 0.0.0.0\"";
                }

                TryAddHostsEntry();
                return null;
            }

            private static void TryAddHostsEntry() {
                try {
                    string hostsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), @"drivers\etc\hosts");

                    var lines = File.ReadAllLines(hostsPath);
                    foreach (var ln in lines)
                        if (ln.Contains($" app")) return;

                    File.AppendAllText(hostsPath,
                        $"{Environment.NewLine}127.0.0.1    app{Environment.NewLine}");
                }
                catch {

                }
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