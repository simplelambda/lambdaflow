namespace Lambdaflow {
    internal class AndroidWebView : IWebViewPlatform {
        #region Variables

            public event Action<string> WebMessageReceived;

        #endregion

        #region Internal methods

            internal void Initialize(Config config) {
                /* TODO */
            }

            internal bool CheckAvailability() {
                /* TODO */
            }

            internal void InstallPrerequisites() {
                /* TODO */
            }

            internal void Navigate(string url) {
                /* TODO */
            }

            internal void SendMessageToWeb(string json) {
                /* TODO */
            }

            internal void Start() {
                /* TODO */
            }

        #endregion
    }
}