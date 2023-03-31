# Azure Functions dapr extensions

![Build and Test](https://github.com/dapr/azure-functions-extension/workflows/Build%20and%20Test/badge.svg)
[![Gitter](https://badges.gitter.im/Dapr/community.svg)](https://gitter.im/Dapr/community?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

- [dapr.io](https://dapr.io)
- [@DaprDev](https://twitter.com/DaprDev)


⚠️ This extension is currently in preview and not recommended for production. ⚠️

The Azure Functions Dapr extension allows you to easily interact with the Dapr APIs from an Azure Function using triggers and bindings.  This extension is supported in any environment that supports running Dapr and Azure Functions - primarily self-hosted and Kubernetes modes.

If you are unfamiliar with Azure Functions, it's recommended to [try out an Azure Function's quickstart first](https://docs.microsoft.com/azure/azure-functions/) to understand the basics of the programming model.  

You can also jump to the [Dapr + Functions quickstart](./docs/quickstart.md) below.

This extension currently supports Azure Functions written in [C#](./samples/dotnet-azurefunction), [JavaScript / TypeScript](./samples/javascript-azurefunction), and [Python](./samples/python-azurefunction).

```javascript
module.exports = async function (context, req) {
    context.log('Function triggered.  Reading Dapr state...');

    context.log('Current state of this function: ' + context.bindings.daprState);

    context.log('Using Dapr service invocation to trigger Function B');

    context.bindings.daprInvoke = {
        appId: 'function-b',
        methodName: 'process',
        httpVerb: 'post',
        body: context.bindings.daprState
    }

    context.res = {
        status: 200
    };
};
```

## Function Triggers

Azure Function triggers start an execution.

| Trigger Type | Description | Samples |
| -- | -- | -- |
| [daprBindingTrigger][binding-trigger-docs] | Trigger on a Dapr input binding | [C#][csharp-binding-trigger], [JavaScript][javascript-binding-trigger], [Python][python-binding-trigger] |
| [daprServiceInvocationTrigger][service-invocation-trigger-docs] | Trigger on a Dapr service invocation | [C#][csharp-service-invocation-trigger], [JavaScript][javascript-service-invocation-trigger], [Python][python-service-invocation-trigger] |
| [daprTopicTrigger][topic-trigger-docs] | Trigger on a Dapr topic subscription | [C#][csharp-topic-trigger], [JavaScript][javascript-topic-trigger], [Python][python-topic-trigger] |

## Function Bindings

Azure Function bindings allow you to pull data in or push data out as during an execution.  **Input Bindings** pass in data at the beginning of an execution at the time of triggering.  **Output Bindings** push data out once an execution has completed.

| Binding Type | Direction | Description | Samples |
| -- | -- | -- | -- |
| [daprState][state-input-docs] | Input | Pull in Dapr state for an execution | [C#][csharp-state-input], [JavaScript][javascript-state-input], [Python][python-state-input] |
| [daprSecret][secret-input-docs] | Input | Pull in Dapr secrets for an execution | [C#][csharp-secret-input], [JavaScript][javascript-secret-input], [Python][python-secret-input] ||
| [daprState][state-output-docs] | Output | Save a value to Dapr state | [C#][csharp-state-output], [JavaScript][javascript-state-output], [Python][python-state-output] |
| [daprInvoke][invoke-output-docs] | Output | Invoke another Dapr app | [C#][csharp-invoke-output], [JavaScript][javascript-invoke-output], [Python][python-invoke-output] |
| [daprPublish][publish-output-docs] | Output | Publish a message to a Dapr topic | [C#][csharp-publish-output], [JavaScript][javascript-publish-output], [Python][python-publish-output] |
| [daprBinding][binding-output-docs] | Output | Send a value to a Dapr output binding | [C#][csharp-binding-output], [JavaScript][javascript-binding-output], [Python][python-binding-output] |

## Quickstart

You can run through a quickstart of developing some JavaScript Azure Functions that leverage Dapr with the [following tutorial](./docs/quickstart.md)

## Installing the extension

### .NET Functions

[Install the NuGet package](https://www.nuget.org/packages/Dapr.AzureFunctions.Extension) for this extension into your function app project.

### Non-.NET Functions

While this extension is in preview it is not included in the default extension bundle for functions.  You can still include it, but will need to manually install it into the project, and opt-out to using the default extensions.  

1. Open the `host.json` file from the root of the project and remove the `extensionBundle` property and values (if they exist).  Save the file.
1. Run `func extensions install -p Dapr.AzureFunctions.Extension -v 0.10.0-preview01`.  Be sure to use the latest version as [published on NuGet](https://www.nuget.org/packages/Dapr.AzureFunctions.Extension).  You must have the .NET Core SDK installed in order for this command to work.

This also means for other extensions your app may be leveraging (e.g. Azure Service Bus or Azure Storage) you will need to manually install them using the NuGet package for that extension.  For example, with Azure Storage the [documentation](https://docs.microsoft.com/azure/azure-functions/functions-bindings-storage-blob) links to a NuGet package for that extension where you could include in your app with this Dapr extension by running `func extensions install -p Microsoft.Azure.WebJobs.Extensions.Storage -v 4.0.2`.

### Developing the extension

The samples in this repo (other than the quickstart) are set up to run using a local build of the extension.

You can use a development build of the extension for any function by:

- Referencing the Dapr.AzureFunctions.Extension project in your .NET function
- Publishing the extension to the `bin/` directory of your non-.NET function

Example for non-.NET function:

```sh
dotnet publish /path/to/Dapr.AzureFunctions.Extension -o bin/
```

## Dapr ports and listeners
When you are triggering a function from Dapr, the extension will expose port 3001 automatically to listen to incoming requests from the Dapr sidecar.  

> IMPORTANT: Port 3001 will only be exposed and listened if a Dapr trigger is defined in the function app.  When using Dapr the sidecar will wait to receive a response from the defined port before completing instantiation.  This means it is important to NOT define the `dapr.io/port` annotation or `--app-port` unless you have a trigger.  Doing so may lock your application from the Dapr sidecar.  Port 3001 does not need to be exposed or defined if only using input and output bindings.

By default, when Azure Functions tries to communicate with Dapr it will call Dapr over the port resolved from the environment variable `DAPR_HTTP_PORT`.  If that is null, it will default to port `3500`.  

You can override the Dapr address used by input and output bindings by setting the `DaprAddress` property in the `function.json` for the binding (or the attribute).  By default it will use `http://localhost:{DAPR_HTTP_PORT}`.

The function app will still expose another port and endpoint for things like HTTP triggers (locally this defaults to 7071, in a container it defaults to 80).

## Running and debugging an app

Normally when debugging an Azure Function you use the `func` command line tool to start up the function app process and trigger your code.  When debugging or running an Azure Function that will leverage Dapr, you need to use `dapr` alongside `func` so both processes are running.

So when running a Dapr app locally using the default ports, you would leverage the `dapr` CLI to start the `func` CLI.

### If no Dapr triggers are in the app
`dapr run --app-id functionA --dapr-http-port 3501 -- func host start --no-build`

### If Dapr triggers are in the app
`dapr run --app-id functionA --app-port 3001 --dapr-http-port 3501 -- func host start --no-build`

## Deploying to Kubernetes

You can annotate your function Kubernetes deployments to include the Dapr sidecar.

> IMPORTANT: Port 3001 will only be exposed and listened if a Dapr trigger is defined in the function app.  When using Dapr, the sidecar will wait to receive a response from the defined port before completing instantiation.  This means it is important to NOT define the `dapr.io/port` annotation or `--app-port` unless you have a trigger.  Doing so may lock your application from the Dapr sidecar. Port 3001 does not need to be exposed or defined if only using input and output bindings.

To generate a Dockerfile for your app if you don't already have one, you can run the following command in your function project:
`func init --docker-only`.

The Azure Function core tools can automatically generate for you Kubernetes deployment files based on your local app.  It's worth noting these manifests expect [KEDA](https://keda.sh) will be present to manage scaling, so if not using KEDA you may need to remove the `ScaledObjects` generated, or craft your own deployment YAML file.  We do recommend including KEDA in any cluster that is running Azure Functions containers (with or without Dapr), but it is an optional component to assist with scaling.

An example of a function app deployment for Kubernetes can be [found below](#sample-kubernetes-deployment).

The following command will generate a `deploy.yaml` file for your project:
`func kubernetes deploy --name {container-name} --registry {docker-registry} --dry-run > deploy.yaml`

You can then edit the generated `deploy.yaml` to add the dapr annotations.  You can also craft a deployment manually.

### Azure Storage account requirements

While an Azure Storage account is required to run functions within Azure, it is NOT required for functions that run in Kubernetes or from a Docker container.  The exception to that is functions that leverage a Timer trigger or Event Hub trigger.  In those cases the storage account is used to coordinate leases for instances, so you will need to set an `AzureWebJobsStorage` connection string if using those triggers.  You can set the `AzureWebJobsStorage` value to `none` if not using any of the triggers that require it.

### Sample Kubernetes deployment

```yml
apiVersion: v1
kind: Service
metadata:
  name: my-function
  namespace: default
spec:
  selector:
    app: my-function
  ports:
  - protocol: TCP
    port: 80
    targetPort: 80
  type: LoadBalancer
---
apiVersion: apps/v1
kind: Deployment
metadata:
  name: my-function
  namespace: default
  labels:
    app: my-function
spec:
  replicas: 1
  selector:
    matchLabels:
      app: my-function
  template:
    metadata:
      labels:
        app: my-function
      annotations:
        dapr.io/enabled: "true"
        dapr.io/id: "functionapp"
        # Only define port of Dapr triggers are included
        dapr.io/port: "3001"
    spec:
      containers:
      - name: my-function
        image: myregistry/my-function
        ports:
        # Port for HTTP triggered functions
        - containerPort: 80
---
```

[binding-trigger-docs]: ./docs/triggers.md#input-binding-trigger
[service-invocation-trigger-docs]: ./docs/triggers.md#service-invocation-trigger
[topic-trigger-docs]: ./docs/triggers.md#topic-trigger
[state-input-docs]: ./docs/input-bindings.md#state-input-binding
[secret-input-docs]: ./docs/input-bindings.md#secret-input-binding
[state-output-docs]: ./docs/output-bindings.md#state-output-binding
[invoke-output-docs]: ./docs/output-bindings.md#service-invocation-output-binding
[publish-output-docs]: ./docs/output-bindings.md#topic-publish-output-binding
[binding-output-docs]: ./docs/output-bindings.md#dapr-binding-output-binding

[csharp-binding-trigger]: ./samples/dotnet-azurefunction/ConsumeMessageFromKafka.cs
[csharp-service-invocation-trigger]: ./samples/dotnet-azurefunction/RetrieveOrder.cs
[csharp-topic-trigger]: ./samples/dotnet-azurefunction/PrintTopicMessage.cs
[csharp-state-input]: ./samples/dotnet-azurefunction/StateInputBinding.cs
[csharp-secret-input]: ./samples/dotnet-azurefunction/RetrieveSecret.cs
[csharp-state-output]: ./samples/dotnet-azurefunction/StateOutputBinding.cs
[csharp-invoke-output]:  ./samples/dotnet-azurefunction/InvokeOutputBinding.cs
[csharp-publish-output]: ./samples/dotnet-azurefunction/PublishOutputBinding.cs
[csharp-binding-output]: ./samples/dotnet-azurefunction/SendMessageToKafka.cs

[javascript-binding-trigger]: ./samples/javascript-azurefunction/ConsumeMessageFromKafka/index.js
[javascript-service-invocation-trigger]: ./samples/javascript-azurefunction/RetrieveOrder/index.js
[javascript-topic-trigger]: ./samples/javascript-azurefunction/PrintTopicMessage/index.js
[javascript-state-input]: ./samples/javascript-azurefunction/StateInputBinding/index.js
[javascript-secret-input]:./samples/javascript-azurefunction/RetrieveSecret/index.js
[javascript-state-output]: ./samples/javascript-azurefunction/StateOutputBinding/index.js
[javascript-invoke-output]: ./samples/javascript-azurefunction/InvokeOutputBinding/index.js
[javascript-publish-output]: ./samples/javascript-azurefunction/PublishOutputBinding/index.js
[javascript-binding-output]: ./samples/javascript-azurefunction/SendMessageToKafka/index.js

[python-binding-trigger]: ./samples/python-azurefunction/ConsumeMessageFromKafka/__init__.py
[python-service-invocation-trigger]: ./samples/python-azurefunction/RetrieveOrder/__init__.py
[python-topic-trigger]: ./samples/python-azurefunction/PrintTopicMessage/__init__.py
[python-state-input]: ./samples/python-azurefunction/StateInputBinding/__init__.py
[python-secret-input]: /samples/python-azurefunction/RetrieveSecret/__init__.py
[python-state-output]: ./samples/python-azurefunction/StateOutputBinding/__init__.py
[python-invoke-output]: ./samples/python-azurefunction/InvokeOutputBinding/__init__.py
[python-publish-output]: ./samples/python-azurefunction/PublishOutputBinding/__init__.py
[python-binding-output]: ./samples/python-azurefunction/SendMessageToKafka/__init__.py
