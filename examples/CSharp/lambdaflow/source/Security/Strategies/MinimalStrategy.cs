using System.Security;

namespace LambdaFlow {
    internal class MinimalStrategy : IStrategy {
        #region Variables

            private readonly ISigner _signer = SignerFactory.GetSigner();
            private readonly IProtector _protector = ProtectorFactory.GetProtector();

        #endregion

        #region Public methods

            public void ApplySecurity() {
                if (!_signer.Verify()) throw new SecurityException($"Integrity failure. Executable modified.");
            }

        #endregion
    }
}