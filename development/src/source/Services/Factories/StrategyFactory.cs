using System;

namespace LambdaFlow {
    internal static class StrategyFactory {
        internal static IStrategy GetStrategy(ISigner signer, IProtector protector) {
            return Config.SecurityMode switch {
                #if MINIMAL
                    SecurityMode.MINIMAL => new MinimalStrategy(signer, protector),
                #elif INTEGRITY
                    SecurityMode.INTEGRITY => new IntegrityStrategy(signer, protector),
                #elif HARDENED
                    SecurityMode.HARDENED => new HardenedStrategy(signer, protector),
                #elif RUN
                    SecurityMode.RUN => new RunStrategy(),
                #endif

                _ => throw new NotSupportedException($"Security mode '{Config.SecurityMode}' is not supported.")
            };
        }
    }
}