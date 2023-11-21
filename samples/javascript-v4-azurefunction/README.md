# Node V4 Azure Function Sample

This tutorial will demonstrate how to use Azure Functions programming model to integrate with multiple Dapr components. Please first go through the [samples](https://github.com/dapr/samples) to get some contexts on various Dapr building blocks as well as go through Azure Functions [hello-world sample](https://docs.microsoft.com/en-us/azure/azure-functions/functions-create-first-function-vs-code?pivots=programming-language-javascript) to familiarize with function programming model.
We'll be running a Darp'd function app locally:
1) Invoked by [Dapr Service Invocation](https://docs.dapr.io/developing-applications/building-blocks/service-invocation/) and persist/retrieve state using [Dapr State Management](https://github.com/dapr/components-contrib/tree/master/state)
2) Publish/consume message on a specific topic powered by [Dapr pub/sub](https://github.com/dapr/components-contrib/tree/master/pubsub) and `DaprPublish`/`DaprTopicTrigger`
3) Interact with [Dapr Bindings](https://github.com/dapr/components-contrib/tree/master/bindings) using `DaprBinding`

## Prerequisites
This sample requires you to have the following installed on your machine:
- Setup Dapr : Follow instructions to [download and install the Dapr CLI](https://docs.dapr.io/getting-started/install-dapr-cli/) and [initialize Dapr](https://docs.dapr.io/getting-started/install-dapr-selfhost/).
- [Install Azure Functions Core Tool](https://github.com/Azure/azure-functions-core-tools/blob/master/README.md#windows)
- [Run Kafka Docker Container locally](../../samples/dapr-kafka/README.md)

# Step 1 - Understand the Settings 

Now that we've locally set up Dapr, clone the repo, then navigate to the javascript-azurefunction sample: 

```bash
git clone https://github.com/dapr/azure-functions-extension.git
cd samples/javascript-v4-azurefunction
```
In this folder, you will find `local.settings.json`, which lists a few app settings used in the trigger/binding attributes.

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

Alternatively, you can install the Dapr extension by running the following command. Retrieve the latest `version` from [here](https://www.nuget.org/packages/Microsoft.Azure.WebJobs.Extensions.Dapr).

```
func extensions install -p Microsoft.Azure.WebJobs.Extensions.Dapr -v <version>
```

Run function host with Dapr: 

Windows
```
dapr run --app-id functionapp --app-port 3001 --dapr-http-port 3501  --resources-path ..\components\ -- func host start --no-build
```

Linux/MacOS
```
dapr run --app-id functionapp --app-port 3001 --dapr-http-port 3501  --resources-path ../components/ -- func host start --no-build
```

Linux/MacOS
```
dapr run --app-id functionapp --app-port 3001 --dapr-http-port 3501  --resources-path ../components/ -- func host start --no-build
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

```javascript
const { app, output, trigger } = require('@azure/functions');

const daprStateOuput = output.generic({
    type: "daprState",
    stateStore: "%StateStoreName%",
    direction: "out",
    name: "order",
    key: "order"
});

app.generic('CreateNewOrder', {
    trigger: trigger.generic({
        type: 'daprServiceInvocationTrigger',
        name: "payload"
    }),
    return: daprStateOuput,
    handler: async (request, context) => {
        context.log("Node function processed a CreateNewOrder request from the Dapr Runtime.");
        context.log(context.triggerMetadata.payload.data)

       return context.triggerMetadata.payload.data;
    }
});
```

Here `DaprServiceInvocationTrigger` is used to receive and handle `CreateNewOrder` request which first logs that this function is successfully triggered. Then it binds the content to the `order` object. The `DaprState` *output binding* will persist the order into the state store by serializing `order` object into a state arrary format and posting it to `http://localhost:${daprPort}/v1.0/state/${stateStoreName}`.

Now you can invoke this function by using the Dapr cli in a new command line terminal.  

Windows Command Prompt
```sh
dapr invoke --app-id functionapp --method CreateNewOrder --data "{\"data\": { \"orderId\": \"41\" } }"
```

Windows PowerShell
```powershell
dapr invoke --app-id functionapp --method CreateNewOrder --data '{\"data\": { \"orderId\": \"41\" } }'
```

Linux or MacOS
```sh
dapr invoke --app-id functionapp --method CreateNewOrder --data '{"data": { "orderId": "41" } }'
```

You can also do this using the Visual Studio Code [Rest Client Plugin](https://marketplace.visualstudio.com/items?itemName=humao.rest-client)

```http
POST  http://localhost:3501/v1.0/invoke/functionapp/method/CreateNewOrder

{
    "data": {
        "orderId": "42"
    } 
}
```

**Note**: in this sample, `daprServiceInvocationTrigger` binding in the function.json does not specify the method name, so it defaults to use the FunctionName. Alternatively, you can use `methodName` field to specify the service invocation method name that your function should respond. In this case, then you need to use the following command:

```powershell
dapr invoke --app-id functionapp --method newOrder --data "{\"data\": { \"orderId\": \"41\" } }"
```

In your terminal window, you should see logs indicating that the message was received and state was updated:

```
== APP == [TIMESTAMP] Executing 'CreateNewOrder' (Reason='', Id=<ExecutionId>)
== APP == [TIMESTAMP] Node function processed a CreateNewOrder request from the Dapr Runtime.
== APP == [TIMESTAMP] Executed 'CreateNewOrder' (Succeeded, Id=<ExecutionId>)
```
----------------
In order to confirm the state is now persisted, you can move to the next function:

```javascript
const { app, input, trigger } = require('@azure/functions');

const daprStateInput = input.generic({
    type: "daprState",
    stateStore: "%StateStoreName%",
    direction: "in",
    name: "order",
    key: "order"
});

app.generic('RetrieveOrder', {
    trigger: trigger.generic({
        type: 'daprServiceInvocationTrigger',
        name: "payload"
    }),
    extraInputs: [daprStateInput],
    handler: async (request, context) => {
        context.log("Node function processed a RetrieveOrder request from the Dapr Runtime.");
        const daprStateInputValue = context.extraInputs.get(daprStateInput);
        // print the fetched state value
        context.log(daprStateInputValue);
    }
});
```

Similarly, the function will be triggered by any `RetrieveOrder` service invocation request. However, here `DaprState` *input binding* is used to fetch the latest value of the key `order` and bind the value to string object `data`' before executing the function block.

In your terminal window, you should see logs to confirm the expected result:

```
== APP == [TIMESTAMP]  Executing 'RetrieveOrder' (Reason='', Id=<ExecutionId>)
== APP == [TIMESTAMP]  Node function processed a RetrieveOrder request from the Dapr Runtime.
== APP == [TIMESTAMP]  {"orderId":"41"}
== APP == [TIMESTAMP]  Executed 'RetrieveOrder' (Succeeded, Id=<ExecutionId>)
```


## 2. Pub/Sub: TransferEventBetweenTopics and PrintTopicMessage

```javascript
const { app, output, trigger } = require('@azure/functions');

const daprPublishOutput = output.generic({
    type: "daprPublish",
    direction: "out",
    pubsubname: "%PubSubName%",
    topic: "B",
    name: "payload"
});

app.generic('TransferEventBetweenTopics', {
    trigger: trigger.generic({
        type: 'daprTopicTrigger',
        name: "subEvent",
        pubsubname: "%PubSubName%",
        topic: "A"
    }),
    return: daprPublishOutput,
    handler: async (request, context) => {
        context.log("Node function processed a TransferEventBetweenTopics request from the Dapr Runtime.");
        context.log(context.triggerMetadata.subEvent.data);

        return { payload: context.triggerMetadata.subEvent.data };
    }
});
```

Here `DaprTopicTrigger` is used to subscribe to topic `A`, so whenever a message is published on topic `A`, the message will bind to `context.bindings.subEvent`. Please see the [`CloudEvent`](https://github.com/cloudevents/spec/blob/master/spec.md) for details.


> **Note**: Alternatively, any other JSON-serializable datatype binds directly to the data field of the cloud event. For example, int, double, and custom “POCO” types can be used as the trigger type and will be deserialized from the event’s data field. 

Then, `DaprPublish` *output binding* is used to publish a new event to topic `B`.


Also, the function below subscribes to topic `B` which simply prints the message content when an event arrives.

```javascript
module.exports = async function (context) {
    context.log("Node function processed a PrintTopicMessage request from the Dapr Runtime.");
    context.log(`Topic B received a message: ${JSON.stringify(context.bindings.subEvent.data)}.`);
};
```

You can publish a message to topic A using the Dapr cli:

```powershell
dapr publish --pubsub messagebus --publish-app-id functionapp --topic A --data 'This is a test'
```

The Dapr logs should show the following:
```
== APP == [TIMESTAMP] Executing 'TransferEventBetweenTopics' (Reason='',Id={ExectuionId})
== APP == [TIMESTAMP] Node function processed a TransferEventBetweenTopics request from the Dapr Runtime.
== APP == [TIMESTAMP] Executed 'TransferEventBetweenTopics' (Succeeded, Id={ExectuionId})
== APP == [TIMESTAMP] Executing 'PrintTopicMessage' (Reason='', Id={AnotherExectuionId})
== APP == [TIMESTAMP] Node function processed a PrintTopicMessage request from the Dapr Runtime.
== APP == [TIMESTAMP] Topic B received a message: Transfer from Topic A: This is a test.
== APP == [TIMESTAMP] Executed 'PrintTopicMessage' (Succeeded, Id={AnotherExectuionId})
```

## 3. Dapr Binding
This section describes how this extension integrates with the Dapr Binding component. Here Kafka binding is used as an example. Please refer [this doc to spin up Kafka locally](../../samples/dapr-kafka/README.md). In the example below, `DaprBindingTrigger` is used to have the azure function triggerred when a new message arrives at Kafka.

```javascript
const { app, trigger } = require('@azure/functions');

app.generic('ConsumeMessageFromKafka', {
    trigger: trigger.generic({
        type: 'daprBindingTrigger',
        bindingName: "%KafkaBindingName%",
        name: "triggerData"
    }),
    handler: async (request, context) => {
        context.log("Node function processed a ConsumeMessageFromKafka request from the Dapr Runtime.");
        context.log(context.triggerMetadata.triggerData)
    }
});
```
Now let's look at how our function uses `DaprBinding` to push messages into our Kafka instance in `SendMessageToKafka` function:

```javascript
const { app, output, trigger } = require('@azure/functions');

const daprBindingOuput = output.generic({
    type: "daprBinding",
    direction: "out",
    bindingName: "%KafkaBindingName%",
    operation: "create",
    name: "messages"
});

app.generic('SendMessageToKafka', {
    trigger: trigger.generic({
        type: 'daprServiceInvocationTrigger',
        name: "payload"
    }),
    return: daprBindingOuput,
    handler: async (request, context) => {
        context.log("Node function processed a SendMessageToKafka request from the Dapr Runtime.");
        context.log(context.triggerMetadata.payload)

        return { "data": context.triggerMetadata.payload };
    }
});
```
`DaprBinding` *output binding* sends the payload to the `sample-topic` Kafka Dapr binding.

Now you can use service invocation to invoke this function:

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
== APP == [TIMESTAMP] Node HTTP trigger function processed a request.
== APP == [TIMESTAMP] Executed 'SendMessageToKafka' (Succeeded, Id=<ExecutionId>)
```

Since both functions have been deployed in the same app, you should see the logs below which indicate that the message has been consumed:
```
== APP == [TIMESTAMP] Executing 'ConsumeMessageFromKafka' (Reason='', Id=<ExecutionId>)
== APP == [TIMESTAMP] Hello from Kafka!
== APP == [TIMESTAMP] Trigger data: { message: 'hello!' }
== APP == [TIMESTAMP] Executed 'ConsumeMessageFromKafka' (Succeeded, Id=<ExecutionId>)
```

## 4. Dapr Secret: 
This section demonstrates how `DaprSecret` **input binding** integrates with Dapr Secret component. Here, Local file Secret Store is used and you can follow the setup instructions at [Local file secret store](https://docs.dapr.io/operations/components/setup-secret-store/supported-secret-stores/file-secret-store/) to configure a secret named "my-secret".

Please refer to [Dapr Secret Store doc](https://docs.dapr.io/operations/components/setup-secret-store/) to set up other supported secret stores.

```js
const { app, input, trigger } = require('@azure/functions');

const daprSecretInput = input.generic({
    type: "daprSecret",
    secretStoreName: "localsecretstore",
    metadata: "metadata.namespace=default",
    direction: "in",
    name: "secret",
    key: "my-secret"
});

app.generic('RetrieveSecret', {
    trigger: trigger.generic({
        type: 'daprServiceInvocationTrigger',
        name: "payload"
    }),
    extraInputs: [daprSecretInput],
    handler: async (request, context) => {
        context.log("Node function processed a RetrieveSecret request from the Dapr Runtime.");
        const daprSecretInputValue = context.extraInputs.get(daprSecretInput);

        // print the fetched secret value
        for (var key in daprSecretInputValue) {
            context.log(`Stored secret: Key=${key}, Value=${daprSecretInputValue[key]}`);
        }
    }
});
```

`DaprSecret` *input binding* retreives the secret named by `my-secret` and binds to `secret` as a dictionary object. Since Local Secret Store supports multiple keys in a secret, the secret dictionary could include multiple key value pairs and you can access the specfic one. For other secret store only supports one keys, the dictionary will only contain one key value pair where key matches the secret name, namely `my-secret` in this example, and the actual secret value is in the property value. This sample just simply prints out all secrets, but please do not log any real secret in your production code.

You can retrieve the secret by invoking the RetrieveSecretLocal function using the command:-
```
dapr invoke --app-id functionapp --method RetrieveSecret my-secret
```

Some secret stores need a metadata string to be provided. In order to specify multiple metadata fields, join them by `&`, see the below [Hashicorp Vault](https://docs.dapr.io/operations/components/setup-secret-store/supported-secret-stores/hashicorp-vault/) example.
```json
"metadata": "metadata.version_id=15&metadata.version_stage=AAA"
```

## 5. Dapr Invoke output binding
Dapr invoke output binding can be used to invoke other Azure functions or services where it will act as a proxy. For example, In the below Azure function, which gets triggered on HttpTrigger, can invoke another Azure functions like RetrieveOrder.

```JavaScript
const { app, output, trigger } = require('@azure/functions');

const daprInvokeOutput = output.generic({
    type: "daprInvoke",
    direction: "out",
    appId: "{appId}",
    methodName: "{methodName}",
    httpVerb: "post",
    name: "invokePayload"
});

app.generic('InvokeOutputBinding', {
    trigger: trigger.generic({
        type: 'httpTrigger',
        authLevel: 'anonymous',
        methods: ['POST'],
        route: "invoke/{appId}/{methodName}",
        name: "req"
    }),
    return: daprInvokeOutput,
    handler: async (request, context) => {
        context.log("Node HTTP trigger function processed a request.");

        const payload = await request.text();
        context.log(JSON.stringify(payload));
        
        return { body: payload };
    }
});
```

Invoke the above function (InvokeOutputBinding) with a HTTP GET request.

  ```
  http://localhost:7071/api/invoke/functionapp/RetrieveOrder
  ```

Once InvokeOutputBinding is called, it will invoke the RetrieveOrder azure function and the output will look like as shown below.

```
== APP == [TIMESTAMP] Executing 'InvokeOutputBinding' (Reason='This function was programmatically called via the host APIs.', Id=<ExecutionId>)
== APP == [TIMESTAMP] C# HTTP trigger function processed a request.
== APP == [TIMESTAMP] Executing 'RetrieveOrder' (Reason='(null)', Id=<ExecutionId>)
== APP == [TIMESTAMP] C# function processed a RetrieveOrder request from the Dapr Runtime.
== APP == [TIMESTAMP] {"orderId":"41"}
== APP == [TIMESTAMP] Executed 'RetrieveOrder' (Succeeded, Id=<ExecutionId>)
== APP == [TIMESTAMP] Executed 'InvokeOutputBinding' (Succeeded, Id=<ExecutionId>)
```

# Step 6 - Cleanup

To stop your services from running, simply stop the "dapr run" process. Alternatively, you can spin down each of your services with the Dapr CLI "stop" command. For example, to spin down both services, run these commands in a new command line terminal:

```bash
dapr stop --app-id functionapp
```
