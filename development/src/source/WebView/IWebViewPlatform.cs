namespace LambdaFlow {
    internal interface IWebViewPlatform {
        internal void Initialize(Config config);
        internal bool CheckAvailability();
        internal void InstallPrerequisites();

        internal void Navigate(string urlOrHtml);
        internal void SendMessageToWeb(string json);

        internal event Action<string> WebMessageReceived;
        internal void StartLoop();
    }
}