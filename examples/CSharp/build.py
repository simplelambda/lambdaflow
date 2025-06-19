#!/usr/bin/env python3

import sys, os, platform, subprocess, json, shutil, tempfile, hashlib, zipfile, importlib
import tkinter as tk
from tkinter import messagebox, filedialog
from pathlib import Path
from string import Template

# ----- GLOBAL VARIABLES -----

developer_so = platform.system().lower()
developer_so_dist = ""
developer_win_pkgman = ""
developer_win_bash = ""

bash_cmd = None

# config.json vars

cfg = None

platforms = None
        
result_folder = None
result_name = None

development_backend_folder = None
development_frontend_folder = None

app_name = None
app_version = None
app_ico = None
org_name = None

include_version_in_result_name = None

# ----- AUX METHODS -----

def print_banner(text, size = 50, banner_type = "title"):
    text_size = len(text)
    sub = size - text_size

    if sub < 0:
        print(text)

    if sub % 2 != 0:
        text = text + " "
        sub -= 1

    num = sub // 2

    if banner_type == "title":
        print("\n" + "=" * num + " " + text + " " + "=" * num + "\n")
    elif banner_type == "subtitle":
        print("\n" + "-" * num + " " + text + " " + "-" * num + "\n")
    elif banner_type == "info":
        print(text + "\n")

def obtain_linux_pkg_manager():
    if shutil.which("apt-get"):
        return "apt"
    if shutil.which("dnf"):
        return "dnf"
    if shutil.which("yum"):
        return "yum"
    if shutil.which("pacman"):
        return "pacman"
    return None

def obtain_linux_dist():
    try:
        raw_id = subprocess.check_output(["sed", "-n", "s/^ID=\\(\"\\?\\)\\([^\"].*\\)\\1$/\\2/p", "/etc/os-release"]).decode().strip()
        distro_id = raw_id.lower()

        if distro_id in (
            "debian", "ubuntu", "linuxmint", "elementary", "pop",
            "kali", "raspbian", "devuan", "deepin", "zorin", "peppermint",
            "endless", "neon", "parrot", "kubuntu", "lubuntu", "xubuntu", 
            "ubuntukylin", "ubuntu-mate", "ubuntu-budgie"
        ):
            return "debian"

        if distro_id in (
            "rhel", "redhat", "redhatenterpriseserver",
            "fedora", "centos", "centos-stream",
            "rocky", "almalinux", "oraclelinux", "cloudlinux",
            "amzn", "amazon", "amazonlinux", "amazonlinux2",
            "scientific"
        ):
            return "redhat"

        if distro_id in (
            "arch", "manjaro", "endeavouros", "artix", "archlabs",
            "archcraft", "antergos", "garuda"
        ):
            return "arch"
    except subprocess.CalledProcessError:
        print("Failed to read /etc/os-release. Trying other methods.")


    if Path("/etc/debian_version").exists():
        return "debian"
    if Path("/etc/redhat-release").exists():
        return "redhat"
    if Path("/etc/arch-release").exists():
        return "arch"


    pm = obtain_linux_pkg_manager()
    if pm == "apt":
        return "debian"
    elif pm in ("dnf", "yum"):
        return "redhat"
    elif pm == "pacman":
        return "arch"


    syste.exit("Could not determine Linux distribution. Aborting.")

def run(cmd, cwd=None):
    res = subprocess.run(cmd, shell=isinstance(cmd, str), cwd=cwd)
    if res.returncode != 0:
        raise RuntimeError(f"ERROR: {cmd}")

def ask(title, msg):
    root = tk.Tk()
    root.withdraw()
    return messagebox.askyesno(title, msg)

def sha512(path):
    h = hashlib.sha512()
    with open(path, "rb") as f:
        for chunk in iter(lambda: f.read(8192), b""):
            h.update(chunk)
    return h.hexdigest()

# ----- IMPORT CTYPRODOME FOR SIGN -----

try:
    from Cryptodome.PublicKey import ECC
    from Cryptodome.Signature import DSS
    from Cryptodome.Hash import SHA512
