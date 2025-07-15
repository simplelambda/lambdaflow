using System;

namespace LambdaFlow {
    internal static class IPCBridgeFactory {
        internal static IIPCBridge GetIPCBridge() {
            return Utilities.Platform switch{
                Platform.WINDOWS => new DesktopIPCBridge(),
                Platform.LINUX => new DesktopIPCBridge(),
                Platform.ANDROID => new AndroidIPCBridge(),
                _ => throw new PlatformNotSupportedException($"'{Utilities.Platform}' is not supported.")
            };
        }
    }
}