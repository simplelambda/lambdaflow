using System;

namespace LambdaFlow {
    internal class StrategyFactory {
        internal IStrategy GetStrategy() {
            return Utilities.securityMode switch {
                SecurityMode.MINIMAL => new MinimalStrategy(),
                SecurityMode.INTEGRITY => new WindowsIntegrityStrategy(),
                SecurityMode.HARDENED => new WindowsHardenedStrategy(),
                _ => throw new NotSupportedException($"Security mode '{mode}' is not supported.")
            }
        }
    }
}