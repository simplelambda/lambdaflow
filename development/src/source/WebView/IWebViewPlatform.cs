using System;

namespace LambdaFlow {
    internal interface IWebViewPlatform {
        void Initialize(Config config);
        bool CheckAvailability();
        void InstallPrerequisites();

        void Navigate(string urlOrHtml);
        void SendMessageToWeb(string json);

        event Action<string> WebMessageReceived;
        void Start();
    }
}