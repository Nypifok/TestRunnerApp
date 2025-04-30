# TestRunnerApp
Test task for JetBrains team
![image](https://github.com/user-attachments/assets/dded08cb-de9a-41dd-b18f-5001b09e727b)


##How to install:
Launch ./TestRunnerInstaller.exe

After installation completed, just open TestRunner.exe from WindowsStartMenu or directly from "C:\Program Files (x86)\TestRunner\TestRunner.exe"

##How to build:

###Frontend:
Build:
PS: yourPath\TestRunnerApp\TestRunnerUI > ./gradlew build --refresh-dependencies 
Run: 
Before running don't forget to publish Backend to default folder, or choose any folder and change .\TestRunnerApp\TestRunnerUI\src\main\resources\config.dev.json

PS: $env:APP_ENV="dev"; ./gradlew run --stacktrace

###Backend

Build:
./TestRunnerApp/TestRunnerBackend/src > dotnet build TestRunnerBackend.sln

Publish:
./TestRunnerApp/TestRunnerBackend/src >

dotnet publish TestRunner.UIHost.csproj `
  -c Release `
  -r win-x64 `
  --self-contained true `
  /p:PublishSingleFile=true `
  /p:PublishReadyToRun=true `
  /p:IncludeNativeLibrariesForSelfExtract=true `
  /p:DeleteExistingFiles=true `
  -o ./publish

