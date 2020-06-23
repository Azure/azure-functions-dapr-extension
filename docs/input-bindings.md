# Azure Functions Dapr input bindings

Input bindings allow you to pull data in at the beginning of an execution.  The parameters for the input binding can either be set statically in the `function.json` or attribute definition, use the `%syntax%` to reference a value from environment variables, or pull from surfaced trigger metadata (e.g. the route parameter of an HTTP triggered function).  More details on bindings can be found in the [Azure Functions documentation](https://docs.microsoft.com/en-us/azure/azure-functions/functions-bindings-expressions-patterns).  You can always leverage the Dapr SDK directly within your function instead of using an input binding.

## State Input Binding
Retrieve the current state for a specified key at the beginning of an execution.

### Function.json sample
```json
{
    "type": "daprState",
    "direction": "in",
    "dataType": "string",
    "name": "state",
    "stateStore": "statestore",
    "key": "{key}"
}
```

### C# Attribute sample
```csharp
[HttpTrigger(AuthorizationLevel.Function, "get", Route = "state/{key}")] HttpRequest req,
[DaprState("statestore", Key = "{key}")] string state,
```

### Properties

|Property Name|Description|
|--|--|
|StateStore|The name of the state store to retrieve state.|
|Key|The name of the key to retrieve from the specified state store.|


## Secret Input Binding
Retrieve the value of a dapr secret at the beginning of an execution.

### Function.json sample
> NOTE: `%secret-key%` would resolve from the environment variable of `secret-key`
```json
{
    "type": "daprSecret",
    "name": "secret",
    "secretStoreName": "secretStore",
    "key": "%secret-key%",
    "direction": "in"
}
```

### C# Attribute sample
```csharp
// %secret-key% would resolve from the environment variable of `secret-key`
[DaprSecret("secretStore", "%secret-key%")] JObject secret,
```

### Properties

|Property Name|Description|
|--|--|
|SecretStoreName|The name of the secret store to get the secret.|
|Key|The key identifying the name of the secret to get.|
|Metadata|Optional. An array of metadata properties in the form "key1=value1&amp;key2=value2".|