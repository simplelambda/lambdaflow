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
using System.Text.Json;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace LambdaFlow {
    internal static class Program {

        [STAThread]
        static void Main(string[] args) {
            // Create and run AppLauncher

            var launcher = new AppLauncher();
            launcher.Run();
        }
    }
}