except ImportError:
    if ask("pycryptodome not found", "pycryptodome is required for signing. Do you want to install it now?"):
        subprocess.check_call([
            sys.executable, "-m", "pip", "install", "pycryptodomex"
        ])
    else:
        sys.exit("pycryptodome is neccesary. Aborting.")

    try:
        from Cryptodome.PublicKey import ECC
        from Cryptodome.Signature import DSS
        from Cryptodome.Hash import SHA512
    except ImportError as e:
        sys.exit(f"Error importing Cryptodome after installation: {e}")

# ----- DEPENDENCY CHECK METHODS -----

def has_webview2_windows():
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

def is_nsis_installed():
    if shutil.which("makensis"):
        return True

    if developer_so.startswith("win"):
        candidates = []
        pf86 = os.environ.get("ProgramFiles(x86)")
        pf   = os.environ.get("ProgramFiles")
        if pf86:
            candidates.append(os.path.join(pf86, "NSIS", "makensis.exe"))
        if pf:
            candidates.append(os.path.join(pf,   "NSIS", "makensis.exe"))

        for exe in candidates:
            if os.path.isfile(exe):
                pdir = os.path.dirname(exe)
                return True
    return False

def check_webview_dependency():
    if developer_so == "windows":
        import winreg

        if not has_webview2_windows():
            if ask("Install Webview2?", "To execute its application we need webView2 Runtime. Install now?"):
                url = "https://go.microsoft.com/fwlink/p/?LinkId=2124703"
                tmp = os.path.join(tempfile.gettempdir(), "WebView2Installer.exe")

                print_banner("Downloading WebView2 installer via PowerShell...", banner_type="info")

                ps_cmd = [
                    "powershell", "-NoProfile", "-Command",
                    f"Invoke-WebRequest -Uri '{url}' -OutFile '{tmp}'"
                ]
                run(ps_cmd)

                print_banner("Running WebView2 installer with elevation...", banner_type="info")

                ps_elevate = [
                    "powershell", "-NoProfile", "-Command",
                    f"Start-Process -FilePath '{tmp}' -ArgumentList '/silent','/install' -Verb RunAs -Wait"
                ]
                run(ps_elevate)
            else:
                sys.exit("WebView2 Runtime required. Aborting.")

    elif developer_so == "linux":
        pkg = "webkit2gtk-4.0-dev"

        if subprocess.run(["pkg-config","--exists",pkg]).returncode != 0:
            if ask("Install GTK+ WebKit?", f"Missing {pkg}. Install now?"):
                if  developer_so_dist == "debian":
                    run(["sudo","apt","update"])
                    run(["sudo","apt","install","-y",pkg])
                elif developer_so_dist == "modern-red-hat":
                    run(["sudo","dnf","install","-y",pkg])
                elif developer_so_dist == "old-red-hat":
                    run(["sudo","yum","install","-y",pkg])
                else:
                    sys.exit(f"Package manager not detected. install manually {pkg}.")
            else:
                sys.exit(f"{pkg} required. Aborting.")
    elif developer_so == "darwin":
        print("")
    else:
        print_banner(f"SO not supported: {developer_so}", banner_type="info")
        sys.exit(1)

    print_banner("WebView dependency check passed.", banner_type="info")

def check_win_pckman_dependency():
    global developer_win_pkgman

    if developer_so != "windows":
        return

    if shutil.which("winget"):
        print_banner("Windows package manager dependency check passed.", banner_type="info")
        developer_win_pkgman = "winget"
        return

    if shutil.which("choco"):
        print_banner("Windows package manager dependency check passed.", banner_type="info")
        developer_win_pkgman = "choco"
        return;

    if not ask("Neither Winget nor Chocolatey detected", "Install Chocolatey now?"):
        system.exit("Windows package manager is required for building Windows installers. Aborting.")

    print_banner("Downloading Chocolatey via PowerShell...", banner_type="info")

    ps_cmd = [
        "powershell", "-NoProfile", "-InputFormat", "None",
        "-ExecutionPolicy", "Bypass", "-Command",
        "Set-ExecutionPolicy Bypass -Scope Process;"
        "[System.Net.ServicePointManager]::SecurityProtocol = "
        "[System.Net.ServicePointManager]::SecurityProtocol -bor 3072;"
        "iex ((New-Object System.Net.WebClient).DownloadString("
        "'https://chocolatey.org/install.ps1'))"
    ]
    try:
        subprocess.check_call(ps_cmd, shell=False)
    except subprocess.CalledProcessError as e:
        sys.exit(f"Error installing Chocolatey: {e}")

    if shutil.which("choco"):
        print_banner("Windows package manager dependency check passed.", banner_type="info")
        developer_win_pkgman = "choco"
        return;
    else:
        sys.exit("Chocolatey installation failed. Aborting.")   

