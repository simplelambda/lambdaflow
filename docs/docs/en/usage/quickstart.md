# 🚀 Quickstart

Get a working Lambdaflow app running in just a few commands.

---

## 1. Prerequisites

- **Python 3.x** (for the `build.py` script)  
- **.NET SDK 7.0+** on your PATH  

---

## 2. Clone the repo

```bash
git clone https://github.com/simplelambda/lambdaflow.git
cd lambdaflow
```
Your folder structure:

```bash
lambdaflow/
├── development/          # Framework source (read/modify core). ONLY FOR THE FRAMEWORK! NOT FOR CREATING APPS WITH LAMBDASFLOW.
│   ├── src/              # Core framework code. See tech docs for details.
│   ├── app.ico           # Application icon (used in examples)
│   ├── build.py          # Script to compile and package Lambdaflow apps, used in examples.
│   └── lambdaflow.csproj # Framework C# project file (used in examples)
│
├── examples/
│    └── CSharp/               # Ready-to-run example app with C# backend
│        ├── backend/          # C# backend project
│        ├── frontend/         # HTML/CSS/JS UI
│        ├── lambdaflow/       # Copy of the src/ folder in development/
│        ├── config.json       # App settings & build targets
│        ├── app.ico           # Application icon
│        ├── build.py	       # Build script
│        ├── license.txt       # License example for your app (optional)           
│        └── lambdaflow.csproj # framework C# project file
│
├── docs/                 # MkDocs sources (this documentation). Not relevant for building apps.
├── buildExamples.py      # Script that allows to modify all examples at once if source code is changed
└── …
```
>Tip: Don't build from development/src. Use ready examples like `examples/CSharp/` instead.

---

## 3. Configure the example

Open one of the `examples` of your choice and modify config.json. You should modify:

- `"appName"`, `"appVersion"`, `"organizationName"`

- `"appIcon"` (path to app.ico)

- `Window size`, `frontendInitialHTML`, `platforms` & `archs` to compile
 
- IMPORTANT: Set `"compileCommand"` to the correct command for the platform and arch you are building for.

> Go to the [Configuration Reference](usage/configuration.md) for details on each setting and `all available options`.

---

## 4. Build everything

From the example directory, run the build script:

```bash
python build.py
```

This will:

1. Check prerequisites and dependencies.

2. Compile your backend with the specified `compileCommand` in `config.json`.

3. Compile the framework C# project (`lambdaflow.csproj`) using the .NET SDK.

4. Package output files into the `'Results'` (by default) folder inside the example directory.

The result could look like this:

```
Project/
├── Results/
│	├── windows/
│	│   ├── x64/...
│	│   └── x86/...
│	├── linux/
│   │   ├── x64/...
│   │   └── arm64/...
│	└── mac/
│       └── x64/...
├── ...
├── ...
└── ...
```

---

## 5. Run your app

### On Windows

```powershell
cd Project\Results\windows\x64
.\Program.exe
```

### On Linux/macOS

```bash
cd Project/Results/linux/x64
chmod +x Program
./Program
```

You should see the WebView window launch your HTML/JS UI and communicate via IPC with the backend.

## 6. What’s next?

- [Configuration Reference](usage/configuration.md)

- [Security Guide](usage/security.md)

- [Architecture & API](tech/architecture.md)

- [Packaging & Installers](usage/packaging.md)