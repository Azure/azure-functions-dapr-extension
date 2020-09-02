# Azure Functions Dapr triggers

Azure Functions can be triggered using the following Dapr events.

There are no templates for triggers in Dapr in the functions tooling today.  Start your project with another trigger type (e.g. Storage Queues) and then modify the `function.json` or attributes.

## Input Binding Trigger
Trigger on a Dapr input binding.

### Function.json sample
```json
{
    "type": "daprBindingTrigger",
    "bindingName": "myKafkaBinding",
    "name": "triggerData",
    "direction": "in"
}
```

### C# Attribute sample
```csharp
[DaprBindingTrigger(BindingName = "myKafkaBinding")] JObject triggerData,
```

### Properties

|Property Name|Description|
|--|--|
|BindingName|The name of the Dapr trigger. If not specified, the name of the function is used as the trigger name.|


## Service Invocation Trigger
Trigger on a Dapr service invocation.

### Function.json sample
```json
{
    "type": "daprServiceInvocationTrigger",
    "name": "triggerData",
    "direction": "in"
}
```

### C# Attribute sample
```csharp
[DaprServiceInvocationTrigger] JObject triggerData,
```

### Properties

|Property Name|Description|
|--|--|
|MethodName|Optional. The name of the method the Dapr caller should use. If not specified, the name of the function is used as the method name.|

## Topic Trigger
Trigger on a Dapr topic subscription.  The trigger will automatically expose and endpoint and communicate with the Dapr sidecar which topics it is interested in receiving data for based on trigger configuration.

### Function.json sample
```json
{
    "type": "daprTopicTrigger",
    "pubsubname": "pubsub",
    "topic": "myTopic",
    "name": "triggerData",
    "direction": "in"
}
```

### C# Attribute sample
```csharp
[DaprTopicTrigger("pubsub", Topic = "myTopic")] CloudEvent triggerData,
```

### Properties

|Property Name|Description|
|--|--|
|PubSubName|The name of the Dapr pub/sub.|
|Topic|The name of the Dapr topic.|