def check_win_bash_dependency():
    global developer_win_bash
    global bash_cmd
    
    if developer_so != "windows":
        if not shutil.which("bash"):
            system.exit("Bash not found. Aborting.")
        else:
            return

    windir = os.environ.get("WINDIR", r"C:\Windows")
    wsl_exe = Path(windir) / "System32" / "wsl.exe"
    if wsl_exe.is_file():
        print_banner("Windows bash dependency check passed.", banner_type="info")
        developer_win_bash = "wsl"
        return
        
    git_bash = Path(os.environ.get("ProgramFiles", r"C:\Program Files")) / "Git" / "bin" / "bash.exe"
    if git_bash.is_file():
        print_banner("Windows bash dependency check passed.", banner_type="info")
        developer_win_bash = "git"
        return

    if not ask("Git bash not detected","Install Git for Windows now?"):
        sys.exit("Bash required. Aborting.")

    if developer_win_pkgman == "winget":
        cmd = ["winget", "install", "--id", "Git.Git", "-e", "--silent"]
    else:
        cmd = ["choco", "install", "git", "-y"]

    subprocess.check_call(cmd, shell=False)

    if git_bash.is_file():
        print_banner("Windows bash dependency check passed.", banner_type="info")
        developer_win_bash = "git"
        return

    sys.exit("Error installing Git for Windows. Aborting.")

def check_nsis_dependency():
    if is_nsis_installed():
        print_banner("NSIS dependency check passed.", banner_type="info")
        return

    if not ask("NSIS not found", "Do you want to install it now?"):
        system.exit("NSIS is required for building Windows installers. Aborting.")

    if developer_so == "windows":
        if developer_win_pkgman == "winget":
            cmds = ["winget install --id NSIS.NSIS -e --silent"]
        elif developer_win_pkgman == "choco":
            cmds = ["choco install nsis -y"]
        else:
            print_banner("Windows package manager not detected trying Winget.", banner_type="info")
            cmds = ["winget install --id NSIS.NSIS -e --silent"]
    elif developer_so == "darwin":
        cmds = ["brew install makensis"]
    else:  # Linux
        if developer_so_dist == "debian":
            cmds = ["sudo apt-get update", "sudo apt-get install -y nsis"]
        elif developer_so_dist == "redhat":
            cmds = ["sudo dnf install -y nsis || sudo yum install -y nsis"]
        elif developer_so_dist == "arch":
            cmds = ["sudo pacman -Sy --noconfirm nsis"]
        else:
            system.exit("Package manager not detected. Install NSIS manually. Aborting")

    for c in cmds:
        subprocess.check_call(c, shell=True)

    print_banner("NSIS dependency check passed.", banner_type="info")

def check_dependencies():
    global bash_cmd

    print_banner("DEPENDENCY CHECKS")

    check_webview_dependency()
    check_win_pckman_dependency()
    check_win_bash_dependency()

    bash_cmd = get_bash_cmd()

    check_nsis_dependency()

# ----- METHODS -----

