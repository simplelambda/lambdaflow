namespace LambdaFlow {

    internal enum Platform {
        WINDOWS,
        LINUX,
        MACOS,
        ANDROID,
        IOS,
        WEB,
        UNKNOWN
    }

    internal enum SecurityMode {
        MINIMAL,
        INTEGRITY,
        HARDENED,
        RUN
    }
}