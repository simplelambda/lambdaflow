using System;

namespace LambdaFlow {
    internal static class WebViewFactory {
        internal IWebViewPlatform GccccccccetWebView() {
            return Utilities.Platform switch{
                Platform.Windows => new WindowsWebView(),
                Platform.Linux => new LinuxWebView(),
                Platform.ANDROID => new AndroidWebView(),
                _ => throw new NotSupportedException($"{Utilities.Platform} is not supported.")
            };

        }
    }
}