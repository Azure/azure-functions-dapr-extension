# Setup Local Development

This document is a TODO.

## Prerequisites

## Clone the repository

## Build the project

## Run tests

## Debugging
### Debugging InProc

1. Create/reuse existing samples to debug the extension.
2. Run the function app with below command.
    ```
    dapr run --app-id functionapp --app-port 3001 --dapr-http-port 3501 --components-path ..\components\ -- func host start
    ```
3. Attach debugger in Visual Studio by pressing Ctrl+Alt+P key, and search `func` in the windows, select the process and click on attach.
4. Add break points and enjoy debugging.

### Debugging Isolated (Out of proc)

1. Create/reuse existing samples to debug the extension.
2. Run the function app with below command.
    ```
    dapr run --app-id functionapp --app-port 3001 --dapr-http-port 3501 --components-path ..\components\ -- func host start
    ```
3. In Visual studio, you can add nuget package reference to `local-packages` folder where it will have local nuget package when you build the extension solution.
    1. To add local nuget package, in Visual Studio go to, `Tool->Nuget Package Manager->Package Manager Settings->Nuget Package Manager->Package Sources`
    2. Click on `plus` sign, In name section provide a friendly name e.g `nuget.local` and in the source, provide the path the `local-packages` folder
4. Attach debugger in Visual Studio by pressing Ctrl+Alt+P key, and search `func` in the windows, select the process and click on attach.
4. Add break points and enjoy debugging.


### Known issues with isolated worker debugging.
1. You may see error `Microsoft.Git.Build.Task.dll` is not accessible, when you clean the nuget package, its better if you keep the backup before cleaning the nuget and paste it on the same path nuget folder after cleaning up.
2. Some times, you may have to go and update `%appdata%\NuGet\NuGet.Config` and provide the path to your local-packages
    ```xml
    <?xml version="1.0" encoding="utf-8"?>
    <configuration>
    <packageSources>
        <add key="nuget.org" value="https://api.nuget.org/v3/index.json" protocolVersion="3" />
        <add key="Microsoft Visual Studio Offline Packages" value="C:\Program Files (x86)\Microsoft SDKs\NuGetPackages\" />
        <add key="local" value="C:\local-packages" />
    </packageSources>
    </configuration>
    ```
    Its better if you can create a junction to `C:\local-packages` and `.\azure-functions-dapr-extension\local-packages`, so that anytime you build the extension, you always have the latest nuget package picked.

    ```
     mklink /J C:\local-packages path\to\azure-functions-dapr-extension\local-packages
    ```