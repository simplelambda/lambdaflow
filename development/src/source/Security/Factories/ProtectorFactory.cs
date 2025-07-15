using System;

namespace LambdaFlow {
    internal static class ProtectorFactory {
        internal static IProtector GetProtector() {
            return Utilities.Platform switch{
                #if WINDOWS
                    Platform.WINDOWS => new WindowsProtector(),
                #elif LINUX
                    Platform.LINUX => new LinuxProtector(),
                #elif ANDROID
                    Platform.ANDROID => new AndroidProtector(),
                #endif
                _ => throw new PlatformNotSupportedException($"'{Utilities.Platform}' is not supported.")
            };
        }
    }
}