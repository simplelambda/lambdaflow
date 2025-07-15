namespace LambdaFlow {
    internal interface IIPCBridge : IDisposable {
        internal event Func<string, Task> OnProcessStdOut;
        internal Task SendMessageToBackend(string message);
    }
}