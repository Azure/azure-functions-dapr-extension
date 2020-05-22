# .NET Azure Function Sample

This tutorial will demonstrate how to use Azure Functions programming model to integrate with multiple Dapr components. Please first go through the [samples](https://github.com/dapr/samples) to get some contexts on various Dapr building blocks as well as go through Azure Functions [hello-world sample](https://docs.microsoft.com/en-us/azure/azure-functions/functions-create-first-function-vs-code?pivots=programming-language-csharp) to familiarize with function programming model.
We'll be running a Darp'd function app locally:
1) Invoked by [Dapr Service Invocation](https://github.com/dapr/docs/tree/master/concepts/service-invocation) and persist/retrieve state using [Dapr State Management](https://github.com/dapr/components-contrib/tree/master/state)
2) Publish/consume message on a specific topic powered by [Dapr pub/sub](https://github.com/dapr/components-contrib/tree/master/pubsub) and `DaprPublish`/`DaprTopicTrigger`
3) Interact with [Dapr Bindings](https://github.com/dapr/components-contrib/tree/master/bindings) using `DaprBinding`

## Prerequisites
This sample requires you to have the following installed on your machine:
- [Setup Dapr](https://github.com/dapr/samples/tree/master/1.hello-world) : Follow [instructions](https://github.com/dapr/docs/blob/master/getting-started/environment-setup.md#environment-setup) to download and install the Dapr CLI and initialize Dapr.
- [Install Azure Functions Core Tool](https://github.com/Azure/azure-functions-core-tools/blob/master/README.md#windows)
- [Run Kafka Docker Container Locally](https://github.com/dapr/samples/tree/master/5.bindings)

# Step 1 - Understand the Settings 

Now that we've locally set up Dapr, clone the repo, then navigate to the dotnet-azurefunction sample: 

```bash
git clone https://github.com/dapr/azure-functions-extension.git
cd samples/dotnet-azurefunction
```
In this folder, you will find `local.settings.json`, which lists a few app settings we used in our trigger/binding attributes. 

```json
"StateStoreName": "statestore"
```

The `%` denotes an app setting value, for the following binding as an example:

`[DaprState("%StateStoreName%", Key = "order")]`

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

```
dapr run --app-id functionapp --app-port 3001 --port 3501 func host start
```

The command should output the dapr logs that look like the following:

```
Starting Dapr with id functionapp. HTTP Port: 3501. gRPC Port: 55377
Updating metadata for app command: func host start
You're up and running! Both Dapr and your app logs will appear here.
...
```

> **Note**: there are four ports in this service. The `--app-port`(3001) is where our function host listens on for any Dapr trigger. The `--port`(3501) is where Dapr APIs runs on as well as the  grpc port. The function port (default 7071) is where function host listens on for any HTTP triggred function using `api/{functionName}` URl path. All of these ports are configurable.
> 


# Step 3 - Understand the Sample

## 1. Service Invocation and State Management: Create New Order and Retrieve Order

```csharp
[FunctionName("CreateNewOrder")]
public static void Run(
    [DaprServiceInvocationTrigger] JObject payload, 
    [DaprState("%StateStoreName%", Key = "order")] out object order,
    ILogger log)
{
    log.LogInformation("C# function processed a CreateNewOrder request from the Dapr Runtime.");

    order = payload["data"];
}
```

Here we use `DaprServiceInvocationTrigger` to receive and handle `CreateNewOrder` request. We first log that this function is successfully triggered. Then we binds the content to the `order` object. The `DaprState` *output binding* will persist the order into the state store by serializing `order` object into a state arrary format and posting it to `http://localhost:${daprPort}/v1.0/state/${stateStoreName}`.

Now we can invoke this function by using the Dapr cli in a new command line terminal.  

Windows Command Prompt
```sh
dapr invoke --app-id functionapp --method CreateNewOrder --payload "{\"data\": { \"orderId\": \"41\" } }"
```

Windows PowerShell
```powershell
dapr invoke --app-id functionapp --method CreateNewOrder --payload '{\"data\": { \"orderId\": \"41\" } }'
```

Linux or MacOS
```sh
dapr invoke --app-id functionapp --method CreateNewOrder --payload '{"data": { "orderId": "41" } }'
```

We can also do this using the Visual Studio Code [Rest Client Plugin](https://marketplace.visualstudio.com/items?itemName=humao.rest-client)

```http
POST  http://localhost:3501/v1.0/invoke/functionapp/method/CreateNewOrder

{
    "data": {
        "orderId": "42"
    } 
}
```

**Note**: in this sample, `DaprServiceInvocationTrigger` attribute does not specify the method name, so it defaults to use the FunctionName. Alternatively, we can use `[DaprServiceInvocationTrigger(MethodName = "newOrder")]` to specify the service invocation method name that your function should respond. In this case, then we need to use the following command:

```powershell
dapr invoke --app-id nodeapp --method newOrder --payload "{\"data\": { \"orderId\": \"41\" } }"
```

In your terminal window, you should see logs indicating that the message was received and state was updated:

```
== APP == [TIMESTAMP] Executing 'CreateNewOrder' (Reason='', Id=<ExecutionId>)
== APP == [TIMESTAMP] C# function processed a CreateNewOrder request from the Dapr Runtime.
== APP == [TIMESTAMP] Executed 'CreateNewOrder' (Succeeded, Id=<ExecutionId>)
```
----------------
In order to confirm the state is now persisted. We now can move to our next function:

```csharp
[FunctionName("RetrieveOrder")]
public static void Run(
    [DaprServiceInvocationTrigger] object args,
    [DaprState("%StateStoreName%", Key = "order")] string data,
    ILogger log)
{
    log.LogInformation("C# function processed a RetrieveOrder request from the Dapr Runtime.");

    // print the fetched state value
    log.LogInformation(data);
}
```

Similarly, the function will be triggered by any `RetrieveOrder` service invocation request. However, here we use `DaprState` *input binding* to fetch the latest value of the key `order` and bind the value to string object `data`' before we start exectuing the function block.

In your terminal window, you should see logs to confirm the expected result:

```
== APP == [TIMESTAMP]  Executing 'RetrieveOrder' (Reason='', Id=<ExecutionId>)
== APP == [TIMESTAMP]  C# function processed a RetrieveOrder request from the Dapr Runtime.
== APP == [TIMESTAMP]  {"orderId":"41"}
== APP == [TIMESTAMP]  Executed 'RetrieveOrder' (Succeeded, Id=<ExecutionId>)
```


## 2. Pub/Sub: TransferEventBetweenTopics and PrintTopicMessage

```csharp
[FunctionName("TransferEventBetweenTopics")]
public static void Run(
    [DaprTopicTrigger(Topic = "A")] CloudEvent subEvent,
    [DaprPublish(Topic = "B")] out DaprPubSubEvent pubEvent,
    ILogger log)
{
    log.LogInformation("C# function processed a TransferEventBetweenTopics request from the Dapr Runtime.");


    pubEvent = new DaprPubSubEvent("Transfer from Topic A: " + subEvent.Data);
}
```

Here we use `DaprTopicTrigger` to subscribe to topic `A`, so whenever a message is published on topic `A`, the message will bind to `CloudEvent` `subEvent`. Please see the [`CloudEvent`](https://github.com/cloudevents/spec/blob/master/spec.md) for details. 


> **Note**: Alternatively, any other JSON-serializable datatype binds directly to the data field of the cloud event. For example, int, double, and custom “POCO” types can be used as the trigger type and will be deserialized from the event’s data field. 

Then we use `DaprPublish` *output binding* to publish a new event to topic `B` using the strongly-typed `DaprPubSubEvent` class, or it can be written using the attribute `[DaprPublish(Topic = "B")] out object pubEvent`:

```csharp
    pubEvent = "Transfer from Topic A:" + subEvent.Data;
```

At the same time, we also have a function that subscribes to topic `B`, and it will simply just print the message content when an event arrives. 

```csharp
[FunctionName("PrintTopicMessage")]
public static void Run(
    [DaprTopicTrigger(Topic = "B")] CloudEvent subEvent,
    ILogger log)
{
    log.LogInformation("C# function processed a PrintTopicMessage request from the Dapr Runtime.");
    log.LogInformation($"Topic B received a message: {subEvent.Data}.");
}
```

Then let's see what will happen if we publish a message to topic A using the Dapr cli:

```powershell
dapr publish --topic A --payload 'This is a test'
```

The Dapr logs should show the following:
```
== APP == [TIMESTAMP] Executing 'TransferEventBetweenTopics' (Reason='',Id={ExectuionId})
== APP == [TIMESTAMP] C# function processed a TransferEventBetweenTopics request from the Dapr Runtime.
== APP == [TIMESTAMP] Executed 'TransferEventBetweenTopics' (Succeeded, Id={ExectuionId})
== APP == [TIMESTAMP] Executing 'PrintTopicMessage' (Reason='', Id={AnotherExectuionId})
== APP == [TIMESTAMP] C# function processed a PrintTopicMessage request from the Dapr Runtime.
== APP == [TIMESTAMP] Topic B received a message: Transfer from Topic A: This is a test.
== APP == [TIMESTAMP] Executed 'PrintTopicMessage' (Succeeded, Id={AnotherExectuionId})
```

## 3. Dapr Binding: 
Next we will show how this extension integrates with Dapr Binding component. Here we uses Kafka binding as an example. Please refer to [Dapr Bindings Sample](https://github.com/dapr/samples/tree/master/5.bindings) to spin up your the Kafka locally. In the example below, we use `DaprBindingTrigger` to have our function triggerred when a new message arrives at Kafka.

```csharp
[FunctionName("ConsumeMessageFromKafka")]
public static void Run(
    // Note: the value of BindingName must match the binding name in components/kafka-bindings.yaml
    [DaprBindingTrigger(BindingName = "%KafkaBindingName%")] JObject triggerData,
    ILogger log)
{
    log.LogInformation("Hello from Kafka!");

    log.LogInformation($"Trigger data: {triggerData}");
}
```
Now let's look at how our function uses `DaprBinding` to push messages into our Kafka instance.

```csharp
[FunctionName("SendMessageToKafka")]
public static async void Run(
    [DaprServiceInvocationTrigger] JObject payload,
    [DaprBinding(BindingName = "%KafkaBindingName%")] IAsyncCollector<object> messages,
    ILogger log)
{
    log.LogInformation("C# HTTP trigger function processed a request.");

    await messages.AddAsync(payload);
}
```
`DaprBinding` *output binding* sends the payload to the `sample-topic` Kafka Dapr binding. `IAsyncCollector<object>` allows you to send multiple message by calling `AddAsync` with different payloads. 

Now we can use service invocation to invoke this function:

```powershell
dapr invoke --app-id functionapp --method SendMessageToKafka --payload '{\"message\": \"hello!\" }'
```

The Dapr'd function logs should show the following:
```
== APP == [TIMESTAMP] Executing 'SendMessageToDaprBinding' (Reason='', Id=<ExecutionId>)
== APP == [TIMESTAMP] C# HTTP trigger function processed a request.
== APP == [TIMESTAMP] Executed 'SendMessageToDaprBinding' (Succeeded, Id=<ExecutionId>)
```

Since we have both functions deployed in the same app, you should also see we have consumed the message by see the folowing:
```
== APP == [TIMESTAMP] Executing 'ConsumeMessageFromKafka' (Reason='', Id=<ExecutionId>)
== APP == [TIMESTAMP] Hello from Kafka!
== APP == [TIMESTAMP] Trigger data: { message: 'hello!' }
== APP == [TIMESTAMP] Executed 'ConsumeMessageFromKafka' (Succeeded, Id=<ExecutionId>)
```


# Step 4 - Cleanup

To stop your services from running, simply stop the "dapr run" process. Alternatively, you can spin down each of your services with the Dapr CLI "stop" command. For example, to spin down both services, run these commands in a new command line terminal: 

```bash
dapr stop --app-id functionapp
dapr stop --app-id nodeapp
```