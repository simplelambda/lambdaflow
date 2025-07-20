namespace LambdaFlow {
    internal interface IServices {
        IProtector Protector { get; }
        IIPCBridge IPCBridge { get; }
        IWebView WebView { get; }
        ISigner Signer { get; }
        IStrategy Strategy { get; }
    }
}
