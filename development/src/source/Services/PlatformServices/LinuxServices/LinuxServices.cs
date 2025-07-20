using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lambdaFlow {
    internal class LinuxServices : IServices {
        IProtector Protector { get; };
        IIPCBridge IPCBridge { get; };
        IWebView WebView     { get; };
        ISigner Signer       { get; };
        IStrategy Strategy   { get; };

        internal LinuxServices() {
            Protector = new LinuxProtector();
            IPCBridge = new LinuxIPCBridge();
            WebView = new LinuxWebView();
            Signer = new LinuxSigner();
            Strategy = StrategyFactory.GetStrategy(Signer, Protector);
        }
    }
