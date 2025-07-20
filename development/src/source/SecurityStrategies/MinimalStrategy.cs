using System.Security;

namespace LambdaFlow {
    internal class MinimalStrategy : IStrategy {
        #region Variables

            private readonly ISigner _signer;
            private readonly IProtector _protector;

        #endregion

        #region Constructors

            internal MinimalStrategy(ISigner signer, IProtector protector) {
                _signer = signer;
                _protector = protector;
            }

        #region Public methods

            public void ApplySecurity() {
                if (!_signer.Verify()) throw new SecurityException($"Integrity failure. Executable modified.");
            }

        #endregion
    }
}