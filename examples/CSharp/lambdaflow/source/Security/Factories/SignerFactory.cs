using System;

namespace LambdaFlow{
    internal static class SignerFactory{
        internal static ISigner GetSigner(){
            return Utilities.Platform switch{
                Platform.WINDOWS => new WindowsProtector(),
                Platform.LINUX => new LinuxProtector(),
                Platform.ANDROID => new AndroidProtector(),
                _ => throw new PlatformNotSupportedException($"'{Utilities.Platform}' is not supported.")
            };
        }

    }
}