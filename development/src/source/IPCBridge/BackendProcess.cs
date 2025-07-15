namespace Lambdaflow {
    public class BackendProcessManager {
        #region Variables

            internal event Action<string> OnStdOut;

        #endregion

        #region Constructors

            public AppLauncher(Config config) {
                _config = config;
                _security = new SecurityManager(config.SecurityMode);
                _webview = WebViewFactory.Create();
                _ipc = new IPCBridge();
            }

        #endregion

        #region Internal methods

            internal void RouteMessage(string msg, Direction dir) {
                SetupSecurity();
                SetupWebView();
                SetupIpc();
                LoadUI();
            }

        #endregion

        #region Private methods

            private void SetupSecurity() {
                _security.VerifyIntegrity();
                _security.ApplyPolicies();
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