# .NET Azure Function Sample

This tutorial will demonstrate how to use Azure Functions programming model to integrate with multiple Dapr components. Please first go through the [Dapr quickstarts](https://github.com/dapr/quickstarts) to get some contexts on various Dapr building blocks as well as go through Azure Functions [hello-world sample](https://docs.microsoft.com/en-us/azure/azure-functions/functions-create-first-function-vs-code?pivots=programming-language-csharp) to familiarize with function programming model.
We'll be running a Darp'd function app locally:
1) Invoked by [Dapr Service Invocation](https://docs.dapr.io/developing-applications/building-blocks/service-invocation/service-invocation-overview/) and persist/retrieve state using [Dapr State Management](https://github.com/dapr/components-contrib/tree/master/state)
2) Publish/consume message on a specific topic powered by [Dapr pub/sub](https://github.com/dapr/components-contrib/tree/master/pubsub) and `DaprPublish`/`DaprTopicTrigger`
3) Interact with [Dapr Bindings](https://github.com/dapr/components-contrib/tree/master/bindings) using `DaprBinding`

## Prerequisites
This sample requires you to have the following installed on your machine:
- [Setup Dapr](https://github.com/dapr/quickstarts/tree/master/hello-world) : Follow [instructions](https://docs.dapr.io/getting-started/install-dapr/) to download and install the Dapr CLI and initialize Dapr.
- [Install Azure Functions Core Tool](https://github.com/Azure/azure-functions-core-tools/blob/master/README.md#windows)
- [Run Kafka Docker Container Locally](https://github.com/dapr/quickstarts/tree/master/bindings). The required Kafka files is located in `sample\dapr-kafka` directory.

# Step 1 - Understand the Settings 

Now that we've locally set up Dapr, clone the repo, then navigate to the dotnet-isolated-azurefunction sample: 

```bash
git clone https://github.com/dapr/azure-functions-extension.git
cd samples/dotnet-isolated-azurefunction
```
In this folder, you will find `local.settings.json`, which lists a few app settings by the trigger/binding attributes.

```json
"StateStoreName": "statestore"
```

The `%` denotes an app setting value, for the following binding as an example:

`[DaprState("%StateStoreName%", Key = "product")]`

 In the runtime, the binding will check the `local.settings.json` file and resolve `%StateStoreName%` into `statestore`. The function will then make a call into the state store named as `statestore`.


Please make sure the value in `local.settings.json` matches with the name specified in the YAML files in Dapr `/component` folder:

```yaml
...

kind: Component
metadata:
  name: statestore
spec:

....
```

# Step 2 - Run Function App with Dapr


Run function host with Dapr: 

Windows
```
dapr run --app-id functionapp --app-port 3001 --dapr-http-port 3501 --components-path ..\components\ -- func host start
```

Linux/Mac OS
```
dapr run --app-id functionapp --app-port 3001 --dapr-http-port 3501 --components-path ../components/ -- func host start
```

The command should output the dapr logs that look like the following:

```
Starting Dapr with id functionapp. HTTP Port: 3501. gRPC Port: 55377
Updating metadata for app command: func host start
You're up and running! Both Dapr and your app logs will appear here.
...
```

> **Note**: there are three ports in this service. The `--app-port`(3001) is where our function host listens on for any Dapr trigger. The `--dapr-http-port`(3501) is where Dapr APIs runs on as well as the  grpc port. The function port (default 7071) is where function host listens on for any HTTP triggred function using `api/{functionName}` URl path. All of these ports are configurable.
> 


# Step 3 - Understand the Sample

## 1. Service Invocation and State Management: Create New Order and Retrieve Order

```csharp
[Function("DaprStateOutputBindingUserDefinedType")]
[DaprStateOutput("%StateStoreName%", Key = "product")]
public static Product Run(
    [DaprServiceInvocationTrigger] Product payload,
    FunctionContext functionContext)
{
    var log = functionContext.GetLogger("DaprStateOutputBindingUserDefinedType");
    log.LogInformation("C# function processed a DaprStateOutputBindingUserDefinedType request from the Dapr Runtime.");

    return payload;
}
```

Here the `DaprServiceInvocationTrigger` is used to receive and handle `DaprStateOutputBindingUserDefinedType` request and it first logs that this function is successfully triggered. Then it binds the content to the `product` object. The `DaprState` *output binding* will persist the order into the state store by serializing `product` object into a state arrary format and posting it to `http://localhost:${daprPort}/v1.0/state/${stateStoreName}`.

Now you can invoke this function by using the Dapr cli in a new command line terminal.  


Windows PowerShell
```powershell
dapr invoke --app-id functionapp --method DaprStateOutputBindingUserDefinedType --data '{\"Name\":\"Apple\",\"Description\":\"Fruit\",\"Quantity\":10}'
```


In your terminal window, you should see logs indicating that the message was received and state was updated:

```
== APP == [TIMESTAMP] Executing 'DaprStateOutputBindingUserDefinedType' (Reason='', Id=<ExecutionId>)
== APP == [TIMESTAMP] C# function processed a DaprStateOutputBindingUserDefinedType request from the Dapr Runtime.
== APP == [TIMESTAMP] Executed 'DaprStateOutputBindingUserDefinedType' (Succeeded, Id=<ExecutionId>)
```
----------------
In order to confirm the state is now persisted. You can now move to the next function:

```csharp
[Function("DaprStateInputBindingUserDefinedType")]
public static void Run(
    [DaprServiceInvocationTrigger] object args,
    [DaprStateInput("%StateStoreName%", Key = "product")] Product data, FunctionContext functionContext)
{
    var log = functionContext.GetLogger("DaprStateInputBindingUserDefinedType");
    log.LogInformation("C# function processed a DaprStateInputBindingUserDefinedType request from the Dapr Runtime.");

    //print the fetched state value
    log.LogInformation(JsonSerializer.Serialize(data));
}
```

Similarly, the function will be triggered by any `DaprStateInputBindingUserDefinedType` service invocation request. Here `DaprState` *input binding* is used to fetch the latest value of the key `product` and bind the value to string object `data`' before exectuing the function block.

Invoke input binding function with Dapr cli

```
dapr invoke --app-id functionapp --method DaprStateInputBindingUserDefinedType
```

In your terminal window, you should see logs to confirm the expected result:

```
== APP == [TIMESTAMP]  Executing 'DaprStateInputBindingUserDefinedType' (Reason='', Id=<ExecutionId>)
== APP == [TIMESTAMP]  C# function processed a DaprStateInputBindingUserDefinedType request from the Dapr Runtime.
== APP == [TIMESTAMP]  {"Name":"Apple","Description":"Fruit","Quantity":10}
== APP == [TIMESTAMP]  Executed 'DaprStateInputBindingUserDefinedType' (Succeeded, Id=<ExecutionId>)
```
