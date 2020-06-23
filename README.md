# Azure Functions dapr extensions

![Build and Test](https://github.com/dapr/azure-functions-extension/workflows/Build%20and%20Test/badge.svg)

⚠️ This extension is currently in preview and not recommended for production. ⚠️

The Azure Functions Dapr extension allows you to easily interact with the Dapr APIs from an Azure Function using triggers and bindings.  This extension is supported in any environment that supports running Dapr and Azure Functions - primarily self-hosted and Kubernetes modes.

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
| daprState | Input | Pull in Dapr state for an execution | [C#](), [JavaScript](), [Python]() |
| daprSecret | Input | Pull in Dapr secrets for an execution | [C#](), [JavaScript](), [Python]() |
| daprState | Output | Save a value to Dapr state | [C#](), [JavaScript](), [Python]() |
| daprInvoke | Output | Invoke another Dapr app | [C#](), [JavaScript](), [Python]() |
| daprPublish | Output | Publish a message to a Dapr topic | [C#](), [JavaScript](), [Python]() |
| daprBinding | Output | Send a value to a Dapr output binding | [C#](), [JavaScript](), [Python]() |

## Getting Started

### Prerequisites

### Creating the function app

### Installing the Dapr extension

### Using Dapr triggers and bindings

### Debugging locally

### Creating an Azure Function Docker image

### Deploying to Kubernetes

[binding-trigger-docs]: ./docs/triggers.md#input-binding-trigger
[service-invocation-trigger-docs]: ./docs/triggers.md#service-invocation-trigger
[topic-trigger-docs]: ./docs/triggers.md#topic-trigger

[csharp-binding-trigger]: ./samples/dotnet-azurefunction/ConsumeMessageFromKafka.cs
[csharp-service-invocation-trigger]: ./samples/dotnet-azurefunction/RetrieveOrder.cs
[csharp-topic-trigger]: ./samples/dotnet-azurefunction/PrintTopicMessage.cs

[javascript-binding-trigger]: ./samples/javascript-azurefunction/ConsumeMessageFromKafka/index.js
[javascript-service-invocation-trigger]: ./samples/javascript-azurefunction/RetrieveOrder/index.js
[javascript-topic-trigger]: ./samples/javascript-azurefunction/PrintTopicMessage/index.js

[python-binding-trigger]: ./samples/python-azurefunction/ConsumeMessageFromKafka/__init__.py
[python-service-invocation-trigger]: ./samples/python-azurefunction/RetrieveOrder/__init__.py
[python-topic-trigger]: ./samples/python-azurefunction/PrintTopicMessage/__init__.py
