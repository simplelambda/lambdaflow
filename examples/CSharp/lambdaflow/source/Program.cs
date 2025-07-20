using System;
using System.IO;
using System.IO.Compression;
using System.Formats.Asn1;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace LambdaFlow {
    internal static class Program {

        private static readonly IServices services = ServicesFactory.GetServices();

        [STAThread]
        static void Main(string[] args) {
            Console.WriteLine("Inicio Main");

            // Apply initial security strategy
            services.Strategy.ApplySecurity();
            Console.WriteLine("Seguridad aplicada");

            // Bind IPC bridge event, when the backend sends a message to the frontend
            services.IPCBridge.Initialize();
            services.IPCBridge.OnProcessStdOut += async message => {
                services.WebView.SendMessageToFrontend(message);
            };
            Console.WriteLine("IPC iniciado");

            // Initialize Webview
            services.WebView.Initialize(services.IPCBridge);
            Console.WriteLine("Webview Iniciado");

            // Start Application
            services.WebView.Navigate(Config.FrontendInitialHTML);
            services.WebView.Start();  
        }
    }
}