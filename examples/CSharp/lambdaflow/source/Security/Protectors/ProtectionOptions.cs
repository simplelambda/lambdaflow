namespace LambdaFlow {
    internal class ProtectionOptions {

        internal bool AllowRead { get; set; } = false;
        internal bool AllowWrite { get; set; } = false;
        internal bool AllowExecute { get; set; } = false;
        internal bool AllowDelete { get; set; } = false;

        internal bool RequireElevation { get; set; } = false;
    }
}