def load_config():
    global cfg
    global platforms
    global result_folder, result_name
    global development_backend_folder, development_frontend_folder
    global app_name, app_version, org_name, app_ico

    if os.path.exists("config.dev.json"):
        if os.path.exists("config.json"):
            os.remove("config.json")
            os.rename("config.dev.json", "config.json")
        else:
            os.rename("config.dev.json", "config.json")

    os.rename("config.json", "config.dev.json")

    with open("config.dev.json", encoding="utf-8") as f:
        cfg = json.load(f)

    platforms = cfg.get("platforms", {})

    if not platforms:
        os.rename("config.dev.json", "config.json")
        sys.exit("ERROR: No 'platforms' section in config.json.")
        
    result_folder = cfg.get("resultFolder", "Results")

    development_backend_folder = cfg.get("developmentBackendFolder", "backend")
    development_frontend_folder = cfg.get("developmentFrontendFolder", "frontend")

    app_name = cfg.get("appName", "LambdaFlowApp")
    app_version = cfg.get("appVersion", "1.0.0")
    app_ico = cfg.get("appIcon", "app.ico")
    org_name = cfg.get("organizationName", "SimpleLambda")

    include_version_in_result_name = cfg.get("includeVersionInResultName", True)

    result_name = f"{app_name}-v{app_version}" if include_version_in_result_name else app_name

def get_bash_cmd():
    if developer_so == "windows":
        if developer_win_bash == "wsl":
            return [r"C:\Windows\System32\wsl.exe", "bash", "-lc"]
        else:
            git_bash = Path(os.environ["ProgramFiles"]) / "Git" / "bin" / "bash.exe"
            return [str(git_bash), "-lc"]

    return ["bash", "-lc"]

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

def get_nsis_executable():
    exe = shutil.which("makensis")
    if exe:
        return exe

    for base in (os.environ.get("ProgramFiles(x86)"), os.environ.get("ProgramFiles")):
        if base:
            candidate = Path(base) / "NSIS" / "makensis.exe"
            if candidate.is_file():
                return str(candidate)

    raise FileNotFoundError("makensis.exe not found. Please ensure NSIS is installed and in your PATH.")

def build_windows_installer(results_dir, app_name, app_version, org_name, arch):
    tname = "installer_x86.nsi.template" if arch == "x86" else "installer_x64.nsi.template"
    tpl = Path(os.path.join("lambdaflow", "NSIS", tname)).read_text()
    filled = (
        tpl
        .replace("${APP_NAME}",    app_name)
        .replace("${APP_VERSION}", app_version)
        .replace("${ORG_NAME}",    org_name)
        .replace("${SRC_DIR}",     results_dir)
        .replace("${APP_ICO}",        app_ico)
    )

    Path("installer.nsi").write_text(filled, encoding="utf-8")

    try:
        nsis_exe = get_nsis_executable()
    except FileNotFoundError as e:
        sys.exit(f"ERROR: {e}")

    run([nsis_exe, "installer.nsi"])

    os.remove('installer.nsi')
    os.makedirs(os.path.join(results_dir, "installer"), exist_ok=True)
    shutil.move(
        os.path.join(results_dir, f"{app_name}-{app_version}-win{arch}--installer.exe"),
        os.path.join(results_dir, "installer", f"{app_name}-{app_version}-win{arch}--installer.exe")
    )

def build_unix_installer(target, results_dir, app_name, app_version, org_name, arch):
    tpl_text = Path(os.path.join("lambdaflow", "makeself", "install.sh.template")).read_text(encoding="utf-8")
    tpl = Template(tpl_text )  
    content = tpl.safe_substitute(
        APP=app_name,
        ORG=org_name,
        VER=app_version,
        ARCH=arch
    )

    results_dir = Path(results_dir)
    unix_results_dir = results_dir.as_posix()
    script = results_dir / "install.sh"
    content = content.replace('\r\n', '\n')
    script.write_text(content, encoding='utf-8', newline='\n')
    script.chmod(0o755)

    out_run = f"{app_name}-{app_version}-{target}-{arch}.run"

    run([
        *bash_cmd,
        f"lambdaflow/makeself/makeself.sh {unix_results_dir} {out_run} \"{app_name} Installer ({target})\" ./install.sh"
    ])
    

    installer_dir = results_dir / "installer"
    installer_dir.mkdir(exist_ok=True)

    shutil.move(str(out_run), str(installer_dir / out_run))
    os.remove(script)

# ----- MAIN FUNCTION -----

