namespace LambdaFlow {
    internal class AppLauncher {
        #region Variables

            private readonly Config _config;
            private readonly IStrategy _strategy;
            private readonly IWebViewPlatform _webview;
            private readonly IIPCBridge _ipc;

        #endregion

        #region Constructors

            public AppLauncher() {
                _security = StrategyFactory.GetStrategy();
                _webview = WebViewFactory.GetWebView();
                _ipc = IPCBridgeFactory.GetIPCBridge();
            }

        #endregion

        #region Internal methods

            internal void Run() {
                SetupSecurity();
                SetupWebView();
                SetupIpc();
                LoadUI();
            }

        #endregion

        #region Private methods

            private void SetupSecurity() {
                _strategy.Appl
            }

            private voir LoadConfig(){
                var stream = Utilities.GetEmbeddedResourceStream("config.json");
                _config = Config.CreateConfig(stream);
            }

            private void SetupWebView() {
                _webview.Initialize(_config);
                _webview.WebMessageReceived += _ipc.OnWebMessage;
            }

            private void SetupIpc() {
                _ipc.StdOutReceived += _webview.SendMessageToWeb;
                _ipc.LaunchBackend(_config.BackendPath);
            }

            private void LoadUI() {
                _webview.Navigate(_config.InitialPage);
            }

        #endregion
    }
}