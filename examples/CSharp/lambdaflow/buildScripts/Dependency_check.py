from Utilities import *
from Developer_info import *

# ----- VARIABLES -----

developer_win_pkgman = ""
developer_win_bash = ""

bash_cmd = None


# ----- METHODS -----

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

def get_bash_cmd():
    if developer_so == "windows":
        if developer_win_bash == "wsl":
            return [r"C:\Windows\System32\wsl.exe", "bash", "-lc"]
        else:
            git_bash = Path(os.environ["ProgramFiles"]) / "Git" / "bin" / "bash.exe"
            return [str(git_bash), "-lc"]

    return ["bash", "-lc"]

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

    print_banner("DEPENDENCY CHECKS", banner_type = "info")

    check_webview_dependency()
    check_win_pckman_dependency()
    check_win_bash_dependency()

    bash_cmd = get_bash_cmd()

    check_nsis_dependency()
