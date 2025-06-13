#!/usr/bin/env python3

import sys, os, platform, subprocess, json, shutil, tempfile, urllib.request, webbrowser
import tkinter as tk
from tkinter import messagebox, filedialog

def run(cmd, cwd=None):
    print(">>>", cmd)
    res = subprocess.run(cmd, shell=isinstance(cmd, str), cwd=cwd)
    if res.returncode != 0:
        sys.exit(f"ERROR: {cmd}")

def ask(title, msg):
    root = tk.Tk()
    root.withdraw()
    return messagebox.askyesno(title, msg)

def copy_contents(src_dir, dst_dir):
    if os.path.exists(dst_dir):
        shutil.rmtree(dst_dir)

    os.makedirs(dst_dir, exist_ok=True)

    for item in os.listdir(src_dir):
        s = os.path.join(src_dir, item)
        d = os.path.join(dst_dir, item)
        if os.path.isdir(s):
            shutil.copytree(s, d)
        else:
            shutil.copy2(s, d)

def cleanup(base_dir, baseline):
    # Obtain current state of the directory

    current = set()
    for root, dirs, files in os.walk(base_dir):
        for name in dirs + files:
            rel = os.path.relpath(os.path.join(root, name), base_dir)
            current.add(rel)


    # Obtain de diff between current and baseline

    to_delete = current - baseline

    # Delete the diff

    for rel in sorted(to_delete, key=lambda p: p.count(os.sep), reverse=True):
        path = os.path.join(base_dir, rel)
        try:
            if os.path.isdir(path):
                shutil.rmtree(path)
            else:
                os.remove(path)
        except Exception:
            pass

def check_dependencies():
    SO = platform.system()
    print("SO detected:", SO)

    if SO == "Windows":
        import winreg

        def has_webview2():
            reg_paths = [
                r"HKLM\SOFTWARE\Microsoft\EdgeUpdate\Clients",
                r"HKLM\SOFTWARE\WOW6432Node\Microsoft\EdgeUpdate\Clients",
                r"HKCU\SOFTWARE\Microsoft\EdgeUpdate\Clients"
            ]

            for path in reg_paths:
                try:
                    result = subprocess.run(
                        ["reg", "query", path, "/s"],
                        capture_output=True, text=True, shell=False
                    )
                except FileNotFoundError:
                    break

                if result.returncode == 0 and "webview2" in result.stdout.lower():
                    return True

            return False

        if not has_webview2():
            if ask("Install Webview2?", "To execute its application we need webView2 Runtime. Install now?"):
                url = "https://go.microsoft.com/fwlink/p/?LinkId=2124703"
                tmp = os.path.join(tempfile.gettempdir(), "WebView2Installer.exe")

                print("Downloading WebView2 installer via PowerShell...")

                ps_cmd = [
                    "powershell", "-NoProfile", "-Command",
                    f"Invoke-WebRequest -Uri '{url}' -OutFile '{tmp}'"
                ]
                run(ps_cmd)

                print("Running WebView2 installer with elevation...")

                ps_elevate = [
                    "powershell", "-NoProfile", "-Command",
                    f"Start-Process -FilePath '{tmp}' -ArgumentList '/silent','/install' -Verb RunAs -Wait"
                ]
                run(ps_elevate)
            else:
                sys.exit("WebView2 Runtime required. Aborting.")

    elif SO == "Linux":
        if subprocess.run(["pkg-config","--exists","webkit2gtk-4.0"]).returncode != 0:
            if ask("Install GTK+ WebKit?", "Missing libwebkit2gtk-4.0. Install via apt?"):
                run(["sudo","apt","update"])
                run(["sudo","apt","install","-y","libwebkit2gtk-4.0-dev"])
            else:
                sys.exit("libwebkit2gtk-4.0-dev required. Aborting.")

    else:
        print("SO not soported:", SO)
        sys.exit(1)

