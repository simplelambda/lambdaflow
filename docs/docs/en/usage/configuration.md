# 🔧 Configuration

Lambdaflow is driven by a single JSON file (`config.json`) that lives next to your project under `ProjectFolder/config.json`. This file controls **both** build-time and run-time behavior:

- **Build-time**: which RIDs to publish, output folders, dev-only paths...  
- **Run-time**: frontend folder, initial HTML, window size, security settings...

Below is a full **schema** and explanation of each field.

---

## Example `config.json`

```json
{

  // ─── GENERAL FIELDS ────────────────────────────────────────────────────────

  "appName": "App",
  "appVersion": "1.0.0",
  "organizationName": "SimpleLambda",
  "appIcon": "app.ico",

  // ─── BUILD-TIME FIELDS ─────────────────────────────────────────────────────

  "platforms": {
    "windows": {
      "archs": 
        "x64": {
          "compileCommand": "dotnet publish -c Release -r win-x64",
          "compileDirectory": "bin/Release/net8.0/win-x64/publish"
        },
        "x86": {
          "compileCommand": "dotnet publish -c Release -r win-x86",
          "compileDirectory": "bin/Release/net8.0/win-x86/publish"
        }
      }
    },

    "linux": {
      "archs": {
        "x64": {
          "compileCommand": "dotnet publish -c Release -r linux-x64 -o bin",
          "compileDirectory": "bin/Release/net8.0/linux-x64",
        }
      }
    }
  },

  // ─── RUN-TIME FIELDS ──────────────────────────────────────────────────────

  "frontendInitialHTML": "index.html",
  "window": {
    "title": "My Lambdaflow App",
    "width": 1200,
    "height": 800
  }
}
```

---

## ▶️ General Fields

In this section you can find all fields that are used to configure the general properties of your application.

### appName
- Type: `string`
- Default: `"LambdaFlowApp"`
- Purpose: The name of your application. This is used in the window title, executable name, installer...

### appVersion
- Type: `string`
- Default: `"1.0.0"`
- Purpose: The version of your application. This is used in the installer, OS registries and other places where version information is displayed.

### organizationName
- Type: `string`
- Default: `"SimpleLambda"`
- Purpose: The name of your organization or company. This is used in the installer, OS registries and other places where organization information is displayed.

### appIcon
- Type: `string`
- Default: `"app.ico"`
- Purpose: The path to the application icon file. This icon will be used in the window title, executable, installer...

## ▶️ Build-time Fields

In this section you can find all fields that you can use to configure the build process of your application. These fields are used to specify how your application should be compiled and packaged for different platforms and architectures.

### developmentBackendFolder
- Type: `string`
- Default: `"backend"`
- Purpose: The folder where your backend source code is located. This is used to compile the backend code before packaging.
- Example: If you change the backend folder to `"my-backend"`, the build script will look for the backend code in `my-backend/`, not in backend/.

### developmentFrontendFolder
- Type: `string`
- Default: `"frontend"`
- Purpose: The folder where your frontend source code is located. This is used to copy the frontend files to the output folder.
- Example: If you change the frontend folder to `"my-frontend"`, the build script will look for the frontend files in `my-frontend/`, not in frontend/.

### addLicenseToInstaller
- Type: `boolean`
- Default: `false`
- Purpose: Whether to add a license file to the installer. If set to `true`, the build script will look for the `licenseFile` property and include the license in the installer.

### licenseFile
- Type: `string`
- Default: `"license.txt"`
- Purpose: The path to the license file that will be included in the installer if `addLicenseToInstaller` is set to `true`. This file should contain the license text for your application.

### selfContainedFramework
- Type: `boolean`
- Default: `true`
- Purpose: Whether to build a self-contained application. If set to `true`, the build script will include the .NET runtime in the output folder, allowing your application to run on machines without .NET installed.
- Note: Obviously, this will increase the size of your application, if you want to reduce the size, set this to `false` and make sure the target machine has .NET installed.

### resultFolder
- Type: `string`
- Default: `"Results"`
- Purpose: The folder where the build script will output the compiled application. This folder will contain the final executable, frontend files, and any other necessary files for your application to run.

