using System;

namespace LambdaFlow {
    internal static class WebViewFactory {
        internal static IWebViewPlatform GetWebView() {
            return Utilities.Platform switch{
                #if WINDOWS
                    Platform.WINDOWS => new WindowsWebView(),
                #elif LINUX
                    Platform.LINUX => new LinuxWebView(),
                #elif ANDROID
                    Platform.ANDROID => new AndroidWebView(),
                #endif
                _ => throw new NotSupportedException($"{Utilities.Platform} is not supported.")
            };

        }
    }
}