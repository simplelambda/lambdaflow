using System;
using System.Threading.Tasks;

namespace LambdaFlow {
    internal interface IIPCBridge : IDisposable {
        event Func<string, Task>? OnProcessStdOut;

        void Initialize();
        Task SendMessageToBackend(string message);
    }
}