# Quickstart

The following will get you started with a Function app that leverages the Dapr extension.  This sample uses JavaScript, but you can leverage the docs to use whatever language you prefer.  We will create a function app that has two functions.  One that triggers on HTTP, reads and updates state, and then publishes to a Dapr topic.  The second function will trigger on the topic.

## Prerequisites

* [Dapr configured locally (and optionally in Kubernetes)](https://docs.dapr.io/getting-started/install-dapr/)
* [Azure Functions Core Tools - v3](https://github.com/azure/azure-functions-core-tools#installing)
* [Docker](https://docs.docker.com/get-docker/)
* [.NET Core SDK](https://dotnet.microsoft.com/download) - enables building of the extension locally in the project
* [Node 12](https://nodejs.org/) for local debugging of the JavaScript app

## Creating the function app

> For reference or to skip ahead, you can clone and navigate to a working version of [this quickstart app here](../samples/quickstart).

1. Create a new directory and navigate to it in a terminal
1. Run `func init --docker`
1. Choose **Node** and **JavaScript**
    This will create a function project in JavaScript.  You can see a few files here, including a `Dockerfile` which can be used to containerize this function app and run anywhere.
1. Create a new function in the app with `func new`
1. Choose **HTTP trigger**, and leave the name as the default `HttpTrigger`
    There should now be a new folder in the project called `HttpTrigger` which has a `function.json` file containing the trigger metadata, and the `index.js` file for the function code.

## Installing the Dapr extension

While this extension is in preview it is not included in the default extension bundle for functions.  We can still include it, but will need to manually install it into the project, and opt-out to using the default extensions.  

1. Open the `host.json` file from the root of the project and remove the `extensionBundle` property and values (if they exist).  Save the file.
1. Run `func extensions install -p Microsoft.Azure.WebJobs.Extensions.Dapr -v 1.0.0`. Be sure to use the latest version as published on [NuGet](https://www.nuget.org/packages/Microsoft.Azure.WebJobs.Extensions.Dapr).

You can validate the extension installed successfully by running the function.  Run `func start` and validate the app loads and the startup contains the logs

> Loading startup extension 'Dapr'
> Loaded extension 'Dapr' (1.0.0.0)

Stop the function runtime after validating.

## Adding Dapr input and output bindings

Now that we have our HTTP function, we want to add bindings to Dapr to read and update some state.

1. Open the `HttpTrigger/function.json` file.
    This file contains the metadata for all triggers and bindings the function uses.  For our HTTP function it has an HTTP trigger, and an output binding which is the HTTP response.  Let's first change the route of the HTTP trigger to accept a route parameter in the trigger path.
1. Modify the `httpTrigger` object to include a defined route and anonymous auth, so it looks like the below:
    ```json
    {
      "authLevel": "anonymous",
      "type": "httpTrigger",
      "direction": "in",
      "name": "req",
      "route": "state/{key}",
      "methods": [
        "post"
      ]
    }
    ```
    This means it will only accept a `POST` method, and will be called at `/api/state/{key}` - where key is a route parameter.
1. Add an **input binding** for Dapr state, to pull in the state at `{key}` when the function executions.  Add a binding object like the below to the `function.json`.
    ```json
    {
      "type": "daprState",
      "direction": "in",
      "dataType": "string",
      "name": "stateIn",
      "stateStore": "statestore",
      "key": "{key}"
    }
    ```
    This means the binding context with name `state` will have the value pulled in at the beginning of the execution.
1. Add an **output binding** for Dapr state to set and update the state after the function execution.  Add a binding object like the below to the `function.json`.
    ```json
    {
      "type": "daprState",
      "direction": "out",
      "dataType": "string",
      "name": "stateOut",
      "stateStore": "statestore",
      "key": "{key}"
    }
    ```
1. Finally, add an **output binding** for Dapr topic publish.  Add a binding object like the below to the `function.json`.
    ```json
    {
      "type": "daprPublish",
      "direction": "out",
      "name": "publish"
    }
    ```
    This means whatever value we set for the payload of the `publish` output binding during execution will be set.  We could define the topic here in `function.json`, but we will define the topic name in the execution for this sample.
1. Confirm your `function.json` matches the completed [sample here](../samples/quickstart/HttpTrigger/function.json).

## Authoring the first function

We'll now write a function that appends the state with whatever value is passed into the HTTP request body of the function.

1. Open the `index.js` file for the `HttpTrigger` function we have just completed configuring.
1. Replace the code within the function to the following:

```javascript
module.exports = async function (context, req) {
    context.log('JavaScript HTTP trigger function processed a request.');

    let state = context.bindings.stateIn || "";
    context.log(`Current state:\n${state}\n`);

    // update state
    state = `${state}\n${req.body}`;

    context.log(`Updated state:\n${state}\n`);

    // save state using the Dapr output binding
    context.bindings.stateOut = 
    {
        "value": state,
        // "key": "{Optional. We defined in function.json}",
        // "etag": "{Optional. The etag value of the state record.}"
    };

    // publish a message using the Dapr topic publish output binding
    context.bindings.publish = 
    {
        "payload": state,
        "pubsubname": "pubsub",
        "topic": "myTopic"
    }

    // return an http response using the http output binding
    context.res = {
        // status: 200, /* Defaults to 200 */
        body: `State now updated to: \n${state}`
    };
};
```

## Debug the function locally

Now that we have our first function, we can test it out locally. 

### Running from single line command
You can use the Dapr CLI to start the function app.  This will expose a port for the language to attach any debugger to (by default in node it's port 9229).

`dapr run --app-id function-app --dapr-http-port 3501 -- func start -p 7071`

### Running using VS Code debugger / breakpoints
If you prefer, you can use VS Code debugging to use the [Azure Functions VS Code](https://marketplace.visualstudio.com/items?itemName=ms-azuretools.vscode-azurefunctions) extension in conjunction with the Dapr CLI to attach a debugger and set breakpoints easily.

1. Install the [Azure Functions VS Code](https://marketplace.visualstudio.com/items?itemName=ms-azuretools.vscode-azurefunctions) extension.
1. Open the project in VS Code.
1. Open the command pallette and choose the command **Azure Functions: Initialize Project for Use with VS Code...**
    This will generate a `launch.json` file to easily run and debug your code.
1. Specify the port you want Dapr to listen on in the `local.settings.json` file by adding a value for `DAPR_HTTP_PORT` of `3501` as shown in [the quickstart sample](../samples/quickstart/local.settings.json).  If you wanted to debug multiple apps at the same time you would need to assign unique ports for each.
1. Start the debugger
    You will see a window appear with your app running
1. Open a seperate terminal and start the Dapr sidecar at the specified port.
    `dapr run --app-id function-app --app-port 3001 --dapr-http-port 3501`

Dapr should connect to the Function App and display that you are up and running.  Keep Dapr and the Function App running as you debug.

### Testing the function
1. Make an HTTP POST to the local function.  You can use a tool like cUrl, [Postman](https://www.postman.com/), or [REST Client](https://marketplace.visualstudio.com/items?itemName=humao.rest-client).
    ```bash
    POST http://localhost:7071/api/state/mykey
    Content-Type: application/json

    "Hello"
    ```

    You should see the function logs execute in the terminal, and return a response that the state is now `Hello`.  

1. Run the command again and see how the state is persisted and appended.
1. Optional: You can attach a node debugger and breakpoint / step through the function code.  

## Create the subscribing function
We could easily create a second function app and include a function in it that will subscribe to our function, but for simplicity we are going to create the function in the same app.

1. At the base of the project, run `func new` to add a new function.
    There is no template in the CLI for Dapr triggers today, so we'll need to start with a different template and update.
1. Select **Timer Trigger** (we'll change it to a Dapr Topic Trigger later)
1. Give the name `DaprSubscribeTrigger`.
1. Open the `DaprSubscribeTrigger/function.json` metadata for this new function.
1. Replace the `timerTrigger` trigger with a Dapr topic trigger:
    ```json
    {
      "type": "daprTopicTrigger",
      "pubsubname": "pubsub",
      "topic": "myTopic",
      "name": "daprTrigger"
    }
    ```
    You can see a completed `function.json` in the [quickstart sample here](../samples/quickstart/DaprSubscribeTrigger/function.json).
1. Replace the `index.js` code for this function with the following:
```javascript
module.exports = async function (context) {
    context.log("Node function processed a Topic subscribe request from the Dapr Runtime.");
    context.log(`Topic B received a message: ${context.bindings.daprTrigger.data}.`);
};
```
1. May core tools version may block debugging if no storage account is defined as some Azure Functions triggers depend on them.  Dapr triggers do not, but to get around this validation edit the `local.settings.json` file and add `"none"` as the value for `AzureWebJobsStorage`.  A fix for this workaround is [being tracked here](https://github.com/Azure/azure-functions-core-tools/issues/2065).
1. Save and run the app.  Note that because we now need to recieve trigger events for the dapr sidecar, we need to define the port the function will listen on for these triggers.  
    `dapr run --app-id function-app --app-port 3001 --dapr-http-port 3501 -- func start -p 7071` or starting the function debugger and then starting dapr with `dapr run --app-id function-app --app-port 3001 --dapr-http-port 3501`.

When the app runs you should see the topic trigger fires.  You can continue to trigger the first app using HTTP POSTs and update the state. You should see logs for both the HttpTrigger function and the DaprSubscribeTrigger function now, all powered by Dapr.

## Deploying to Kubernetes (Optional)

We can now deploy this app to run in any Kubernetes cluster.

### Pre-requisites

* Dapr deployed to Kubernetes already
* Dapr components defined in the cluster to power a state store with name `statestore`
* Dapr components defined in the cluster to power a `pubsub` message bus.

### Creating the function app container

Our project template already has a `Dockerfile` included.  If yours does not, you can generate one by running `func init --docker-only`.

1. Build a docker container for your app: `docker build -t {registry}/{container-name}`.  For example: `docker build -t jeffhollan/dapr-function`.
1. Push the docker container to your registry: `docker push {registry}/{container-name}`.

### Deploy the container to Kubernetes

1. Copy the contents of [this deployment file](../samples/quickstart/deploy.yaml) to your machine in a new file named `deploy.yaml`.
1. Replace the name of the `image` on line 39 with your container registry and image name.
1. Deploy the function app to your Kubernetes cluster: `kubectl apply -f deploy.yaml`.
    This will create a Kubernetes service named `azure-function` in your cluster exposing port 80 to trigger the function.  The dapr sidecar will also be configured to run alongside the service and communicate with the Azure Function via port 3001.
1. Get the IP address of the new service (this may take a few minutes to generate): `kubectl get service` and copy the EXTERNAL IP created for the app.
1. Test out triggering the function with an HTTP POST to `http://{EXTERNAL IP}/api/state/hello`

You should get a response that validates the state is being called.  You can also use `kubectl logs` to check the logs for the app and see that both functions are working.

Awesome job! Let us know `@daprdev` and `@AzureFunctions` on Twitter if you got this working 😁.
