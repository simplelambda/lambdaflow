using System;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Runtime.InteropServices;

namespace LambdaFlow {
    internal class WindowConfig {
        internal string Title { get; set; } = "lambdaFlowApp";
        internal int Width { get; set; } = 800;
        internal int Height { get; set; } = 600;

        internal int MinWidth { get; set; } = 0;
        internal int MinHeight { get; set; } = 0;
        internal int MaxWidth { get; set; } = 0;
        internal int MaxHeight { get; set; } = 0;
    }

    internal static class Config {
        internal static readonly Platform Platform = GetPlatform();

        internal const string AppName = "lambdaFlowApp";
        internal const string AppVersion = "1.0.0";
        internal const string OrgName = "SimpleLambda";
        internal static readonly WindowConfig Window = new WindowConfig();
        internal const string FrontendInitialHTML = "index.html";

        internal const bool DebugMode = false;

        internal static readonly SecurityMode SecurityMode = SecurityMode.INTEGRITY;

        internal const string Integrity = "";
        internal const string PublicKeyPem = @"";



        private static Platform GetPlatform() {
            if (OperatingSystem.IsBrowser()) return Platform.WEB;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return Platform.WINDOWS;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) return Platform.LINUX;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) return Platform.MACOS;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Create("ANDROID"))) return Platform.ANDROID;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Create("IOS"))) return Platform.IOS;

            return Platform.UNKNOWN;
        }
    }
}