using System;

namespace LambdaFlow {
    internal static class ServicesFactory {
        internal static IServices GetServices() {
            return Config.Platform switch{
                #if WINDOWS
                    Platform.WINDOWS => new WindowsServices(),
                #elif LINUX
                    Platform.LINUX => new LinuxServices(),
                #elif ANDROID
                    Platform.ANDROID => new AndroidServices(),
                #endif
                _ => throw new PlatformNotSupportedException($"'{Config.Platform}' is not supported.")
            };
        }
    }
}