using System;

namespace LambdaFlow {
    internal static class IPCBridgeFactory {
        internal static IIPCBridge GetIPCBridge() {
            return Utilities.Platform switch{
                #if WINDOWS
                    Platform.WINDOWS => new WindowsIPCBridge(),
                #elif LINUX
                    Platform.LINUX => new LinuxIPCBridge(),
                #elif ANDROID
                    Platform.ANDROID => new AndroidIPCBridge(),
                #endif
                _ => throw new PlatformNotSupportedException($"'{Utilities.Platform}' is not supported.")
            };
        }
    }
}