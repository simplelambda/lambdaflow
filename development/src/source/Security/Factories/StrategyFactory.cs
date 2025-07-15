using System;
using System;

namespace LambdaFlow {
    internal static class StrategyFactory {
        internal static IStrategy GetStrategy() {
            return Utilities.securityMode switch {
                #if MINIMAL
                    SecurityMode.MINIMAL => new MinimalStrategy(),
                #elif INTEGRITY
                    SecurityMode.INTEGRITY => new IntegrityStrategy(),
                #elif HARDENED
                    SecurityMode.HARDENED => new HardenedStrategy(),
                #endif
                _ => throw new NotSupportedException($"Security mode '{Utilities.securityMode}' is not supported.")
            };
        }
    }
}