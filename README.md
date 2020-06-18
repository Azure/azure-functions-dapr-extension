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
| daprBindingTrigger | Trigger on a Dapr input binding | [C#](), [JavaScript](), [Python]() |
| daprServiceInvocationTrigger | Trigger on a Dapr service invocation | [C#](), [JavaScript](), [Python]() |
| daprTopicTrigger | Trigger on a Dapr topic subscription | [C#](), [JavaScript](), [Python]() |

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
