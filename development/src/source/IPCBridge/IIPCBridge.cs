using System;
using System.Threading.Tasks;

namespace LambdaFlow {
    internal interface IIPCBridge : IDisposable {
        event Func<string, Task>? OnProcessStdOut;
        Task SendMessageToBackend(string message);
    }
}