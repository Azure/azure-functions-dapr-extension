# Azure Functions dapr extensions

⚠️ This is a proof of concept extensions.  It is not intended for production ⚠️

The Azure Functions dapr extension allows you to easily interact with the dapr APIs from an Azure Function.  This would work in local and Kubernetes scenarios.

## State management

The state management input and output bindings allow you to write state from your Azure Function.

### JavaScript Example
```javascript
module.exports = async function (context, req) {
    context.log('JavaScript HTTP trigger function processed a request.');

    // input binding
    var data = context.bindings.daprInput;

    // output binding
    context.bindings.daprOutput = {
        // stateStore: 'statestore-if-not-in-function.json'
        // key: 'key-if-not-in-function.json'
        value: data
    };

    context.res = {
        status: 200
    };
};
```

### C# Example

```csharp
[FunctionName("MyFunction")]
public static async Task<IActionResult> Run(
    [HttpTrigger(AuthorizationLevel.Function, "get", Route = "state/{key}")] HttpRequest req,
    // Input binding
    [DaprState(StateStore = "statestore", Key = "{key}")] string inputState,
    // Output binding - to override statestore or key bind to SaveStateOptions
    [DaprState(StateStore = "statestore", Key = "{key}-output")] IAsyncCollector<string> outputState,
    ILogger log)
{
    // ...
    await outputState.AddAsync(inputState);
}
```

Can bind to `string`, `Stream`, `byte[]`, or `SaveStateOptions`.

### Properties

|Property Name|Description|
|---|---|
|DaprAddress|The address to reach dapr. Default is `http://localhost:3500`|
|StateStore|Name of the state store|
|Key|Key used to store the data|

## Invoke method

The invoke method output binding allows your function to invoke a downstream dapr app.

### JavaScript Example
```javascript
module.exports = async function (context, req) {
    context.log('JavaScript HTTP trigger function processed a request.');

    // output binding
    context.bindings.daprInvoke = {
        // appId: 'appId-if-not-in-function.json'
        // methodName: 'methodName-if-not-in-function.json'
        // httpVerb: `httpVerb-if-not-in-function.json'
        body: data
    };

    context.res = {
        status: 200
    };
};
```

### C# Example

```csharp
[FunctionName("MyFunction")]
public static async Task<IActionResult> Run(
    [HttpTrigger(AuthorizationLevel.Function, "get", Route = "invoke/{methodName}")] HttpRequest req,
    [DaprInvoke(AppId = "other-function", MethodName = "{methodName}", HttpVerb = "post")] IAsyncCollector<InvokeMethodOptions> output,
    ILogger log)
{
    // ..

    var outputContent = new InvokeMethodOptions(){
        Body = "SomeData"
    };

    await output.AddAsync(outputContent);
}
```

### Properties

|Property Name|Description|
|---|---|
|DaprAddress|The address to reach dapr. Default is `http://localhost:3500`|
|AppId|ID of the app to invoke|
|MethodName|Name of the method to invoke|
|HttpVerb|Http verb to use during invoke (e.g. `get`, `put`, `post`)|
|Body|Optional. Any content to send with invoke|