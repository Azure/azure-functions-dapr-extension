# Python V2 Azure Function Sample

This tutorial will demonstrate how to use Azure Functions Python V2 programming model to integrate with multiple Dapr components.  Please first go through the [samples](https://github.com/dapr/samples) to get some contexts on various Dapr building blocks as well as go through Azure Functions [hello-world sample](https://docs.microsoft.com/en-us/azure/azure-functions/functions-create-first-function-vs-code?pivots=programming-language-python) to familiarize with function programming model.
We'll be running a Darp'd function app locally:
1) Invoked by [Dapr Service Invocation](https://docs.dapr.io/developing-applications/building-blocks/service-invocation/service-invocation-overview/) and persist/retrieve state using [Dapr State Management](https://github.com/dapr/components-contrib/tree/master/state)
2) Publish/consume message on a specific topic powered by [Dapr pub/sub](https://github.com/dapr/components-contrib/tree/master/pubsub) and `DaprPublish`/`DaprTopicTrigger`
3) Interact with [Dapr Bindings](https://github.com/dapr/components-contrib/tree/master/bindings) using `DaprBinding`

## Prerequisites
This sample requires you to have the following installed on your machine:
- Setup Dapr : Follow instructions to [download and install the Dapr CLI](https://docs.dapr.io/getting-started/install-dapr-cli/) and [initialize Dapr](https://docs.dapr.io/getting-started/install-dapr-selfhost/).
- [Install Azure Functions Core Tool](https://github.com/Azure/azure-functions-core-tools/blob/master/README.md#windows)
- Install Python on your machine
    - This sample uses Python 3.8. Some nuance or issue is expected if using other version
- [Set up Python Environment in Visual Studio Code](https://code.visualstudio.com/docs/python/python-tutorial)
- [Install .NET Core SDK](https://dotnet.microsoft.com/download), used for install Dapr Extension for non .NET language
- Run Kafka Docker Container locally with below command.

  ```
  cd samples\dapr-kafka
  docker-compose -f docker-compose-single-kafka.yml up -d
  ```

# Step 1 - Understand the Settings 

Now that we've locally set up Dapr, clone the repo, then navigate to the python-v2-azurefunction sample: 

```bash
git clone https://github.com/dapr/azure-functions-extension.git
cd samples/python-v2-azurefunction
```
In this folder, you will find `local.settings.json`, which lists a few app settings used in the trigger/binding attributes.

```json
"StateStoreName": "statestore"
```

The `%` denotes an app setting value, for the following binding as an example:

```python
@dapp.dapr_state_output(arg_name="state", state_store="%StateStoreName%", key="order")
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

# DaprBlueprint
The Python v2 programming model introduces the concept of blueprints. A blueprint is a new class that's instantiated to register functions outside of the core function application. The functions registered in blueprint instances aren't indexed directly by the function runtime. To get these blueprint functions indexed, the function app needs to register the functions from blueprint instances.

## Using blueprints provides the following benefits:

- Lets you break up the function app into modular components, which enables you to define functions in multiple Python files and divide them into different components per file.
- Provides extensible public function app interfaces to build and reuse your own APIs.

# Step 2 - Run Function App with Dapr

Build the function app:

```
dotnet build -o bin/ extensions.csproj
```

Note that this extensions.csproj file is required in order to reference the exception as a project rather than as an nuget package. To do the equivalent step with a published version of the extension on nuget.org, run the following step:

```
func extensions install -p Microsoft.Azure.WebJobs.Extensions.Dapr -v <version>
```

Run function host with Dapr. `--components-path` flag specifies the directory stored all Dapr Components for this sample. They should be language ignostic.

Windows
```
dapr run --app-id functionapp --app-port 3001  --components-path ..\components\ -- func host start 
```

Linux/MacOS
```
dapr run --app-id functionapp --app-port 3001  --components-path ../components/ -- func host start 
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

### Troubleshootings:

1. Binding extension is installed
   
   If you see log message similar to:
    ```powershell
    The binding type(s) 'daprServiceInvocationTrigger, daprState' are not registered. Please ensure the type is correct and the binding extension is installed.
    ```
    Please check `host.json` file under this project and make sure it **DOES NOT** use `extensionBundle`. **REMOVE the following entry.** This extension bundle overwrites the manual extension installation step specified in the `extension.proj`. Dapr extension is not included in the bundle, and needs to be imported separately into this project.

    ```json
    "extensionBundle": {
            "id": "Microsoft.Azure.Functions.ExtensionBundle",
            "version": "[1.*, 2.0.0)"
        }
    ```
2. Extension is not compatible with netcore31
   
   When running `dotnet build`, if you see the error below:
    ```
    Project Microsoft.Azure.WebJobs.Extensions.Dapr is not compatible with netcore31 (.NETCore,Version=v3.1). Project Microsoft.Azure.WebJobs.Extensions.Dapr supports: netstandard2.0 (.NETStandard,Version=v2.0)
    ```
    Make sure the target framework for `extension.proj` is netstandard 2.0. Since a project reference for the Dapr Extension exists, build step tries to restore `Microsoft.Azure.WebJobs.Extensions.Dapr.csproj` as other non-compatible framework, but Dapr Extension is using netstandard 2.0 framework. If you switch to a package reference, this should not be a concern since netstandard2.0 is compatible with netcore31.


# Step 3 - Understand the Sample

## 1. Service Invocation and State Management: Create New Order and Retrieve Order

Please read [Azure Functions Python programming guide](https://docs.microsoft.com/en-us/azure/azure-functions/functions-triggers-bindings) for basic knowledge on triggers/bindings, logging, file structure and so on. Also, familiarize yourself with `function.json` and `__init__.py` files.

```python
import json
import azure.functions as func
import logging

dapp = func.DaprFunctionApp()

@dapp.function_name(name="CreateNewOrder")
@dapp.dapr_service_invocation_trigger(arg_name="payload", method_name="CreateNewOrder")
@dapp.dapr_state_output(arg_name="state", state_store="statestore", key="order")
def main(payload: str, state: func.Out[str] ) :
    # request body must be passed this way '{\"value\": { \"key\": \"some value\" } }'
    logging.info('Python function processed a CreateNewOrder request from the Dapr Runtime.')
    if payload is not None:
        logging.info(payload)
        state.set(payload)
    else:
        logging.info('payload is none')
```
Data from triggers and bindings is bound to the function via method attributes using the name property defined in the decorator. The decorator describes a `daprServiceInvocationTrigger` trigger wit argument named as `payload` and a `daprState` output binding with key named as `order`. This function will be invoke when the function host receive a `CreateNewOrder` request from Dapr runtime. The actual payload content will be bound to this `payload` parameter when passing into the function. In the function, [azure.functions.Out](https://docs.microsoft.com/en-us/python/api/azure-functions/azure.functions.out?view=azure-python) interface is used to explicitly declare the attribute types of `order`. Then the content of `data` property is bound to the `order` binding by calling `set()`. The `DaprState` *output binding* will persist the order into the state store by serializing `order` object into a state arrary format and posting it to `http://localhost:${daprPort}/v1.0/state/${stateStoreName}`.

You can invoke this function by using the Dapr cli in a new command line terminal.  

Windows Command Prompt
```sh
dapr invoke --app-id functionapp --method CreateNewOrder --data "{\"value\": { \"orderId\": \"41\" } }"
```

Windows PowerShell
```powershell
dapr invoke --app-id functionapp --method CreateNewOrder --data '{\"value\": { \"orderId\": \"41\" } }'
```

Linux or MacOS
```sh
dapr invoke --app-id functionapp --method CreateNewOrder --data '{"value": { "orderId": "41" } }'
```

You can also do this using the Visual Studio Code [Rest Client Plugin](https://marketplace.visualstudio.com/items?itemName=humao.rest-client)

```http
POST  http://localhost:3501/v1.0/invoke/functionapp/method/CreateNewOrder

{
    "value": {
        "orderId": "42"
    } 
}
```

**Note**: in this sample, `daprServiceInvocationTrigger` binding specifies the method name, if it is not specified then it will default to use the FunctionName.

In your terminal window, you should see logs indicating that the message was received and state was updated:

```
== APP == [TIMESTAMP] Executing 'CreateNewOrder' (Reason='', Id=<ExecutionId>)
== APP == [TIMESTAMP] Python function processed a CreateNewOrder request from the Dapr Runtime.
== APP == [TIMESTAMP] Executed 'CreateNewOrder' (Succeeded, Id=<ExecutionId>)
```
----------------
In order to confirm the state is now persisted, you can move to the next function:

```python
@dapp.function_name(name="RetrieveOrder")
@dapp.dapr_service_invocation_trigger(arg_name="payload", method_name="RetrieveOrder")
@dapp.dapr_state_input(arg_name="data", state_store="statestore", key="order")
def main(payload, data: str) :
    # Function should be invoked with this command: dapr invoke --app-id functionapp --method RetrieveOrder  --data '{}'
    logging.info('Python function processed a RetrieveOrder request from the Dapr Runtime.')
    logging.info(data)
```

Similarly, the function will be triggered by any `RetrieveOrder` service invocation request such as:
```
dapr invoke --app-id functionapp --method RetrieveOrder --data '{}'
```

.Here `DaprState` *input binding* is used to fetch the latest value of the key `order` and bind the value to string object `data` before exectuing the function block.

In your terminal window, you should see logs to confirm the expected result:

```
== APP == [TIMESTAMP]  Executing 'RetrieveOrder' (Reason='', Id=<ExecutionId>)
== APP == [TIMESTAMP]  Python function processed a RetrieveOrder request from the Dapr Runtime.
== APP == [TIMESTAMP]  {"orderId":"41"}
== APP == [TIMESTAMP]  Executed 'RetrieveOrder' (Succeeded, Id=<ExecutionId>)
```


## 2. Pub/Sub: TransferEventBetweenTopics and PrintTopicMessage

```python
# Dapr topic trigger
@dapp.function_name(name="PrintTopicMessage")
@dapp.dapr_topic_trigger(arg_name="subEvent", pub_sub_name="%PubSubName%", topic="B", route="B")
def main(subEvent) -> None:
    logging.info('Python function processed a PrintTopicMessage request from the Dapr Runtime.')
    subEvent_json = json.loads(subEvent)
    logging.info("Topic B received a message: " + subEvent_json["data"])

# Dapr publish output
# Dapr topic trigger with dapr_publish_output
@dapp.function_name(name="TransferEventBetweenTopics")
@dapp.dapr_topic_trigger(arg_name="subEvent", pub_sub_name="%PubSubName%", topic="A", route="A")
@dapp.dapr_publish_output(arg_name="pubEvent", pub_sub_name="%PubSubName%", topic="B")
def main(subEvent, pubEvent: func.Out[bytes]) -> None:
    logging.info('Python function processed a TransferEventBetweenTopics request from the Dapr Runtime.')
    subEvent_json = json.loads(subEvent)
    payload = "Transfer from Topic A: " + str(subEvent_json["data"])
    pubEvent.set(json.dumps({"payload": payload}).encode('utf-8'))
```

Here, `daprTopicTrigger` is used to subscribe to topic `A`, so whenever a message is published on topic `A`, the message will bind to `subEvent`. Please see the [`CloudEvent`](https://github.com/cloudevents/spec/blob/master/spec.md) for details.

> **Note**: Alternatively, any other JSON-serializable datatype binds directly to the data field of the cloud event. For example, int, double, and custom “POCO” types can be used as the trigger type and will be deserialized from the event’s data field.

> **Note**: route is not a mandatory parameter, however, currently it is being passed due to a bug in python library where it considers route as HttpTrigger parameter, once this issue is fixed, route will be optional parameter. For now, it must be passed.

Then, `daprPublish` *output binding* is used to publish a new event to topic `B`.


Also, the function below subscribes to topic `B`, and it will simply just print the message content when an event arrives. 

You can publish a message to topic A using the Dapr cli:

```powershell
dapr publish  --publish-app-id functionapp --pubsub messagebus --topic A --data 'This is a test'
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
This sections describes how this extension integrates with Dapr Binding component. Here Kafka binding is used as an example. Please refer to [Dapr Bindings Sample](https://github.com/dapr/quickstarts/tree/master/bindings) to spin up your the Kafka locally. In the example below, `daprBindingTrigger` is used to have the azure function triggerred when a new message arrives at Kafka.

```python
# Dapr binding trigger
@dapp.function_name(name="ConsumeMessageFromKafka")
@dapp.dapr_binding_trigger(arg_name="triggerData", binding_name="%KafkaBindingName%")
def main(triggerData: str) -> None:
    logging.info('Python function processed a ConsumeMessageFromKafka request from the Dapr Runtime.')
    logging.info('Trigger data: ' + triggerData)
```

Now let's look at how our function uses `DaprBinding` to push messages into our Kafka instance. In the function.json, it sepcifies the `operation` and `bindingName` required for this **output binding**.

```python
# Dapr binding output
# Dapr state output binding with http dapr_service_invocation_trigger
@dapp.function_name(name="SendMessageToKafka")
@dapp.dapr_service_invocation_trigger(arg_name="payload", method_name="SendMessageToKafka")
@dapp.dapr_binding_output(arg_name="messages", binding_name="%KafkaBindingName%", operation="create")
def main(payload: str, messages: func.Out[bytes]) -> None:
    logging.info('Python processed a SendMessageToKafka request from the Dapr Runtime.')
    messages.set(json.dumps({"data": payload}).encode('utf-8'))
```

`DaprBinding` *output binding* sends the payload to the `sample-topic` Kafka Dapr binding.

You can use service invocation to invoke this function:

Windows
```powershell
dapr invoke --app-id functionapp --method SendMessageToKafka --data '{\"message\": \"hello!\" }'
```

Linux/MacOS
```shell
dapr invoke --app-id functionapp --method SendMessageToKafka --data '{"message": "hello!" }'
```

The Dapr'd function logs should show the following:
```
== APP == [TIMESTAMP] Executing 'SendMessageToKafka' (Reason='', Id=<ExecutionId>)
== APP == [TIMESTAMP] Python processed processed a a SendMessageToKafka request from the Dapr Runtime.
== APP == [TIMESTAMP] Executed 'SendMessageToKafka' (Succeeded, Id=<ExecutionId>)
```

Since both functions have been deployed in the same app, you can see the logs below which indicate that the message has been consumed:
```
== APP == [TIMESTAMP] Executing 'ConsumeMessageFromKafka' (Reason='', Id=<ExecutionId>)
== APP == [TIMESTAMP] Hello from Kafka!
== APP == [TIMESTAMP] Trigger data: { message: 'hello!' }
== APP == [TIMESTAMP] Executed 'ConsumeMessageFromKafka' (Succeeded, Id=<ExecutionId>)
```
## 4. Dapr Secret: 
This section demonstrates how `DaprSecret` **input binding** integrates with Dapr Secret component. Here, Local file Secret Store is used and you can follow the setup instructions at [Local file secret store](https://docs.dapr.io/operations/components/setup-secret-store/supported-secret-stores/file-secret-store/) to configure a secret named "my-secret".

Please refer to [Dapr Secret Store doc](https://docs.dapr.io/operations/components/setup-secret-store/) to set up other supported secret stores.

```python
# Dapr secret input binding with http dapr_service_invocation_trigger
@dapp.function_name(name="RetrieveSecret")
@dapp.dapr_service_invocation_trigger(arg_name="payload", method_name="RetrieveSecret")
@dapp.dapr_secret_input(arg_name="secret", secret_store_name="localsecretstore", key="my-secret", metadata="metadata.namespace=default")
def main(payload, secret: str) :
    # Function should be invoked with this command: dapr invoke --app-id functionapp --method RetrieveSecret  --data '{}'
    logging.info('Python function processed a RetrieveSecret request from the Dapr Runtime.')
    secret_dict = json.loads(secret)

    for key in secret_dict:
        logging.info("Stored secret: Key = " + key +
                     ', Value = ' + secret_dict[key])
```

`DaprSecret` *input binding* retreives the secret named by `my-secret` and binds to `secret` as a dictionary object. Since Local Secret Store supports multiple keys in a secret, the secret dictionary could include multiple key value pairs and you can access the specfic one. For other secret store only supports one keys, the dictionary will only contain one key value pair where key matches the secret name, namely `my-secret` in this example, and the actual secret value is in the property value. This sample just simply prints out all secrets, but please do not log any real secret in your production code.

You can retrieve the secret by invoking the RetrieveSecret function using the command:-
```
dapr invoke --app-id functionapp --method RetrieveSecret --data '{}'
```

Some secret stores need a metadata string to be provided. In order to specify multiple metadata fields, join them by `&`, see the below [Hashicorp Vault](https://docs.dapr.io/operations/components/setup-secret-store/supported-secret-stores/hashicorp-vault/) example.
```json
"metadata": "metadata.version_id=15&metadata.version_stage=AAA"
```


# Step 4 - Cleanup

To stop your services from running, simply stop the "dapr run" process. Alternatively, you can spin down each of your services with the Dapr CLI "stop" command. For example, to spin down both services, run these commands in a new command line terminal: 

```bash
dapr stop --app-id functionapp
```