namespace LambdaFlow {
    internal class MinimalStrategy : IStrategy {
        #region Variables

            protected readonly ISigner _signer = SignerFactory.GetSigner();
            protected readonly IResourceProtector _protector = ResourceProtectorFactory.GetResourceProtector();

        #endregion

        #region Properties

            internal Config Config { get; private set; }

        #endregion

        #region Internal methods

            internal void ApplySecurity() {
                if (!_signer.Verify()) throw new SecurityException($"Integrity failure. Executable modified.");

                Config = Config.CreateConfig(Utilities.GetEmbeddedResourceStream("config.json"));
            }

        #region
    }
}