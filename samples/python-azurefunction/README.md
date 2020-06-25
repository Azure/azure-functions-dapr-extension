# Python Azure Function Sample

This tutorial will demonstrate how to use Azure Functions Python programming model to integrate with multiple Dapr components.  Please first go through the [samples](https://github.com/dapr/samples) to get some contexts on various Dapr building blocks as well as go through Azure Functions [hello-world sample](https://docs.microsoft.com/en-us/azure/azure-functions/functions-create-first-function-vs-code?pivots=programming-language-python) to familiarize with function programming model.
We'll be running a Darp'd function app locally:
1) Invoked by [Dapr Service Invocation](https://github.com/dapr/docs/tree/master/concepts/service-invocation) and persist/retrieve state using [Dapr State Management](https://github.com/dapr/components-contrib/tree/master/state)
2) Publish/consume message on a specific topic powered by [Dapr pub/sub](https://github.com/dapr/components-contrib/tree/master/pubsub) and `DaprPublish`/`DaprTopicTrigger`
3) Interact with [Dapr Bindings](https://github.com/dapr/components-contrib/tree/master/bindings) using `DaprBinding`

## Prerequisites
This sample requires you to have the following installed on your machine:
- [Setup Dapr](https://github.com/dapr/samples/tree/master/1.hello-world) : Follow [instructions](https://github.com/dapr/docs/blob/master/getting-started/environment-setup.md#environment-setup) to download and install the Dapr CLI and initialize Dapr.
- [Install Azure Functions Core Tool](https://github.com/Azure/azure-functions-core-tools/blob/master/README.md#windows)
- Install Python on your machine
    - This sample uses Python 3.7.6. Some nuance or issue is expected if using other version
- [Set up Python Environment in Visual Studio Code](https://code.visualstudio.com/docs/python/python-tutorial)
- [Install .NET Core SDK](https://dotnet.microsoft.com/download), used for install Dapr Extension for non .NET language
- [Run Kafka Docker Container Locally](https://github.com/dapr/samples/tree/master/5.bindings). The required Kafka files is located in `sample\dapr-kafka` directory.

# Step 1 - Understand the Settings 

Now that we've locally set up Dapr, clone the repo, then navigate to the python-azurefunction sample: 

```bash
git clone https://github.com/dapr/azure-functions-extension.git
cd samples/python-azurefunction
```
In this folder, you will find `local.settings.json`, which lists a few app settings we used in our trigger/binding attributes. 

```json
"StateStoreName": "statestore"
```

The `%` denotes an app setting value, for the following binding as an example:

```json 
{
    "type": "daprState",
    "stateStore": "%StateStoreName%",
    "direction": "out",
    "name": "order",
    "key": "order"
}
```

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

Build the function app:

```
dotnet build -o bin extensions.csproj
```

Note that this extensions.csproj file is required in order to reference the exception as a project rather than as an nuget package. To do the equivalent step with a published version of the extension on nuget.org, run the following step:

```
func extensions install -p Dapr.AzureFunctions.Extension -v <version>
```

Run function host with Dapr. `--components-path` flag specifies the directory stored all Dapr Components for this sample. They should be language ignostic.

```
dapr run --app-id functionapp --app-port 3001  --components-path ..\components\ -- func host start 
```

The command should output the dapr logs that look like the following:

```
Starting Dapr with id functionapp. HTTP Port: 3501. gRPC Port: 55377
Updating metadata for app command: func host start
You're up and running! Both Dapr and your app logs will appear here.
...
```

> **Note**: there are three ports in this service. The `--app-port`(3001) is where our function host listens on for any Dapr trigger. The `--port`(3501) is where Dapr APIs runs on as well as the  grpc port. The function port (default 7071) is where function host listens on for any HTTP triggred function using `api/{functionName}` URl path. All of these ports are configurable.
> 

### Troubleshootings:

1. Binding extension is installed
   
   If you see log message similar to:
    ```powershell
    The binding type(s) 'daprServiceInvocationTrigger, daprState' are not registered. Please ensure the type is correct and the binding extension is installed.
    ```
    Please check `host.json` file under this project and make sure it **DOES NOT** use `extensionBundle`. **REMOVE the following entry.** This extension bundle overwrites the manual extension installation step specified in the `extension.proj`. Dapr extension is not included in the bundle, so we have to import the Dapr Extension ourselves into this project. 

    ```json
    "extensionBundle": {
            "id": "Microsoft.Azure.Functions.ExtensionBundle",
            "version": "[1.*, 2.0.0)"
        }
    ```
2. Extension is not compatible with netcore31
   
   When running `dotnet build`, if you see the error below:
    ```
    Project Dapr.AzureFunctions.Extension is not compatible with netcore31 (.NETCore,Version=v3.1). Project Dapr.AzureFunctions.Extension supports: netstandard2.0 (.NETStandard,Version=v2.0)
    ```
    Make sure the target framework for `extension.proj` is netstandard 2.0. Since we have a project reference for our Dapr Extension, build step tries to restore `Dapr.AzureFunctions.Extension.csproj` as other non-compatible framework, but Dapr Extension is using netstandard 2.0 framework. If you swtich to a package reference, this should not be a concern since netstandard2.0 is compatible with netcore31.


# Step 3 - Understand the Sample

## 1. Service Invocation and State Management: Create New Order and Retrieve Order

Please read this [Azure Functions Python programming guide](https://docs.microsoft.com/en-us/azure/azure-functions/functions-triggers-bindings) for basic knowledge on triggers/bindings, logging, file structure and so on. Here we assume you are already familiar with `function.json` and `__init__.py` files. 

```python
import logging
import json
import azure.functions as func

def main(payload,
         order: func.Out[str]) -> None:
    logging.info('Python function processed a CreateNewOrder request from the Dapr Runtime.')  
    payload_json = json.loads(payload)
    logging.info(payload_json["data"])
    order.set(json.dumps(payload_json["data"]))
```

```json
{
  "scriptFile": "__init__.py",
  "bindings": [
    {
      "type": "daprServiceInvocationTrigger",
      "name": "payload",
      "direction": "in"
    },
    {
      "type": "daprState",
      "stateStore": "%StateStoreName%",
      "direction": "out",
      "name": "order",
      "key": "order"
    }
  ]
}

```
Data from triggers and bindings is bound to the function via method attributes using the name property defined in the function.json file. The function.json file describes we will use a `daprServiceInvocationTrigger` trigger named as `payload` and a `daprState` output binding named as `order`.  This function will be invoke when the function host receive a `CreateNewOrder` request from Dapr runtime. The actual payload content will be bound to this `payload` parameter when passing into the function. In the function, we use [azure.functions.Out](https://docs.microsoft.com/en-us/python/api/azure-functions/azure.functions.out?view=azure-python) interface to explicitly declare the attribute types of `order`. Then we binds the content of `data` property to `order` binding by calling `set()`. The `DaprState` *output binding* will persist the order into the state store by serializing `order` object into a state arrary format and posting it to `http://localhost:${daprPort}/v1.0/state/${stateStoreName}`.

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

**Note**: in this sample, `daprServiceInvocationTrigger` binding in the function.json does not specify the method name, so it defaults to use the FunctionName. Alternatively, we can use `methodName` field to specify the service invocation method name that your function should respond. In this case, then we need to use the following command:

```json
// function.json
    {
      "type": "daprServiceInvocationTrigger",
      "name": "payload",
      "methodName": "newOrder",
      "direction": "in"
    }

```

```powershell
dapr invoke --app-id functionapp --method newOrder --payload "{\"data\": { \"orderId\": \"41\" } }"
```

In your terminal window, you should see logs indicating that the message was received and state was updated:

```
== APP == [TIMESTAMP] Executing 'CreateNewOrder' (Reason='', Id=<ExecutionId>)
== APP == [TIMESTAMP] Python function processed a CreateNewOrder request from the Dapr Runtime.
== APP == [TIMESTAMP] Executed 'CreateNewOrder' (Succeeded, Id=<ExecutionId>)
```
----------------
In order to confirm the state is now persisted. We now can move to our next function:

```python
def main(payload, data: str) -> None:
    logging.info('Python function processed a RetrieveOrder request from the Dapr Runtime.')
    logging.info(data)
```

```json
{
  "scriptFile": "__init__.py",
  "bindings": [
    {
      "type": "daprServiceInvocationTrigger",
      "name": "payload",
      "direction": "in"
    },
    {
      "type": "daprState",
      "direction": "in",
      "key": "order",
      "stateStore": "%StateStoreName%",
      "name": "data"
    }
  ]
}
```

Similarly, the function will be triggered by any `RetrieveOrder` service invocation request.Here we use `DaprState` *input binding* to fetch the latest value of the key `order` and bind the value to string object `data` before we start exectuing the function block.

In your terminal window, you should see logs to confirm the expected result:

```
== APP == [TIMESTAMP]  Executing 'RetrieveOrder' (Reason='', Id=<ExecutionId>)
== APP == [TIMESTAMP]  Python function processed a RetrieveOrder request from the Dapr Runtime.
== APP == [TIMESTAMP]  {"orderId":"41"}
== APP == [TIMESTAMP]  Executed 'RetrieveOrder' (Succeeded, Id=<ExecutionId>)
```


## 2. Pub/Sub: TransferEventBetweenTopics and PrintTopicMessage

```python
def main(subEvent,
         pubEvent: func.Out[bytes]) -> None:
    logging.info('Python function processed a TransferEventBetweenTopics request from the Dapr Runtime.')
    subEvent_json = json.loads(subEvent)
    payload = "Transfer from Topic A: " + str(subEvent_json["data"])
    pubEvent.set(json.dumps({"payload": payload }))

```

```json
{
  "scriptFile": "__init__.py",
  "bindings": [
    {
      "type": "daprTopicTrigger",
      "topic": "A",
      "name": "subEvent",
      "direction": "in",
      "dataType": "string"
    },
    {
      "type": "daprPublish",
      "direction": "out",
      "name": "pubEvent",
      "topic": "B"
    }
  ]
}
```

Here we use `daprTopicTrigger` to subscribe to topic `A`, so whenever a message is published on topic `A`, the message will bind to `subEvent`. Please see the [`CloudEvent`](https://github.com/cloudevents/spec/blob/master/spec.md) for details. 


> **Note**: Alternatively, any other JSON-serializable datatype binds directly to the data field of the cloud event. For example, int, double, and custom “POCO” types can be used as the trigger type and will be deserialized from the event’s data field. 

Then we use `daprPublish` *output binding* to publish a new event to topic `B`.


At the same time, we also have a function that subscribes to topic `B`, and it will simply just print the message content when an event arrives. 

Then let's see what will happen if we publish a message to topic A using the Dapr cli:

```powershell
dapr publish --topic A --data 'This is a test'
```

The Dapr logs should show the following:
```
== APP == [TIMESTAMP] Executing 'TransferEventBetweenTopics' (Reason='',Id={ExectuionId})
== APP == [TIMESTAMP] Python Function processed a TransferEventBetweenTopics request from the Dapr Runtime.
== APP == [TIMESTAMP] Executed 'TransferEventBetweenTopics' (Succeeded, Id={ExectuionId})
== APP == [TIMESTAMP] Executing 'PrintTopicMessage' (Reason='', Id={AnotherExectuionId})
== APP == [TIMESTAMP] Python Function processed a PrintTopicMessage request from the Dapr Runtime.
== APP == [TIMESTAMP] Topic B received a message: Transfer from Topic A: This is a test.
== APP == [TIMESTAMP] Executed 'PrintTopicMessage' (Succeeded, Id={AnotherExectuionId})
```

## 3. Dapr Binding: 
Next we will show how this extension integrates with Dapr Binding component. Here we uses Kafka binding as an example. Please refer to [Dapr Bindings Sample](https://github.com/dapr/samples/tree/master/5.bindings) to spin up your the Kafka locally. In the example below, we use `daprBindingTrigger` to have our function triggerred when a new message arrives at Kafka.

```python
def main(triggerData: str) -> None:
    logging.info('Hello from Kafka!')
    logging.info('Trigger data: ' + triggerData)
```
```json
{
  "scriptFile": "__init__.py",
  "bindings": [
    {
      "type": "daprBindingTrigger",
      "bindingName": "sample-topic",
      "name": "triggerData",
      "direction": "in"
    }
  ]
}
```

Now let's look at how our function uses `DaprBinding` to push messages into our Kafka instance. In the function.json, it sepcifies the `operation` and `bindingName` required for this **output binding**.

```python
def main(args, messages: func.Out[bytes]) -> None:
    logging.info('Python processed a SendMessageToKafka request from the Dapr Runtime.')
    messages.set(json.dumps({"data": args}))
```

```json
{
  "bindings": [
    {
      "type": "daprServiceInvocationTrigger",
      "name": "args",
      "direction": "in"
    },
    {
      "type": "daprBinding",
      "direction": "out",
      "bindingName": "%KafkaBindingName%",
      "operation": "create",
      "name": "messages"
    }
  ]
}
```

`DaprBinding` *output binding* sends the payload to the `sample-topic` Kafka Dapr binding.

Now we can use service invocation to invoke this function:

```powershell
dapr invoke --app-id functionapp --method SendMessageToKafka --payload '{\"message\": \"hello!\" }'
```

The Dapr'd function logs should show the following:
```
== APP == [TIMESTAMP] Executing 'SendMessageToKafka' (Reason='', Id=<ExecutionId>)
== APP == [TIMESTAMP] Python processed processed a a SendMessageToKafka request from the Dapr Runtime.
== APP == [TIMESTAMP] Executed 'SendMessageToKafka' (Succeeded, Id=<ExecutionId>)
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
```