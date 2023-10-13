# .NET Azure Function quicksart in Dotnet isolated mode

This quickstart will demonstrate how to use Azure Functions programming model to integrate with multiple Dapr components in dotnet isolated mode. Please first go through the [Dapr quickstarts](https://github.com/dapr/quickstarts) to get some contexts on various Dapr building blocks as well as go through Azure Functions [hello-world sample](https://docs.microsoft.com/en-us/azure/azure-functions/functions-create-first-function-vs-code?pivots=programming-language-csharp) to familiarize with function programming model.
We'll be running a Darp'd function app locally:

This quickstart contains OrderService function app which below three azure functions
- **OrderSerivce** - This is Http Trigger function which internally does [Dapr Service Invocation](https://docs.dapr.io/developing-applications/building-blocks/service-invocation/service-invocation-overview/) using Dapr invoke output binding.
- **CreateNewOrder** - This is Dapr Service Invocation enabled azure function which will be invoked by OrderService, and save the state to state store using [Dapr State Output Binding](https://docs.dapr.io/reference/api/state_api/#save-state).
- **RetrieveOrder** - This is Http Trigger function which uses [Dapr State Input Binding](https://docs.dapr.io/reference/api/state_api/#get-state) to read from state store.


## Prerequisites
This sample requires you to have the following installed on your machine:
- Setup Dapr: Follow instructions to [download and install the Dapr CLI](https://docs.dapr.io/getting-started/install-dapr-cli/) and [initialize Dapr](https://docs.dapr.io/getting-started/install-dapr-selfhost/).
- [Install Azure Functions Core Tool](https://github.com/Azure/azure-functions-core-tools/blob/master/README.md#windows)

# Step 1 - Understand the Settings 

Now that we've locally set up Dapr, clone the repo, then navigate to the dotnet-isolated-azurefunction sample: 

```bash
git clone https://github.com/Azure/azure-functions-dapr-extension.git
cd azure-functions-dapr-extension
dotnet build --configfile nuget.config
cd quickstarts/dotnet-isolated
```

In this folder, you will find `local.settings.json`, which lists a few app settings by the trigger/binding attributes.

```json
"StateStoreName": "statestore"
```

Dapr components: This quickstart uses default dapr components `(redis state store)` which gets installed in local when you perform `dapr init`.

You can find default dapr componentst at below location

**Windows:** 
```
C:\Users\<username>\.dapr
```
**Mac:** 
```
/Users/<username>/.dapr
```

# Step 2 - Run Function App with Dapr

Run function host with Dapr: 

Windows (requires Dapr 1.12+)
```
dapr run -f .
```

Linux/Mac OS (requires Dapr 1.11+)
```
dapr run -f .
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

Below is the Http Trigger function which internally does [Dapr Service Invocation](https://docs.dapr.io/developing-applications/building-blocks/service-invocation/service-invocation-overview/) using Dapr invoke output binding.

```csharp
[Function("OrderService")]
[DaprInvokeOutput(AppId = "{appId}", MethodName = "{methodName}", HttpVerb = "post")]
public static async Task<InvokeMethodParameters> Run(
    [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "invoke/{appId}/{methodName}")] HttpRequestData req, 
    FunctionContext functionContext)
{
    var log = functionContext.GetLogger("OrderService");
    log.LogInformation("C# HTTP trigger function processed a request.");

    string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

    //print the received payload
    log.LogInformation($"Received Payload OrderService: {requestBody}");

    var outputContent = new InvokeMethodParameters
    {
        Body = requestBody
    };

    return outputContent;
}
```

Below `DaprServiceInvocationTrigger` is used to receive and handle `CreateNewOrder` request and it first logs that this function is successfully triggered. Then it binds the content to the `JsonElement` object. The `DaprState` *output binding* will persist the order into the state store by serializing `JsonElement` object into a state arrary format and posting it to `http://localhost:${daprPort}/v1.0/state/${stateStoreName}`.

```csharp
[Function("CreateNewOrder")]
[DaprStateOutput("%StateStoreName%", Key = "order")]
public static JsonElement Run(
    [DaprServiceInvocationTrigger] JsonElement payload,
    FunctionContext functionContext)
{
    var log = functionContext.GetLogger("CreateNewOrder");
    log.LogInformation("C# function processed a CreateNewOrder request from the Dapr Runtime.");

    //print the received payload
    log.LogInformation($"Received Payload CreateNewOrder: {JsonSerializer.Serialize(payload)}");

    // payload must be of the format { "data": { "value": "some value" } }
    payload.TryGetProperty("data", out JsonElement data);

    return data;
}
```

Now you can invoke this function by using either the [test.http](test.http) file with your favorite REST client, or use the Dapr cli in a new command line terminal.  


Windows PowerShell
```powershell
dapr invoke --app-id functionapp --method CreateNewOrder --data '{ \"data\": {\"value\": { \"orderId\": \"42\" } } }'
```


In your terminal window, you should see logs indicating that the message was received and state was updated:

```
== APP == [TIMESTAMP] Executing 'Functions.CreateNewOrder' (Reason='(null)', Id=<ExecutionId>)
== APP == [TIMESTAMP] C# function processed a CreateNewOrder request from the Dapr Runtime.
== APP == [TIMESTAMP] Executed 'Functions.CreateNewOrder' (Succeeded, Id=<ExecutionId>, Duration=39ms)
```
----------------
In order to confirm the state is now persisted. You can now move to the next function:

```csharp
public static JsonElement Run(
    [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "RetrieveOrder")] HttpRequestData req,
    [DaprStateInput("%StateStoreName%", Key = "order")] JsonElement data, 
    FunctionContext functionContext)
{
    var log = functionContext.GetLogger("RetrieveOrder");
    log.LogInformation("C# function processed a RetrieveOrder request from the Dapr Runtime.");

    //print the fetched state value
    log.LogInformation($"Retrieved order: {JsonSerializer.Serialize(data)}");

    return data;
}
```

You can use [test.http](test.http) get to read the data from state store.

```
== APP == [TIMESTAMP] Executing 'Functions.RetrieveOrder' (Reason='(null)', Id=<ExecutionId>)
== APP == [TIMESTAMP] {"orderId":"42"}
== APP == [TIMESTAMP] C# function processed a RetrieveOrder request from the Dapr Runtime.
== APP == [TIMESTAMP] Executed 'Functions.RetrieveOrder' (Succeeded, Id=<ExecutionId>, Duration=186ms)
```
## Follow below links to deploy the function in ACA and Kubernetes
### [Deploy Dapr Enabled Function App to Azure Container Apps (ACA)](./deploy/aca/deploy-quickstart.bicep)

### [Deploy Dapr enabled function app to Kubernetes](../../deploy/kubernetes/kubernetes-deployment.md)