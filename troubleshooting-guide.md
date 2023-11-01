# Troubleshooting Guide

## Introduction

Welcome to the troubleshooting guide for Azure Functions Dapr Extension. This guide is designed to help you address common issues and problems that may arise during the usage of Dapr Extension. Please follow the steps below to resolve any issues you encounter.

## Table of Contents

- [Issue 1: Unable to find package Microsoft.Azure.WebJobs.Extensions.Dapr with version (>= 99.99.99)](#unable-to-find-package-microsoftazurewebjobsextensionsdapr-with-version-999999)
- [Issue 2: Dapr sidecar is not present](#dapr-sidecar-is-not-present)
- [Issue 3: Dapr app config error](#dapr-app-config-error)


## Unable to find package Microsoft.Azure.WebJobs.Extensions.Dapr with version 99.99.99

**Symptoms:** When building the Azure Function Dapr Extension [dotnet-isolated sample](https://github.com/Azure/azure-functions-dapr-extension/tree/master/samples/dotnet-isolated-azurefunction), you encounter an issue where it looks for the package `Microsoft.Azure.WebJobs.Extensions.Dapr` with version (>= 99.99.99), which is not available in the global NuGet package.

```
== APP == /WorkerExtensions.csproj : error NU1102: Unable to find package Microsoft.Azure.WebJobs.Extensions.Dapr with version (>= 99.99.99)
== APP == WorkerExtensions.csproj : error NU1102:   - Found 4 version(s) in nuget.org [ Nearest version: 0.17.0-preview01 ]
```

**Possible Causes:** The issue is caused by the dotnet language worker adding a reference to the NuGet package `Microsoft.Azure.WebJobs.Extensions.Dapr` with an a version (99.99.99) in `WorkerExtensions.csproj`. Once package reference is added to `WorkerExtensions.csproj`, it always looks for above nuget package even though you are not referencing it in you dotnet-isolate azure function. This is a [known issue in dotnet-language worker](https://github.com/Azure/azure-functions-dotnet-worker/issues/550) and should be fixed as part of [this PR](https://github.com/Azure/azure-functions-dotnet-worker/pull/1749).

**Resolution:**

1. ***Generate the Package:***

   Build the Azure Function Dapr Extension repository from the root directory using the following command:

   ```bash
   dotnet build --configfile nuget.config
   ```
   This build step will create an `local-packages` folder in the root directory.
2. ***Modify Global NuGet Configuration:***

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

## Dapr sidecar is not present

**Symptoms:** If you are utilizing Dapr bindings and triggers in Azure Functions, you might encounter an error message like the following:

```plaintext
Dapr sidecar is not present. Please see (https://aka.ms/azure-functions-dapr-sidecar-missing) for more information.
```

**Possible Causes:** This error typically occurs when Dapr is not properly enabled in your environment.

**Resolution:**

***Enable Dapr in your application:***

- If your Azure Function is deployed in Azure Container Apps (ACA), refer to the documentation [here](https://learn.microsoft.com/en-us/azure/azure-functions/functions-bindings-dapr?tabs=in-process%2Cpreview-bundle-v4x%2Cbicep1&pivots=programming-language-python#dapr-enablement) for instructions on enabling Dapr.
- If your Azure Function is deployed in Kubernetes, ensure that your [deployment](https://github.com/ASHIQUEMD/azure-functions-dapr-extension/blob/master/deploy/kubernetes/kubernetes-deployment.md#sample-kubernetes-deployment) has the following annotations in its YAML configuration:
    ```YAML
    annotations:
        dapr.io/enabled: "true"
        dapr.io/app-id: "functionapp"
        # Only define port of Dapr triggers are included
        dapr.io/app-port: "3001"
    ```
- If you are running your Azure Function locally, make sure you are [running the function app with Dapr](https://github.com/ASHIQUEMD/azure-functions-dapr-extension/tree/master/samples/python-v2-azurefunction#step-2---run-function-app-with-dapr). Execute the following command:
    ```bash
    dapr run --app-id functionapp --app-port 3001  --components-path ..\components\ -- func host start 
    ```

**Preventive Measures:** To prevent this error, always ensure that Dapr is properly set up and enabled in your environment before using Dapr bindings and triggers with Azure Functions.

## Dapr app config error

**Symptoms:** If you encounter the following error message:
```plaintext
The Dapr sidecar is configured to listen on port {portInt}, but the app server is running on port {appPort}. This may cause unexpected behavior. For more information, visit [this link](https://aka.ms/azfunc-dapr-app-config-error).
```

**Possible Causes:** This error occurs when using Dapr Triggers and not configuring the app port in Dapr settings for the ContainerApps properly. 

**Note:** The Azure Function Dapr Extension starts an HTTP server on port 3001 when there is Dapr trigger in your Azure Function, you can configure this port using the DAPR_APP_PORT environment variable. If you provide an incorrect app_port value when running the Function app, it can lead to this problem.

**Resolution:**

***Configure the app-port correctly:*** When you are triggering a function from Dapr, the extension will expose port 3001 automatically to listen to incoming requests from the Dapr sidecar. This port is configurable, you can provide any other available port in your app settings for DAPR_APP_PORT env variable instead of 3001.

Ensure that you provide the correct DAPR_APP_PORT value to Dapr in the Dapr configuration.

- In Azure Container Apps (ACA), specify it in Bicep as shown below:

```
DaprConfig: {
...
appPort: 3001
...
}
```

- In a Kubernetes environment, set the dapr.io/app-port annotation:


```
annotations:
    ...
    dapr.io/app-port: "3001"
    ...
```

- For local development, make sure to provide --app-port when running the Function app with Dapr:

```
dapr run --app-id functionapp --app-port 3001 --components-path ..\components\ -- func host start 
```

**Preventive Measures:** To prevent this error, always configure the app-port parameter correctly in your Dapr setup.

## Conclusion

This troubleshooting guide aims to help you resolve common issues and maintain a seamless experience while using Azure Functions Dapr Extension. If you encounter an issue that is not covered in this guide or need further assistance, please raise a GitHub issue.

We appreciate your use of Azure Functions Dapr Extension and value your feedback to help improve our service.
