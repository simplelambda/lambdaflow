import sys, os, platform, subprocess, json, shutil, tempfile, hashlib, zipfile, importlib
import tkinter as tk
from tkinter import messagebox, filedialog
from pathlib import Path
from string import Template

import Utilities

# ----- VARIABLES -----

developer_so = platform.system().lower()
developer_so_dist = ""


# ----- METHODS -----

def obtain_developer_info():
    if not developer_so.startswith("win"):
        developer_so_dist = obtain_linux_dist()

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