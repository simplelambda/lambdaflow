#!/usr/bin/env python3

import sys, traceback

from Utilities.Utilities import *
from Utilities.Config    import Config
from Platforms.Platform  import Platform

def main():
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

        copy("lambdaflow/source", "lambdaflow/TMP/lambdaflow/source")
        copy(f"{config.Get("developmentBackendFolder", "backend")}", "lambdaflow/TMP/backend")
        copy(f"config.json", "lambdaflow/TMP/config.json")

        # ----- CREATION OF THE RESULTS FOLDER -----

        log(f"Creating results folder {config.Get("resultFolder", "Results")}", banner_type="info")
        mkdir(config.Get("resultFolder", "Results"))
            

        # ==============================================
        # ----------  APPLICATION COMPILATION ----------
        # ==============================================

        log("APPLICATION COMPILATION")

        # ----- PACKAGE FRONTEND INTO PAK -----

        log('Packaging frontend into frontend.pak...', banner_type="info")
        pakFolder("frontend", "lambdaflow/TMP/frontend.pak")

        # ----- COMPILATION OF EACH PLATFORM -----

        log('Compiling framework and backend for each platform', banner_type="subtitle")

        for os, os_info in config.Get("platforms", {}).items():
            for arch, arch_info in config.Get(f"platforms.{os}.archs", {}).items():
                platform = Platform(os, arch)
                platform.Compile(config) 

    except Exception:
        traceback.print_exc()
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