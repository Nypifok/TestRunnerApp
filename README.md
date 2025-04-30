# 🧪 TestRunnerApp

> ✅ **Test task for JetBrains team**

![screenshot](https://github.com/user-attachments/assets/dded08cb-de9a-41dd-b18f-5001b09e727b)

---

## 🚀 Installation

### 🔧 Via Installer:

1. Run `./TestRunnerInstaller.exe`
2. After installation:
   - Launch the app from **Start Menu** → `TestRunner`
   - Or directly from:  
     `C:\Program Files (x86)\TestRunner\TestRunner.exe`

---

## 🛠️ Build Instructions

### 📦 Frontend (Kotlin + Swing)

#### 📌 Build:
```powershell
cd yourPath\TestRunnerApp\TestRunnerUI
./gradlew build --refresh-dependencies
```

#### ▶️ Run:
Before running, make sure the **backend is published** to the default folder.  
If using a custom location, update the path in `TestRunnerUI/src/main/resources/config.dev.json`.

```powershell
$env:APP_ENV="dev"
./gradlew run --stacktrace
```

---

### ⚙️ Backend (.NET 8, gRPC)

#### 📌 Build:
```powershell
cd TestRunnerApp/TestRunnerBackend/src
dotnet build TestRunnerBackend.sln
```

#### 🚀 Publish (Self-Contained Executable):
```powershell
cd TestRunnerApp/TestRunnerBackend/src

dotnet publish TestRunner.UIHost.csproj `
  -c Release `
  -r win-x64 `
  --self-contained true `
  /p:PublishSingleFile=true `
  /p:PublishReadyToRun=true `
  /p:IncludeNativeLibrariesForSelfExtract=true `
  /p:DeleteExistingFiles=true `
  -o ./publish
```

---
