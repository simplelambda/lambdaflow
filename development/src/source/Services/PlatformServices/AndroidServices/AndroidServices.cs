using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lambdaFlow {
    internal class AndroidServices : IServices {
        IProtector Protector { get; };
        IIPCBridge IPCBridge { get; };
        IWebView WebView     { get; };
        ISigner Signer       { get; };
        IStrategy Strategy   { get; };

        internal LinuxServices() {
            Protector = new AndroidProtector();
            IPCBridge = new AndroidIPCBridge();
            WebView = new AndroidWebView();
            Signer = new AndroidSigner();
            Strategy = StrategyFactory.GetStrategy(Signer, Protector);
        }
    }
}
