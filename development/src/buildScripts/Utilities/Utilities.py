import shutil, json, zipfile, os, subprocess, hashlib, re, secrets, asyncio, sys

import tkinter as tk
from tkinter import messagebox, filedialog

from pathlib import Path
from types import SimpleNamespace
from cryptography.hazmat.primitives.ciphers.aead import AESGCM

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


# ----- PATH TO THE PYTHON SCRIPTS -----

HERE = Path(__file__).resolve().parents[3]


# ----- UTILITIES METHODS -----

def log(text, size = 75, banner_type = "title"):
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

def ask(title, msg):
    root = tk.Tk()
    root.withdraw()
    return messagebox.askyesno(title, msg)

def run(cmd, cwd=None):
    res = subprocess.run(cmd, shell=isinstance(cmd, str), cwd=cwd)
    if res.returncode != 0:
        raise RuntimeError(f"ERROR: {cmd}")

async def run_async(cmd, cwd=None):
    proc = await asyncio.create_subprocess_shell(
        cmd,
        cwd = cwd,
        stdout = sys.stdout,
        stderr = sys.stderr
    )
    ret = await proc.wait()
    if ret:
        raise RuntimeError(f"ERROR: {cmd}")

def sha512(path):
    h = hashlib.sha512()
    with open(path, "rb") as f:
        for chunk in iter(lambda: f.read(8192), b""):
            h.update(chunk)
    return h.hexdigest()

def encrypt_file_aes_gcm(key, input_path, output_path = None):
    input_path = Normalize(input_path)

    with open(input_path, "rb") as f:
        plaintext = f.read()

    nonce = secrets.token_bytes(12)

    aesgcm = AESGCM(key)
    ciphertext_with_tag = aesgcm.encrypt(nonce, plaintext, associated_data=None)

    blob = nonce + ciphertext_with_tag

    if output_path is None:
        output_path = input_path
    with open(Normalize(output_path), "wb") as f:
        f.write(blob)

def rename(path, new_name):
    try:
        path = Normalize(path)
        new_path = path.resolve().parents[0] / new_name
        os.rename(path, new_path)
    except FileNotFoundError:
        print(f"Error: File '{path}' not found.")
    except FileExistsError:
        print(f"Error: File '{new_path}' already exists.")
    except PermissionError:
        print("Error: Insufficient permissions to rename the file.")
    except Exception as e:
        print(f"Unexpected error: {e}")

def copy(src, dst):
    src = Normalize(src)
    dst = Normalize(dst)
    
    if not src.exists():
        raise FileNotFoundError(f"Source does not exist: {src.resolve()}")

    if src.is_file():
        dst.parent.mkdir(parents=True, exist_ok=True)
        shutil.copy2(src, dst)
        return

    dst.mkdir(parents=True, exist_ok=True)

    for item in src.iterdir():       
        dest_item = dst / item.name
        if item.is_dir():
            shutil.copytree(item, dest_item, dirs_exist_ok=True)
        else:
            shutil.copy2(item, dest_item)

def remove(target):
    target = Normalize(target)

    if not target.exists():
        return

    if target.is_file() or target.is_symlink():
        target.unlink()
    elif target.is_dir():
        shutil.rmtree(target)
    else:
        raise RuntimeError(f"Unknown file: {target}")

def modify_json(path, fields_to_remove, save=True):
    path = Normalize(path)
    data = json.loads(path.read_text(encoding="utf-8"))

    for field in fields_to_remove:
        if field in data:
            del data[field]

    if save:
        path.write_text(json.dumps(data, indent=2, ensure_ascii=False), encoding="utf-8")

    return data

def mkdir(path):
    path = Normalize(path)

    if os.path.isdir(path):
        shutil.rmtree(path)

    os.makedirs(path, exist_ok=True)

def pakFolder(src, dst):
    src = Normalize(src)
    dst = Normalize(dst)

    if os.path.exists(dst): os.remove(dst)

    with zipfile.ZipFile(dst, 'w', zipfile.ZIP_DEFLATED) as zf:
        for root, dirs, files in os.walk(src):
            for fn in files:
                abs_fn = os.path.join(root, fn)
                rel = os.path.relpath(abs_fn, src)
                zf.write(abs_fn, rel)

def Normalize(path):
    path = Path(path)
    if not path.is_absolute():
        path = HERE / path
    return path.resolve()

def generate_key_pair(public_key_path, private_key_path):
    public_key_path = Normalize(public_key_path)
    private_key_path = Normalize(private_key_path)

    key = ECC.generate(curve='P-256')

    pub = key.public_key().export_key(format='PEM')
    with open(public_key_path,'wt') as f:
        f.write(pub)

    priv = private_key_path.write_text(key.export_key(format="PEM"), encoding="utf-8")

def sign(files, private_key_path, sign_path):
    private_key_path = Normalize(private_key_path)
    sign_path = Normalize(sign_path)

    key = ECC.import_key(private_key_path.read_text(encoding="utf-8"))
    signer = DSS.new(key, mode="fips-186-3")

    h_master  = SHA512.new()
    for f in files:
        with Path(f).open("rb") as stream:
            for chunk in iter(lambda: stream.read(65536), b""):
                h_master.update(chunk)

    signature = signer.sign(h_master)
    sign_path.write_bytes(signature)

def inject_global_variable(file_path, variable_name, new_value, encoding = "utf-8"):
    path = Normalize(file_path)
    text = path.read_text(encoding=encoding)

    pattern = re.compile(
        rf"^(\s*(?:private|internal|public)\b[\w\s\<\>\,\.\[\]]*\b{re.escape(variable_name)}\s*(?:\{{.*?\}}\s*)?=\s*)(.+?)(\s*;)",
        re.MULTILINE | re.DOTALL | re.VERBOSE
    )

    def _repl(match: re.Match) -> str:
        prefix = match.group(1)
        suffix = match.group(3)
        return f"{prefix}{new_value}{suffix}"

    new_text, count = pattern.subn(_repl, text)

    if count == 0:
        raise RuntimeError(f"`{variable_name}` not found in {file_path}")

    path.write_text(new_text, encoding=encoding)

    print(f"Inyected `{variable_name}` ({count} times) in {file_path}")

def find_first_csproj(start_path = "") -> Path:
    start_path = Normalize(start_path)

    for csproj in start_path.rglob("*.csproj"):
        return csproj.stem

    return None