ing System.Runtime.Versioning;

namespace LambdaFlow {
    [SupportedOSPlatform("android")]
    internal class AndroidWebView : IWebViewPlatform {
        #region Variables

            public event Action<string> WebMessageReceived;

        #endregion

        #region Public methods

            public void Initialize(Config config) {
                /* TODO */
            }

            public bool CheckAvailability() {
                /* TODO */
            }

            public void InstallPrerequisites() {
                /* TODO */
            }

            public void Navigate(string url) {
                /* TODO */
            }

            public void SendMessageToWeb(string json) {
                /* TODO */
            }

            public void Start() {
                /* TODO */
            }

        #endregion
    }
}