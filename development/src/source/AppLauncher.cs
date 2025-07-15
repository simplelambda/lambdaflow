namespace LambdaFlow
{
    internal class AppLauncher
    {
        #region Variables

        private readonly Config _config;
        private readonly IStrategy _strategy;
        private readonly IWebViewPlatform _webview;
        private readonly IIPCBridge _ipc;

        #endregion

        #region Constructors

        public AppLauncher()
        {
            _strategy = StrategyFactory.GetStrategy();
            _webview = WebViewFactory.GetWebView();
            _ipc = IPCBridgeFactory.GetIPCBridge();
            _config = LoadConfig();
        }

        #endregion

        #region Internal methods

        internal void Run()
        {
            SetupSecurity();
            SetupWebView();
            SetupIpc();
            LoadUI();
        }

        #endregion

        #region Private methods

        private void SetupSecurity()
        {
            _strategy.ApplySecurity();
        }

        private Config LoadConfig()
        {
            var stream = Utilities.GetEmbeddedResourceStream("config.json");
            return Config.CreateConfig(stream);
        }

        private void SetupWebView()
        {
            _webview.Initialize(_config);
            //_webview.WebMessageReceived += _ipc.OnProcessStdOut; // REVISAR ESTO!!!!
        }

        private void SetupIpc()
        {
            // _ipc.OnProcessStdOut += _webview.SendMessageToWeb;
            //_ipc.LaunchBackend(_config.BackendPath); // REVISAR ESTO!!!!
        }

        private void LoadUI()
        {
            _webview.Navigate(_config.FrontendInitialHTML);
        }

        #endregion
    }
}