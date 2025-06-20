🦙 Lambdaflow

[![Build Status](https://img.shields.io/github/actions/workflow/status/your-org/lambdaflow/ci.yml?branch=main)](https://github.com/your-org/lambdaflow/actions) [![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE) [![Docs](https://img.shields.io/badge/Docs-Online-blue.svg)](https://simplelambda.github.io/lambdaflow/)

---

## What is Lambdaflow?

**Lambdaflow** is a **lightweight C# framework** for building truly **polyglot**, **secure**, **cross-platform** desktop apps:

- **Any backend**: .NET, Go, Rust, Python, Node.js… even native C/C++.  
- **Modern Web UIs**: HTML + CSS + JS via a native WebView, **no HTTP server** required.  
- **High-performance IPC**: binary messages between your code and UI—minimal overhead, maximum speed.  

---

# [📖 Lambdaflow Documentation »](https://simplelambda.github.io/lambdaflow/)

For full detailed guides, installation instructions, API reference and examples, click the link above.

---

## 🚀 Key Features

| Area               | Highlights                                          |
|--------------------|-----------------------------------------------------|
| **Polyglot**       | Any language or runtime—your choice of backend.     |
| **Security**       | Signed integrity manifest, tamper detection, ACL-style locking, CSP. |
| **Frontend**       | In-memory `app://` loader, zero-write extraction, CSP-enforced. |
| **Backend**        | Two-phase temp-dir hardening, single-file optional, file-handle locks. |
| **Build & Ship**   | PublishSingleFile, self-contained, x86/x64/ARM64.   |
| **Installers**     | Windows (.exe/.msi) with shortcuts & clean uninstall, Linux/macOS `.desktop`. |

---

## 🛡️ Security

1. **Integrity at launch**  
   - Verifies an **Ed25519-signed SHA-512 manifest** of all PAKs.  
   - Rejects any modification before UI or backend runs.

2. **Immutable frontend**  
   - `frontend.pak` locked on startup.  
   - No disk-extracted HTML/CSS/JS—served via `app://` + CSP.

3. **Immutable backend**  
   - Two-phase lock: **deny delete** → extract → **deny write** on temp folder.  
   - `backend.exe` held open to prevent replacement, even on Unix `unlink`.

4. **CSP & sandboxing**  
   - Injected `Content-Security-Policy` in your HTML to block remote scripts, external resources, inline XSS.  
   - Secure default `fetch` shim for your WebView.

---

## ⚙️ Interoperability

- **Any OS**: Windows, Linux, macOS (x64 & ARM64).  
- **Any backend**: library, executable, script—just speak the binary-IPC protocol.  
- **Any frontend**: use your favorite Web UI framework (React, Vue, Svelte, plain JS).  
- **Plugin friendly**: load additional modules or inject custom handlers via `readResource` or your own IPC channels.

---

## ⚠️ Note: Although the security model is becoming increasingly robust, backend testing on Linux/MacOS and non-C# platforms is still ongoing. Lambdaflow is not yet recommended for critical production applications.
