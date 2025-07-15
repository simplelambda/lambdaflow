#!/usr/bin/env python3

from Utilities import *
from Dependency_check import *

from SecurityStrategies.Minimal import *
from SecurityStrategies.Integrity import *
from SecurityStrategies.Hardened import *


# ----- COMPILATION METHODS ------

def compile_platform(config, plat, info):
    if info.get("ignore", False):
        print_banner(f"Skipping {plat}: ignored in config.json.", banner_type="info")
        return

    for arch, arch_cfg in info.get("archs", {}).items():
        compile_arch(config, plat, arch, arch_cfg)

def compile_arch(config, plat, arch, arch_cfg):
    try:
        if arch_cfg.get("ignore", False):
            print_banner(f"Skipping {plat}-{arch}: ignored in config.json.", banner_type="info")
            return

        print_banner(f"Compiling for platform: {plat}-{arch}", banner_type="subtitle")

        cmd = arch_cfg.get("compileCommand", None)
        if not cmd:
            print_banner(f"Skipping {plat}-{arch}: no compileCommand specified.", banner_type="info")
            return


        # ----- BACKEND COMPILATION -----

        print_banner("Compiling backend...", banner_type="info")

        backend_cwd = normalizePath(f"lambdaflow/TMP/{config.get("developmentBackendFolder", "backend")}")

        run(cmd, cwd=backend_cwd)

        out_backend = normalizePath(f"lambdaflow/TMP/{config.get("developmentBackendFolder", "backend")}/{arch_cfg.get("compileDirectory", "bin")}")

        if not os.path.isdir(out_backend):
            raise RuntimeError(f"ERROR: Expected backend output in '{out_backend}' but not found.")


        # ----- PACKAGE BACKEND INTO PAK -----

        print_banner('Packaging backend into backend.pak...', banner_type="info")

        pakFolder(out_backend, "lambdaflow/TMP/backend.pak")


        # ----- KEY GENERATION -----

        print_banner('Generating keys for signing', banner_type="info")

        generate_key_pair("lambdaflow/TMP/public.pub", "lambdaflow/TMP/private.pem")
    
        # ----- FRAMEWOKS CODE MODIFICATION -----

        print_banner("Modifying framework code for platform and security mode", banner_type="info")

        if plat.startswith("windows") and arch_cfg.get("useAuthenticode", False):
            inject_global_variable("lambdaflow/TMP/lambdaflow/source/Security/Signers/WindowsSigner.cs", "useAuthenticode", "true")

        plats = ["android", "windows", "linux"]
        folders = ["Security/Protectors", "Security/Signers", "WebView/PlatformWebViews", "IPCBridge/PlatformIPCBridge"]
        plats.remove(plat)

        for folder in folders:  
            path = normalizePath(f"lambdaflow/TMP/lambdaflow/source/{folder}")
            for file in os.listdir(path):
                if file.startswith(plat.capitalize()):
                    if file.endswith(".csx"):
                        rename(f"{path}/{file}", file.replace(".csx", ".cs"))
                        print(f"{file} renamed to {file.replace(".csx", ".cs")}")
                else:
                    for other_plat in plats:
                        if file.startswith(other_plat.capitalize()) and file.endswith(".cs"):
                            rename(f"{path}/{file}", file.replace(".cs", ".csx"))


        securityMode = config.get("securityMode", "Integrity")

        if securityMode == "Minimal":
            minimal_modify_framework(plat)
        elif securityMode == "Integrity":
            integrity_modify_framework(plat)
        elif securityMode == "Hardened":
            hardened_modify_framework(plat)
        else:
            raise ValueError(f"Unknown security mode: {securityMode}")

        # ----- FRAMEWORK COMPILATION -----

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

        rid = rid_map.get(plat + "-" + arch, None)

        if not rid:
            print(f"  Warning: no RID mapping for '{plat}', skipping platform.")
            return

        print_banner("Compiling framework...", banner_type="info")

        fw_out = normalizePath(f"bin/{plat}/{arch}/lambdaflow")
        csproj_name = find_first_csproj("")
        result_name = f"{config.get("appName", "lambdaflowApp")}-v{config.get("appVersion", "1.0.0")}" if config.get("includeVersionInResultName", True) else config.get("appName", "lambdaflowApp")
    
        targets_map = {
            "windows": "net8.0-windows",
            "linux": "net8.0",
            "android": "net8.0-android",
        }
    
        target = targets_map.get(plat)

        run(f"dotnet publish {csproj_name}.csproj -c Release -f {target} -r {rid} -o {fw_out} -p:AssemblyName={result_name} -p:SelfContained={config.get("selfContainedFramework", True)} -p:PlatformDefine={plat.upper()} -p:SecurityDefine={config.get("securityMode", "Integrity").upper()}", cwd=normalizePath(""))

        if not os.path.isdir(fw_out):
            raise RuntimeError(f"ERROR: Expected framework output in '{fw_out}' but not found.")

        results_forlder = config.get("resultFolder", "Results")

        copy(f"bin/{plat}/{arch}/lambdaflow", f"{results_forlder}/{plat}/{arch}")
        remove(f"bin/{plat}/{arch}/lambdaflow")

        copy(f"lambdaflow/TMP/backend.pak", f"{results_forlder}/{plat}/{arch}/backend.pak")
        copy(f"lambdaflow/TMP/frontend.pak", f"{results_forlder}/{plat}/{arch}/frontend.pak")

        # ------ SIGN EXECUTABLE -----

        print_banner("Signing executable", banner_type="info")

        sign([f"{results_forlder}/{plat}/{arch}/{result_name}" + (".exe" if plat.startswith("windows") else ""),
              f"{results_forlder}/{plat}/{arch}/{result_name}.dll"], 
              "lambdaflow/TMP/private.pem", 
              f"{results_forlder}/{plat}/{arch}/integrity.sig"
        )
            
         
    finally:
        remove("lambdaflow/TMP/backend.pak")
        remove("lambdaflow/TMP/integrity.json")
        remove("lambdaflow/TMP/public.pub")
        remove("lambdaflow/TMP/private.pem")