### platforms
- Type: `object`
- Purpose: This object contains the configuration for each platform you want to build your application for. Each platform can have multiple architectures, and each architecture can have its own compile command and output directory.
- Children fields:
  - `windows`: Configuration for Windows platforms.
  - `linux`: Configuration for Linux platforms.
  - `mac`: Configuration for macOS platforms.
  - 
- Note: This field is `REQUIRED`. You must specify at least one platform and architecture to build your application.
- Example:
```json
"platforms": {
    "windows": {
      ...
    },
    "linux": {
      ...
    },
    "mac": {
      ...
    }
  }
```

### windows, linux, mac
- Type: `object`
- Parent: `platforms`
- Purpose: Configuration for Windows, Linux or macOS platforms.
- Children fields:
  - `archs`

### archs
- Type: `object`
- Parent: `windows`, `linux`, `mac`
- Purpose: This object contains the configuration for each architecture you want to build your application for on the specified platform.
- Children fields:
  - `x64`
  - `x86`
  - `arm64`
- Note: You can add more architectures as needed, but you must specify at least one architecture for each platform.
- Example:
```json
"windows":{
   "archs": {
      "x64": {
         ...
      },
      "x86": {
         ...
      }
   }
}

```

### x64, x86, arm64...
- Type: `object`
- Parent: `archs`
- Purpose: Configuration for the specified architecture on the platform.
- Children fields:
  - `compileCommand`
  - `compileDirectory`
  - `ignore`

### compileCommand
- Type: `string`
- Parent: `x64`, `x86`, `arm64`
- Purpose: The command that will be executed to compile your application backend for the specified architecture and platform.
- Example: `"dotnet publish -c Release -r win-x64"` for C# backend.

### compileDirectory
- Type: `string`
- Parent: `x64`, `x86`, `arm64`
- Purpose: The directory where the compiled application will be outputted. This should match the output directory of your backend build command. The specified path will be compressed and included in the final package.
- Example: `"bin/Release/net8.0/win-x64/publish"` for C# backend.

### ignore
- Type: `boolean`
- Parent: `x64`, `x86`, `arm64`
- Purpose: Whether to ignore this architecture when building the application. If set to `true`, this architecture will not be compiled or included in the final package.

## ▶️ Run-time Fields

### frontendInitialHTML
- Type: `string`
- Default: `"index.html"`
- Purpose: The path to the initial HTML file that will be loaded when your application starts. The path is relative to the frontend folder specified in the `developmentFrontendFolder` field.
- Example: If you set this to `"index.html"`, the application will look for `frontend/index.html` when it starts.

### window
- Type: `object`
- Purpose: Configuration for the main application window that will be displayed when your application starts.
- Children fields:
  - `title`
  - `width`
  - `height`
  - `minWidth`
  - `minHeight`
  - `maxWidth`
  - `maxHeight`

### title
- Type: `string`
- Parent: `window`
- Default: `"lambdaFlowApp"`
- Purpose: The title of the main application window. This will be displayed in the window title bar.

### width
- Type: `number`
- Parent: `window`
- Default: `800`
- Purpose: The initial width of the main application window in pixels.

### height
- Type: `number`
- Parent: `window`
- Default: `600`
- Purpose: The initial height of the main application window in pixels.

### minWidth
- Type: `number`
- Parent: `window`
- Default: `0` (not limited)
- Purpose: The minimum width of the main application window in pixels. The user will not be able to resize the window smaller than this width.

### minHeight
- Type: `number`
- Parent: `window`
- Default: `0` (not limited)
- Default: `300`
- Purpose: The minimum height of the main application window in pixels. The user will not be able to resize the window smaller than this height.

### maxWidth
- Type: `number`
- Parent: `window`
- Default: `0` (not limited)
- Purpose: The maximum width of the main application window in pixels. The user will not be able to resize the window larger than this width.

### maxHeight
- Type: `number`
- Parent: `window`
- Default: `0` (not limited)
- Purpose: The maximum height of the main application window in pixels. The user will not be able to resize the window larger than this height.