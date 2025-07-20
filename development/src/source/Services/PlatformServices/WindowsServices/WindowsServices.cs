using System;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Versioning;

namespace LambdaFlow {
    [SupportedOSPlatform("windows")]
    internal class WindowsServices : IServices {
        public IProtector Protector { get; }
        public IIPCBridge IPCBridge { get; }
        public IWebView WebView { get; }
        public ISigner Signer { get; }
        public IStrategy Strategy { get; }

        internal WindowsServices() {
            Protector = new WindowsProtector();
            IPCBridge = new WindowsIPCBridge();
            WebView = new WindowsWebView();
            Signer = new WindowsSigner();
            Strategy = StrategyFactory.GetStrategy(Signer, Protector);
        }
    }
}