def main():
    check_dependencies()

    # ----- READ CONFIG.JSON -----

    with open("config.json", encoding="utf-8") as f:
        cfg = json.load(f)

    platforms = cfg.get("platforms", {})
    if not platforms:
        sys.exit("ERROR: No 'platforms' section in config.json.")
        
    result_folder = cfg.get("resultFolder", "Results")
    result_backend_folder = cfg.get("backendResultFolderName", "bck")
    result_frontend_folder = cfg.get("frontendResultFolderName", "frnt")

    development_backend_folder = cfg.get("developmentBackendFolder", "backend")
    development_frontend_folder = cfg.get("developmentFrontendFolder", "frontend")

    app_name = cfg.get("appName", "LambdaFlowApp")
    app_version = cfg.get("appVersion", "1.0.0")

    include_version_in_result_name = cfg.get("includeVersionInResultName", "true") == "true"

    result_name = f"{app_name}-v{app_version}" if include_version_in_result_name else app_name

    # ----- CREATION OF THE RESULTS FOLDER -----

    result_root = os.path.abspath(result_folder)
    if os.path.isdir(result_root):
        shutil.rmtree(result_root)

    os.makedirs(result_root, exist_ok=True)

    # ----- CLEAN ROOT bin / obj FOLDERS -----

    for d in ("bin", "obj"):
        if os.path.isdir(d):
            shutil.rmtree(d, ignore_errors=True)

    # ----- COMPILATION OF EACH PLATFORM -----

    for plat, info in platforms.items():
        print(f"\n----- Processing platform: {plat} -----")

        cmd = info.get("compileCommand")
        if not cmd:
            print(f"Skipping {plat}: no compileCommand specified.")
            continue

        # ----- BACKEND COMPILATION -----

        backend_cwd = os.path.join(os.getcwd(), development_backend_folder)

        # Save current state of backend folder

        baseline = set()
        for root, dirs, files in os.walk(backend_cwd):
            for name in dirs + files:
                rel = os.path.relpath(os.path.join(root, name), backend_cwd)
                baseline.add(rel)


        print(f"Compiling backend for '{plat}'...")

        run(cmd, cwd=backend_cwd)
        out_backend = os.path.join(backend_cwd, "bin")
        if not os.path.isdir(out_backend):
            sys.exit(f"ERROR: Expected backend output in '{out_backend}' but not found.")

        # ----- FRAMEWORK COMPILATION -----

        rid_map = {
            "windows": "win-x64",
            "linux":   "linux-x64",
            "mac":     "osx-x64"
        }

        rid = rid_map.get(plat)

        if not rid:
            print(f"  Warning: no RID mapping for '{plat}', skipping platform.")
            continue

        print(f"Compiling framework for '{plat}'...")

        fw_out = os.path.join("bin", plat, "framework")
        run(f"dotnet publish lambdaflow.csproj -c Release -r {rid} -o {fw_out} -p:AssemblyName={result_name}", cwd=os.getcwd())

        out_framework = os.path.abspath(fw_out)
        if not os.path.isdir(out_framework):
            sys.exit(f"ERROR: Expected framework output in '{out_framework}' but not found.")

        # ----- COPY FILES INTO RESULT FOLDER -----

        dst_base = os.path.join(result_root, plat)

        print(f"Assembling RESULTS -> {dst_base}/{{{result_backend_folder},{result_frontend_folder}}}")

        # framework
        copy_contents(out_framework, dst_base)

        # backend
        copy_contents(out_backend, os.path.join(dst_base, result_backend_folder))

        # frontend
        copy_contents(os.path.join(os.getcwd(), development_frontend_folder),
                      os.path.join(dst_base, result_frontend_folder))

        # ----- CLEAN UP -----

        print("Cleaning up intermediate build directories...")

        # Clean backend residual folders

        shutil.rmtree(out_backend)
        cleanup(backend_cwd, baseline)


        # Clean framework residual folders

        shutil.rmtree(os.path.join("bin"))
        shutil.rmtree(os.path.join("obj"))

if __name__ == "__main__":
    main()