using System;

namespace LambdaFlow{
    internal static class SignerFactory{
        internal static ISigner GetSigner(){
            return Utilities.Platform switch{
                #if WINDOWS
                    Platform.WINDOWS => new WindowsSigner(),
                #elif LINUX
                    Platform.LINUX => new LinuxSigner(),
                #elif ANDROID
                    Platform.ANDROID => new AndroidSigner(),
                #endif
                _ => throw new PlatformNotSupportedException($"'{Utilities.Platform}' is not supported.")
            };
        }

    }
}