#!/usr/bin/env python3

import sys, traceback, platform, os, asyncio

from Utilities.Utilities import *
from Utilities.Config    import Config
from Platforms.Platform  import Platform

async def main():
    try:

        # =======================================
        # ---------- ENVIRONMENT SETUP ----------
        # =======================================

        mode = sys.argv[1]
        csproj_name = find_first_csproj()

        log(f"Compiling project {csproj_name} in {mode} mode", banner_type="info")
        log("PREPARING ENVIRONMENT")

        # ----- CHECK DEPENDENCIES -----

        #check_dependencies()

        # ----- CLEAN ROOT BIN AND OBJ -----

        log(f"Cleaning root bin and obj folders", banner_type="info")

        remove("bin")
        remove("obj")

        # ----- LOAD CONFIG.JSON -----

        log(f"Loading config.json", banner_type="info")
        config = Config("config.json")

        # ----- CREATE TMP FOLDER FOR COMPILE -----

        log(f"Creating TMP folder for compiling", banner_type="info")

        remove("lambdaflow/TMP")
        mkdir("lambdaflow/TMP")
            
        # ==============================================
        # ---------------  INJECT CONFIG ---------------
        # ==============================================

        log("INJECTING CONFIGURATION", banner_type="subtitle")

        inject_global_variable("lambdaflow/source/Config.cs", "AppName", f"\"{config.Get('appName', 'lambdaflowApp')}\"")
        inject_global_variable("lambdaflow/source/Config.cs", "AppVersion", f"\"{config.Get('appVersion', '1.0.0')}\"")
        inject_global_variable("lambdaflow/source/Config.cs", "OrgName", f"\"{config.Get('orgName', 'LambdaFlow')}\"")
        inject_global_variable("lambdaflow/source/Config.cs", "FrontendInitialHTML", f"\"{config.Get('frontendInitialHTML', 'index.html')}\"")
        inject_global_variable("lambdaflow/source/Config.cs", "AppIcon", f"\"{config.Get('appIcon', 'app.ico')}\"")
        inject_global_variable("lambdaflow/source/Config.cs", "SecurityMode", f"SecurityMode.{config.Get('securityMode', 'Integrity').upper()}")

        inject_global_variable("lambdaflow/source/Config.cs", "Title", f"\"{config.Get('window.title', 'lambdaflowApp')}\"")
        inject_global_variable("lambdaflow/source/Config.cs", "Width", f"{config.Get('window.width', 800)}")
        inject_global_variable("lambdaflow/source/Config.cs", "Height", f"{config.Get('window.height', 600)}")
        inject_global_variable("lambdaflow/source/Config.cs", "MinWidth", f"{config.Get('window.minWidth', 0)}")
        inject_global_variable("lambdaflow/source/Config.cs", "MinHeight", f"{config.Get('window.minHeight', 0)}")
        inject_global_variable("lambdaflow/source/Config.cs", "MaxWidth", f"{config.Get('window.maxWidth', 0)}")
        inject_global_variable("lambdaflow/source/Config.cs", "MaxHeight", f"{config.Get('window.maxHeight', 0)}") 

        copy(config.Get("appIcon", "app.ico"), f"lambdaflow/TMP/")

        # ==============================================
        # ----------  APPLICATION COMPILATION ----------
        # ==============================================

        log("APPLICATION COMPILATION")

        currentPlatform = platform.system()

        # ----- PACKAGE FRONTEND INTO PAK -----

        log('Packaging frontend into frontend.pak...', banner_type="info")
        pakFolder("frontend", "lambdaflow/TMP/frontend.pak")

        # ----- COMPILE BACKEND -----

        result_name = f"{config.Get("appName", "lambdaflowApp")}-v{config.Get("appVersion", "1.0.0")}" if config.Get("includeVersionInResultName", True) else config.Get("appName", "lambdaflowApp")

        log(f"Compiling backend and framework{currentPlatform} platform", banner_type="info")

        await asyncio.gather(
            build_backend(currentPlatform, config),
            build_framework(currentPlatform, config, csproj_name, mode, result_name)
        )

        # ----- COMPILATION FOR CURRENT PLATFORM -----

        log('PROGRAM EXECUTION')

        subprocess.call([Normalize(f"lambdaflow/TMP/{result_name}" + (".exe" if currentPlatform.startswith("Windows") else ""))], cwd=Normalize("lambdaflow/TMP"))

    except Exception:
        traceback.print_exc()

async def build_backend(currentPlatform, config):
    currentArch = {
        "x86":    "x86",
        "x86_32": "x86",
        "i386":   "x86",
        "i486":   "x86",
        "i586":   "x86",
        "i686":   "x86",

        "x86_64": "x64",
        "amd64":  "x64",
        "AMD64":  "x64",

        "armv6l": "arm",
        "armv7l": "arm",
        "armv7":  "arm",
        "arm":    "arm",

        "aarch64": "arm64",
        "arm64":   "arm64",
        "armv8":   "arm64",
        "armv8l":  "arm64",
    }[platform.machine()]

    for _os, os_info in config.Get("platforms", {}).items():
        if _os.capitalize() != currentPlatform:
            continue

        for arch, arch_info in config.Get(f"platforms.{_os}.archs", {}).items():
            if arch != currentArch:
                continue

            base_key = f"platforms.{_os}.archs.{arch}"

            cmd = config.Get(f"{base_key}.compileCommand", None)
            if not cmd:
                log(f"ERROR: {self}: no compile command specified.", banner_type="info")
                return

            backend_cwd = Normalize(f"{config.Get("developmentBackendFolder", "backend")}")
            await run_async(cmd, cwd=backend_cwd)
            out_backend = Normalize(f"{config.Get("developmentBackendFolder", "backend")}/{config.Get(f"{base_key}.compileDirectory", "bin")}")

            if not os.path.isdir(out_backend):
                raise RuntimeError(f"ERROR: Expected backend output in '{out_backend}' but not found.")

            pakFolder(out_backend, Normalize("lambdaflow/TMP/backend.pak"))

            break

async def build_framework(currentPlatform, config, csproj_name, mode, result_name):
    inject_global_variable("lambdaflow/source/Config.cs", "SecurityMode", "SecurityMode.RUN")

    rid = { 
        "Windows" : "win-x64",
        "Linux"   : "linux-x64",
        "Darwin"  : "osx-x64"
    }[currentPlatform]

    target = {
        "Windows": "net8.0-windows",
        "Linux": "net8.0",
        "Android": "net8.0-android",
    }[currentPlatform]

    await run_async(f"dotnet build {csproj_name}.csproj -c {mode} -f {target} -r {rid} -o {Normalize("lambdaflow/TMP/")} -p:AssemblyName={result_name} -p:SelfContained=false -p:PublishSingleFile=false -p:PublishTrimmed=false -p:RunDefine=RUN {'-p:DebugDefine=DEBUG' if mode == "Debug" else ''} -p:PlatformDefine={platform.system().upper()} -p:SecurityDefine=RUN", cwd=Normalize(""))

if __name__ == "__main__":
    asyncio.run(main())