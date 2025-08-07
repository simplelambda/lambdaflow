#!/usr/bin/env python3

import sys, traceback

from Utilities.Utilities       import *
from Utilities.Config          import Config
from Platforms.Platform        import Platform
from Dependencies.Dependencies import Dependencies

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

        dependencies = Dependencies()

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

        # ----- CREATION OF THE RESULTS FOLDER -----

        log(f"Creating results folder {config.Get("resultFolder", "Results")}", banner_type="info")
        mkdir(config.Get("resultFolder", "Results"))
        
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
                platform.Compile(config, mode) 

    except Exception:
        traceback.print_exc()
    finally:
        #remove("lambdaflow/TMP")
        remove("bin")
        remove("obj")

        return

    #if(developer_so == "linux"):
    #    developer_so_dist = obtain_linux_dist()

    #check_dependencies()



if __name__ == "__main__":
    main()