import os
import shutil
import sys
from pathlib import Path

from Utilities.Utilities import *

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

def build_windows_installer(config, result_name, results_dir, app_name, app_version, org_name, install_dir, arch):
    tname = "installer_x86.nsi.template" if arch == "x86" else "installer_x64.nsi.template"
    tpl = Path(os.path.join("lambdaflow", "NSIS", tname)).read_text()
    filled = (
        tpl
        .replace("${APP_NAME}",    app_name)
        .replace("${APP_VERSION}", app_version)
        .replace("${ORG_NAME}",    org_name)
        .replace("${SRC_DIR}",     results_dir)
        .replace("${APP_ICO}",        config.Get("appIcon", "app.ico"))
        .replace("${MACRO_LICENSE}",  "!insertmacro MUI_PAGE_LICENSE" if config.Get("addLicenseToInstaller", True) else "")
        .replace("${LICENSE_FILE}",   config.Get("licenseFile", "license.txt") if config.Get("addLicenseToInstaller", True) else "")
        .replace("${EXE_NAME}", f"{result_name}.exe")
        .replace("${INSTALL_DIR}", install_dir)
    )

    print(filled)

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
        ARCH=arch,
        RESULT_NAME=result_name
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
