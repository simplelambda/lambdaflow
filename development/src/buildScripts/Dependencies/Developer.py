import platform

class Developer():
    def __init__(self):
        self.developer_so = platform.system().lower()
        self.developer_so_dist = ""

        self.__obtain_developer_info()

    def __eq__(self, other):
        return self.developer_so == other.developer_so and self.developer_so_dist == other.developer_so_dist

    def __str__(self):
        return f"{self.os}-{self.arch}"

    def __obtain_developer_info(self):
        if self.developer_so.startswith("win"): 
            return

        # Only Linux need distribution information

        developer_so_dist = self.__obtain_linux_dist()

    def __obtain_linux_dist(self):
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

    def __obtain_linux_pkg_manager(self):
        if shutil.which("apt-get"):
            return "apt"
        if shutil.which("dnf"):
            return "dnf"
        if shutil.which("yum"):
            return "yum"
        if shutil.which("pacman"):
            return "pacman"
        return None