def main():
    global developer_so_dist

    if(developer_so == "linux"):
        developer_so_dist = obtain_linux_dist()

    check_dependencies()

    print_banner("PRECOMPILING TASKS")

    # ----- READ CONFIG.JSON -----

    print_banner("Loading config from config.json", banner_type="info")

    load_config()

    # ----- CREATION OF THE RESULTS FOLDER -----

    print_banner(f"Creating results folder {result_folder}", banner_type="info")

    result_root = os.path.abspath(result_folder)
    if os.path.isdir(result_root):
        shutil.rmtree(result_root)

    os.makedirs(result_root, exist_ok=True)

    # ----- CLEAN ROOT bin / obj FOLDERS -----

    print_banner("Cleaning framework folders", banner_type="info")

    for d in ("bin", "obj"):
        if os.path.isdir(d):
            shutil.rmtree(d, ignore_errors=True)

    # ----- PREBUILD SNAPSHOT IN CASE OF FAILURE -----

    print_banner("Saving backend status...", banner_type="info")

    backend_cwd = os.path.abspath(development_backend_folder)
    initial_backend = set()
    for root, dirs, files in os.walk(backend_cwd):
        for name in dirs + files:
            rel = os.path.relpath(os.path.join(root, name), backend_cwd)
            initial_backend.add(rel)

    # ----- PACKAGE FRONTEND INTO PAK -----

    print_banner('Packaging frontend into frontend.pak...', banner_type="info")

    fr_pak = 'frontend.pak'
    if os.path.exists(fr_pak): os.remove(fr_pak)
    with zipfile.ZipFile(fr_pak, 'w', zipfile.ZIP_DEFLATED) as zf:
        for root, dirs, files in os.walk(development_frontend_folder):
            for fn in files:
                abs_fn = os.path.join(root, fn)
                rel = os.path.relpath(abs_fn, development_frontend_folder)
                zf.write(abs_fn, rel)

    # ----- CREATION OF NEW CONFIG.JSON -----

    cfg_path = Path("config.dev.json")
    with cfg_path.open("r", encoding="utf-8") as f:
        cfg = json.load(f)

    to_remove = ["platforms", "developmentBackendFolder", "developmentFrontendFolder", "resultFolder"]

    for key in to_remove:
        cfg.pop(key, None)

    embed_path = Path("config.json")
    with embed_path.open("w", encoding="utf-8") as f:
        json.dump(cfg, f, indent=2, ensure_ascii=False)

    # ----- COMPILATION OF EACH PLATFORM -----

    print_banner("Compiling for each platform")

    try:
        for plat, info in platforms.items():
            archs = info.get("archs", {})

            if not archs:
                archs = {"x64": {}}

            for arch, arch_cfg in archs.items():

                if arch_cfg.get("ignore", False):
                    print_banner(f"Skipping {plat}-{arch}: ignored in config.json.", banner_type="info")
                    continue

                print_banner(f"Compiling for platform: {plat}-{arch}", banner_type="subtitle")

                cmd = arch_cfg.get("compileCommand")
                if not cmd:
                    print_banner(f"Skipping {plat}-{arch}: no compileCommand specified.", banner_type="info")
                    continue

                # ----- BACKEND COMPILATION -----

                print_banner("Compiling backend...", banner_type="info")

                run(cmd, cwd=backend_cwd)
                out_backend = os.path.join(backend_cwd, "bin")
                if not os.path.isdir(out_backend):
                    raise RuntimeError(f"ERROR: Expected backend output in '{out_backend}' but not found.")

                # ----- PACKAGE BACKEND INTO PAK -----

                print_banner('Packaging backend into backend.pak...', banner_type="info")

                be_pak = "backend.pak"
                shutil.rmtree(be_pak, ignore_errors=True)
                with zipfile.ZipFile(be_pak, 'w', zipfile.ZIP_DEFLATED) as zf:
                    for root,_,files in os.walk(out_backend):
                        for fn in files:
                            abs_fn = os.path.join(root, fn)
                            rel   = os.path.relpath(abs_fn, out_backend)
                            zf.write(abs_fn, rel)

                # ----- REMOVE BACKEND COMPILATION RESULTS -----

                print_banner("Removing backend compilation results...", banner_type="info")

                shutil.rmtree(out_backend)

                # ----- CREATION OF INTEGRITY.JSON -----

                print_banner("Generating integrity.json...", banner_type="info")

                manifest = {
                    "backend.pak": sha512(be_pak),
                    "frontend.pak": sha512(fr_pak)
                }

                with open("integrity.json", "w", encoding="utf-8") as f:
                    json.dump(manifest, f, indent=2)

                # ----- SIGN INTEGRITY.JSON WITH ECDSA/P-256 + SHA-512 -----

                print_banner("Signing integrity.json...", banner_type="info")

                key = ECC.generate(curve='P-256')

                pub = key.public_key().export_key(format='PEM')
                with open('public.pem','wt') as f:
                    f.write(pub)

                data = open('integrity.json','rb').read()
                h = SHA512.new(data)
                signer = DSS.new(key, 'fips-186-3')
                sig = signer.sign(h)

                with open('integrity.sig','wb') as f:
                    f.write(sig)

                # ----- FRAMEWORK COMPILATION -----

                rid_map = {
                    "windows-x64": "win-x64",
                    "windows-x86": "win-x86",
                    "linux-x64":   "linux-x64",
                    "linux-arm64": "linux-arm64",
                    "mac-x64":     "osx-x64",
                    "mac-arm64":   "osx-arm64",
                }

                rid = rid_map.get(plat + "-" + arch, None)

                if not rid:
                    print(f"  Warning: no RID mapping for '{plat}', skipping platform.")
                    continue

                print_banner("Compiling framework...", banner_type="info")

                fw_out = os.path.join("bin", plat, "framework")
                run(f"dotnet publish lambdaflow.csproj -c Release -r {rid} -o {fw_out} -p:AssemblyName={result_name}", cwd=os.getcwd())

                out_framework = os.path.abspath(fw_out)
                if not os.path.isdir(out_framework):
                    raise RuntimeError(f"ERROR: Expected framework output in '{out_framework}' but not found.")

                # ----- COPY FILES INTO RESULT FOLDER -----

                dst_base = os.path.join(result_root, plat, arch)

                print_banner("Assembling results...", banner_type="info")

                # framework
                copy_contents(out_framework, dst_base)

                # backend
                os.makedirs(dst_base, exist_ok=True)
                shutil.copy2("backend.pak", os.path.join(dst_base, "backend.pak"))

                # frontend
                os.makedirs(dst_base, exist_ok=True)
                shutil.copy2("frontend.pak", os.path.join(dst_base, "frontend.pak"))

                # integrity

                shutil.copy2("integrity.sig", os.path.join(dst_base, "integrity.sig"))
                shutil.copy2("public.pem", os.path.join(dst_base, "public.pem"))

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


                # ----- CLEAN UP -----

                print_banner("Cleaning up intermediate build directories...", banner_type="info")

                # Clean backend residual folders

                cleanup(backend_cwd, initial_backend)

                # Clean framework residual folders

                shutil.rmtree(os.path.join("bin"))
                shutil.rmtree(os.path.join("obj"))
                os.remove("integrity.json")
                os.remove('integrity.sig')    
                os.remove('public.pem')
                os.remove('backend.pak')
                
        os.remove('config.json')
        os.remove('frontend.pak')
        os.rename("config.dev.json", "config.json")

    except Exception as e:      
        print(f"\n{e}")
        print("Aborting compilation and rolling back changes...")

        cleanup(backend_cwd, initial_backend)

        if os.path.isdir(os.path.join("bin")):
            shutil.rmtree(os.path.join("bin"))

        if os.path.isdir(os.path.join("obj")):
            shutil.rmtree(os.path.join("obj"))

        for arte in (
            "integrity.json",
            "integrity.sig",
            "public.pem",
            "backend.pak",
            "frontend.pak"
        ):
            if os.path.exists(arte):
                try:
                    os.remove(arte)
                except Exception:
                    pass

        sys.exit(1)

if __name__ == "__main__":
    main()