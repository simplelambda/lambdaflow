using System;
using System.IO;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Runtime.Versioning;

using System.Windows.Forms;
using Microsoft.Web.WebView2.WinForms;
using Microsoft.Web.WebView2.Core;

namespace LambdaFlow {
    [SupportedOSPlatform("windows")]
    internal class WindowsWebView : IWebViewPlatform {
        #region Variables

            public event Action<string> WebMessageReceived;

            private WebView2? _view;
            private Form? _host;

        #endregion

        #region Public methods

            public async void Initialize(Config config) {
                _host = new Form{
                    Text = config?.Window.Title ?? "LambdaFlow app",
                    WindowState = FormWindowState.Maximized,
                    StartPosition = FormStartPosition.CenterScreen
                };

                _view = new WebView2 { Dock = DockStyle.Fill };
                _host.Controls.Add(_view);

                var env = await CoreWebView2Environment.CreateAsync();
                await _view.EnsureCoreWebView2Async(env);

                _view.CoreWebView2.AddWebResourceRequestedFilter( uri: "https://app/*", CoreWebView2WebResourceContext.All);
                //_view.CoreWebView2.WebResourceRequested += HandlePakRequest; AÑADIR PAK REQUEST HANDLER AQUÍ

                //await InjectBridgeAsync();  REVISAR
                _view.CoreWebView2.WebMessageReceived += (_, e) => WebMessageReceived?.Invoke(e.WebMessageAsJson);

                Navigate(config?.FrontendInitialHTML ?? "index.html");
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

                /*if (urlOrHtml.TrimStart().StartsWith("<", StringComparison.Ordinal)){
                    _view.NavigateToString(urlOrHtml);
                    return;
                }

                if (Uri.TryCreate(urlOrHtml, UriKind.Absolute, out var abs)) _view.CoreWebView2.Navigate(abs.ToString());         
                else{
                    var safePath = urlOrHtml.TrimStart('/');
                    _view.CoreWebView2.Navigate($"https://app/{safePath}");
                }*/
            }

            public void SendMessageToWeb(string json) => _view?.CoreWebView2?.PostWebMessageAsJson(json);

            public void Start() {
                if (_host is null) throw new InvalidOperationException("Initialize must be called first.");
                Application.EnableVisualStyles();
                Application.Run(_host);
            }

        #endregion

        #region Private methods

        #endregion
    }
}