# ----- MAIN FUNCTION -----

def main():
    try:

        mode = sys.argv[1]
        csproj_name = find_first_csproj()

        print(f"Compiling in {mode} mode")

        print_banner("PREPARING ENVIRONMENT")

        # ----- CHECK DEPENDENCIES -----

        check_dependencies()

        # ----- CLEAN ROOT BIN AND OBJ -----

        remove("bin")
        remove("obj")

        # ----- LOAD CONFIG.JSON -----

        print_banner(f"Loading config.json", banner_type="info")

        config = load_config("config.json")

      
        # ----- CREATE TMP FOLDER FOR COMPILE -----

        print_banner(f"Creating TMP folder for compiling", banner_type="info")

        remove("lambdaflow/TMP")

        copy("lambdaflow/source", "lambdaflow/TMP/lambdaflow/source")
        copy(f"{config.get("developmentBackendFolder", "backend")}", "lambdaflow/TMP/backend")
        copy(f"config.json", "lambdaflow/TMP/config.json")


        # ----- MODIFY TMP CONFIG.JSON -----

        print_banner(f"Modifying copy of config.json", banner_type="info")

        modify_json("lambdaflow/TMP/config.json", ["platforms", "developmentBackendFolder", "developmentFrontendFolder", "resultFolder", "selfContainedFramework", "addLicenseToInstaller", "licenseFile", "securityMode"])


        # ----- CREATION OF THE RESULTS FOLDER -----

        print_banner(f"Creating results folder {config.get("resultFolder", "Results")}", banner_type="info")

        mkdir(config.get("resultFolder", "Results"))
            

        # ----- PACKAGE FRONTEND INTO PAK -----

        print_banner('Packaging frontend into frontend.pak...', banner_type="info")

        pakFolder("frontend", "lambdaflow/TMP/frontend.pak")


        # ----- COMPILATION OF EACH PLATFORM -----

        print_banner("Compiling for each platform")

        for plat, info in config.get("platforms", {}).items():
            compile_platform(config, plat, info)


    except Exception as e:
        print(f"Error during initial setup: {e}")
    finally:
        #remove("lambdaflow/TMP")
        remove("bin")
        remove("obj")

        return







    if(developer_so == "linux"):
        developer_so_dist = obtain_linux_dist()

    check_dependencies()

    print_banner("PRECOMPILING TASKS")




    # ----- CLEAN ROOT bin / obj FOLDERS -----

    print_banner("Cleaning framework folders", banner_type="info")

    for d in ("bin", "obj"):
        if os.path.isdir(d):
            shutil.rmtree(d, ignore_errors=True)


    # ----- COMPILATION OF EACH PLATFORM -----

    print_banner("Compiling for each platform")
    
    for plat, info in platforms.items():
        archs = info.get("archs", {})

        if not archs:
            archs = {"x64": {}}

        for arch, arch_cfg in archs.items():

                
                


               
            # ----- PACKAGE PROGRAM -----

            package = arch_cfg.get("package", False)

            if package:
                print_banner(f"Packaging installer", banner_type="info")

                if plat == "windows":    
                    build_windows_installer(
                        results_dir=os.path.join(result_folder, plat, arch),
                        app_name=app_name,
                        app_version=app_version,
                        org_name=org_name,
                        arch=arch
                    )
                else:
                    build_unix_installer(
                        target=plat,
                        results_dir=os.path.join(result_folder, plat, arch),
                        app_name=app_name,
                        app_version=app_version,
                        org_name=org_name,
                        arch=arch
                    )


if __name__ == "__main__":
    main()