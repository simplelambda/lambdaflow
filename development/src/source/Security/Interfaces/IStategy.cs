namespace LambdaFlow {
	internal interface IStrategy {
		internal Config config { get; private set; }

        internal void ApplySecurity();
	}
}