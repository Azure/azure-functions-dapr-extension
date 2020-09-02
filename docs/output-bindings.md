# Azure Functions Dapr output bindings

Output bindings allow you to push data to a destination at the end of an execution.  The parameters for the output binding can either be set statically in the `function.json` or attribute definition, use the `%env_variable_name%` syntax to reference a value from environment variables, or pull from surfaced trigger metadata (e.g. the route parameter of an HTTP triggered function).  More details on bindings can be found in the [Azure Functions documentation](https://docs.microsoft.com/en-us/azure/azure-functions/functions-bindings-expressions-patterns).  You can always leverage the Dapr SDK directly within your function instead of using an output binding.

## State Output Binding
Save state to a specified key.

If binding to a `byte[]` or JSON object, the object should be of format:

```json
{
    "value": "The body of the invoke-method operation.",
    "key": "{Optional. If not defined in function.json}",
    "etag": "{Optional. The etag value of the state record.}"
}
```

### Function.json sample
```json
{
    "type": "daprState",
    "direction": "out",
    "name": "state",
    "stateStore": "statestore",
    "key": "{key}"
}
```

### C# Attribute sample
```csharp
[HttpTrigger(AuthorizationLevel.Function, "get", Route = "state/{key}")] HttpRequest req,
[DaprState("statestore", Key = "{key}")] IAsyncCollector<DaprStateRecord> state,
```

### Properties

|Property Name|Description|
|--|--|
|StateStore|The name of the state store to save state.|
|Key|The name of the key to save state within the state store.|


## Service Invocation Output Binding
Invoke another Dapr service.

If binding to a `byte[]` or JSON object, the object should be of format:

```json
{
    "body": "The body of the invoke-method operation.",
    "appId": "{Optional. If not defined in function.json}",
    "methodName": "{Optional. If not defined in function.json}",
    "httpVerb": "{Optional. If not defined in function.json}"
}
```

### Function.json sample
```json
{
    "type": "daprInvoke",
    "direction": "out",
    "appId": "{appId}",
    "methodName": "{methodName}",
    "httpVerb": "post",
    "name": "output"
}
```

### C# Attribute sample
```csharp
[HttpTrigger(AuthorizationLevel.Function, "get", Route = "invoke/{appId}/{methodName}")] HttpRequest req,
[DaprInvoke(AppId = "{appId}", MethodName = "{methodName}", HttpVerb = "post")] IAsyncCollector<InvokeMethodParameters> output,
```

### Properties

|Property Name|Description|
|--|--|
|AppId|The Dapr app ID to invoke.|
|MethodName|The method name of the app to invoke.|
|HttpVerb|Optional. HTTP verb to use of the app to invoke. Default is `POST`.|

## Topic Publish Output Binding
Publish a message to a Dapr topic.

If binding to a `byte[]` or JSON object, the object should be of format:

```json
{
    "payload": "The subscribers will receive this payload as the body of a Cloud Event envelope.",
    "pubsubname": "{Optional. Name of the pub/sub if not defined in function.json}",
    "topic": "{Optional. Name of the topic if not defined in function.json}"
}
```

### Function.json sample
```json
{
    "type": "daprPublish",
    "direction": "out",
    "name": "pubEvent",
    "pubsubname": "pubsub",
    "topic": "myTopic"
}
```

### C# Attribute sample
```csharp
[DaprPublish(PubSubName = "pubsub", Topic = "myTopic")] IAsyncCollector<DaprPubSubEvent> pubEvent,
```

### Properties

|Property Name|Description|
|--|--|
|PubSubName|The name of the Dapr pub/sub to send the message.|
|Topic|The name of the Dapr topic to send the message.|

## Dapr Binding Output Binding
Send data to a Dapr binding.

If binding to a `byte[]` or JSON object, the object should be of format:

```json
{
    "data": "Data to send to binding",
    "metadata": "{Optional. The bag of key value pairs for binding-specific metadata}",
    "operation": "{Optional. If not defined in function.json}",
    "bindingName": "{Optional. If not defined in function.json}"
}
```

### Function.json sample
```json
{
    "type": "daprBinding",
    "direction": "out",
    "bindingName": "myKafkaBinding",
    "operation": "create",
    "name": "messages"
}
```

### C# Attribute sample
```csharp
[DaprBinding(BindingName = "myKafkaBinding", Operation = "create")] IAsyncCollector<DaprBindingMessage> messages,
```

### Properties

|Property Name|Description|
|--|--|
|BindingName|The name of the Dapr binding.|
|Operation|The configured binding operation.|