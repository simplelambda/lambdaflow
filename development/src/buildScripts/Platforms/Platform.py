from Utilities.Utilities import *
from Platforms.OS        import OS 
from Platforms.Arch      import Arch 
from Installer           import *

from SecurityStrategies.Strategy  import Strategy
from SecurityStrategies.Minimal   import Minimal
from SecurityStrategies.Integrity import Integrity
from SecurityStrategies.Hardened  import Hardened

class Platform():
    def __init__(self, os, arch):
        self.os = os
        self.arch = arch

    def __eq__(self, other):
        return self.arch == other.arch and self.os == other.os

    def __str__(self):
        return f"{self.os}-{self.arch}"

    def Compile(self, config, mode):
        try:
            if config.Get(f"platforms.{self.os}.ignore", False) or config.Get(f"platforms.{self.os}.archs.{self.arch}.ignore", False):
                return

            log(f"Compiling for platform: {self}", banner_type="subtitle")

            base_key = f"platforms.{self.os}.archs.{self.arch}"

            # ----- OBTAIN COMPILE COMMAND FOR BACKEND -----

            cmd = config.Get(f"{base_key}.compileCommand", None)
            if not cmd:
                log(f"ERROR: {self}: no compile command specified.", banner_type="info")
                return

            # ----- BACKEND COMPILATION -----

            log("Compiling backend...", banner_type="info")

            backend_cwd = Normalize(f"{config.Get("developmentBackendFolder", "backend")}")
            run(cmd, cwd=backend_cwd)
            out_backend = Normalize(f"{config.Get("developmentBackendFolder", "backend")}/{config.Get(f"{base_key}.compileDirectory", "bin")}")

            if not os.path.isdir(out_backend):
                raise RuntimeError(f"ERROR: Expected backend output in '{out_backend}' but not found.")

            # ----- PACKAGE BACKEND INTO PAK -----

            log('Packaging backend into backend.pak...', banner_type="info")
            pakFolder(out_backend, Normalize("lambdaflow/TMP/backend.pak"))

            # ----- KEY GENERATION -----

            log('Generating keys for signing', banner_type="info")
            generate_key_pair("lambdaflow/TMP/public.pub", "lambdaflow/TMP/private.pem")
    
            # ----- GET SECURITY MODE ------

            log('Getting security mode', banner_type="info")

            # Try to get the security mode from the platform-specific configuration first
            security_mode = config.Get(f"{base_key}.securityMode", None)

            # If not found, try to get it from the general OS configuration
            if not security_mode:
                security_mode = config.Get(f"platforms.{self.os}.securityMode", None)

            # If still not found, try to get it from de general configuration and if not found, default to "Integrity"
            if not security_mode:
                security_mode = config.Get("securityMode", "Integrity")

            # Normalize security mode format
            security_mode = security_mode.capitalize()

            # ----- FRAMEWOKS CODE MODIFICATION -----

            log(f"Modifying framework code for security mode {security_mode}", banner_type="info")
            
            if self.os.startswith("windows") and config.Get(f"platforms.{self.os}.archs.{self.arch}.useAuthenticode", False):
                inject_global_variable("lambdaflow/source/Services/PlatformServices/WindowsServices/WindowsSigner.cs", "useAuthenticode", "true")

            # ----- APPLY SECURITY STRATEGY -----
            
            strategy = None

            if security_mode == "Minimal":
                strategy = Minimal()
            elif security_mode == "Integrity":
                strategy = Integrity()
            elif security_mode == "Hardened":
                strategy = Hardened()
            else:
                raise ValueError(f"Unknown security mode: {securityMode}")

            log(f"Applying security strategy: {security_mode}", banner_type="info")
            strategy.Apply()

            # ----- FRAMEWORK COMPILATION -----

            log(f"Compyling framework...", banner_type="info")
            result_name = f"{config.Get("appName", "lambdaflowApp")}-v{config.Get("appVersion", "1.0.0")}" if config.Get("includeVersionInResultName", True) else config.Get("appName", "lambdaflowApp")
            
            self.__compile_framework(result_name, config, security_mode, mode)

            # ------ SIGN EXECUTABLE -----

            log("Signing executable", banner_type="info")

            result_folder = Normalize(config.Get("resultFolder", "Results"))
            copy(config.Get("appIcon", "app.ico"), f"{result_folder}/{self.os}/{self.arch}/")

            sign([f"{result_folder}/{self.os}/{self.arch}/{result_name}" + (".exe" if self.os.startswith("windows") else ""),
                  f"{result_folder}/{self.os}/{self.arch}/{result_name}.dll"], 
                  "lambdaflow/TMP/private.pem", 
                  f"{result_folder}/{self.os}/{self.arch}/integrity.sig"
            )

            # ----- PACKAGE -----
       
            package = config.Get(f"{base_key}.package", False)

            if package:
                log(f"Packaging installer", banner_type="info")

                if self.os == "windows":    

                    install_dir = ""

                    if self.arch != "x86" and self.arch != "x32":
                        install_dir = "$PROGRAMFILES64" if security_mode == "Hardened" else "$APPDATA"
                    else:
                        install_dir = "$PROGRAMFILES" if security_mode == "Hardened" else "$APPDATA"

                    print(install_dir)

                    build_windows_installer(
                        config,
                        result_name,
                        results_dir=os.path.join(result_folder, self.os, self.arch),
                        app_name=config.Get("appName", "lambdaflowApp"),
                        app_version=config.Get("appVersion", "1.0.0"),
                        org_name=config.Get("organizationName", "SimpleLambda"),
                        install_dir=install_dir,
                        arch=self.arch
                    )
                else:
                    build_unix_installer(
                        target=self.os,
                        results_dir=os.path.join(result_folder, self.os, self.arch),
                        app_name=config.Get("appName", "lambdaflowApp"),
                        app_version=config.Get("appVersion", "1.0.0"),
                        org_name=config.Get("organizationName", "SimpleLambda"),
                        arch=self.arch
                    )

        finally:
            remove("lambdaflow/TMP/backend.pak")
            remove("lambdaflow/TMP/public.pub")
            remove("lambdaflow/TMP/private.pem")

    def __compile_framework(self, result_name, config, security_mode, mode):
        rid_map = {
            "windows-x86": "win-x86",
            "windows-x64": "win-x64",
            "windows-arm": "win-arm",
            "windows-arm64": "win-arm64",
        
            "linux-x64":   "linux-x64",
            "linux-arm":   "linux-arm",
            "linux-arm64": "linux-arm64",

            "android-x86": "android-x86",
            "android-x64": "android-x64",
            "android-arm": "android-arm",
            "android-arm64": "android-arm64",
        }

        rid = rid_map.get(f"{self}", None)

        if not rid:
            raise RuntimeError(f"Warning: no RID mapping for '{plat}', skipping platform.")

        log(f"RID detected: {rid}", banner_type="info")

        fw_out = Normalize(f"bin/{self.os}/{self.arch}")
        csproj_name = find_first_csproj("")

        targets_map = {
            "windows": "net8.0-windows",
            "linux": "net8.0",
            "android": "net8.0-android",
        }
    
        target = targets_map.get(self.os)

        if not target:
            raise RuntimeError(f"ERROR: No target framework mapping for platform '{plat}'.")

        log(f"Target framework: {target}", banner_type="info")

        run(f"dotnet publish {csproj_name}.csproj -c Release -f {target} -r {rid} -o {fw_out} -p:AssemblyName={result_name} -p:SelfContained={config.Get("selfContainedFramework", True)} -p:PlatformDefine={self.os.upper()} -p:SecurityDefine={security_mode.upper()} {'-p:DebugDefine=DEBUG' if mode == "Debug" else ''}", cwd=Normalize(""))

        if not os.path.isdir(fw_out):
            raise RuntimeError(f"ERROR: Expected framework output in '{fw_out}' but not found.")

        results_forlder = config.Get("resultFolder", "Results")

        copy(f"bin/{self.os}/{self.arch}", f"{results_forlder}/{self.os}/{self.arch}")
        remove(f"bin/{self.os}/{self.arch}")

        copy(f"lambdaflow/TMP/backend.pak", f"{results_forlder}/{self.os}/{self.arch}/backend.pak")
        copy(f"lambdaflow/TMP/frontend.pak", f"{results_forlder}/{self.os}/{self.arch}/frontend.pak")