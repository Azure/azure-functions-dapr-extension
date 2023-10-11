# Troubleshooting Guide

## Introduction

Welcome to the troubleshooting guide for Azure Functions Dapr Extension. This guide is designed to help you address common issues and problems that may arise during the usage of Dapr Extension. Please follow the steps below to resolve any issues you encounter.

## Table of Contents

- [Issue 1: Unable to find package Microsoft.Azure.WebJobs.Extensions.Dapr with version (>= 99.99.99)](#unable-to-find-package-microsoftazurewebjobsextensionsdapr-with-version-999999)


## Unable to find package Microsoft.Azure.WebJobs.Extensions.Dapr with version 99.99.99

**Symptoms:** When building the Azure Function Dapr Extension [dotnet-isolated sample](https://github.com/Azure/azure-functions-dapr-extension/tree/master/samples/dotnet-isolated-azurefunction), you encounter an issue where it looks for the package `Microsoft.Azure.WebJobs.Extensions.Dapr` with version (>= 99.99.99), which is not available in the global NuGet package.

```
== APP == /WorkerExtensions.csproj : error NU1102: Unable to find package Microsoft.Azure.WebJobs.Extensions.Dapr with version (>= 99.99.99)
== APP == WorkerExtensions.csproj : error NU1102:   - Found 4 version(s) in nuget.org [ Nearest version: 0.17.0-preview01 ]
```

**Possible Causes:** The issue is caused by the dotnet language worker adding a reference to the NuGet package `Microsoft.Azure.WebJobs.Extensions.Dapr` with an a version (99.99.99) in `WorkerExtensions.csproj`. Once package reference is added to `WorkerExtensions.csproj`, it always looks for above nuget package even though you are not referencing it in you dotnet-isolate azure function. This is a [known issue in dotnet-language worker](https://github.com/Azure/azure-functions-dotnet-worker/issues/550) and should be fixed as part of [this PR](https://github.com/Azure/azure-functions-dotnet-worker/pull/1749).

**Resolution:**

1. **Generate the Package:**

   Build the Azure Function Dapr Extension repository from the root directory using the following command:

   ```bash
   dotnet build --configfile nuget.config
   ```
   This build step will create an `local-packages` folder in the root directory.
2. **Modify Global NuGet Configuration:**

    Next, you need to modify the global NuGet configuration file, which can be found at the following location based on your operating system:

    Windows: ```%appdata%\NuGet\NuGet.Config```

    macOS: Replace username with your username ```/Users/username/.nuget/NuGet.config```

    Edit the NuGet.config file and add `nuget.local` param, replace `<home_directory>` with your actual path:

    ```xml
    <?xml version="1.0" encoding="utf-8"?>
    <configuration>
        <packageSources>
            <add key="nuget.org" value="https://api.nuget.org/v3/index.json" protocolVersion="3" />
            <add key="nuget.local" value="<home_directory>/azure-functions-dapr-extension/local-packages" />
        </packageSources>
    </configuration>
    ```

**Preventive Measures:** If you don't need to debug the extension, you should always use [published nuget package](https://www.nuget.org/packages/Microsoft.Azure.Functions.Worker.Extensions.Dapr)

## Conclusion

This troubleshooting guide aims to help you resolve common issues and maintain a seamless experience while using Azure Functions Dapr Extension. If you encounter an issue that is not covered in this guide or need further assistance, please raise a GitHub issue.

We appreciate your use of Azure Functions Dapr Extension and value your feedback to help improve our service.
