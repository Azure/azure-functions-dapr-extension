# Quickstart

The following will get you started with a Function app that leverages the Dapr extension.  This sample uses JavaScript, but you can leverage the docs to use whatever language you prefer.  We will create a function app that has two functions.  One that triggers on HTTP, reads and updates state, and then publishes to a Dapr topic.  The second function will trigger on the topic.

## Prerequisites

* [Dapr configured locally (and optionally in Kubernetes)](https://github.com/dapr/docs/blob/master/getting-started/environment-setup.md)
* [Azure Functions Core Tools - v3](https://github.com/azure/azure-functions-core-tools#installing)
* [Docker](https://docs.docker.com/get-docker/)
* [.NET Core SDK](https://dotnet.microsoft.com/download) - enables building of the extension locally in the project
* Optional but recommended: [KEDA in Kubernetes](https://keda.sh/docs/1.4/deploy/) (for scaling of the function)
* Optional but recommended: [Visual Studio Code with the Azure Functions extension](https://marketplace.visualstudio.com/items?itemName=ms-azuretools.vscode-azurefunctions)

## Creating the function app

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
1. Run `func extensions install -p Dapr.AzureFunctions.Extension -v 0.8.0-preview01`

## Using Dapr triggers and bindings

## Debugging locally

## Creating an Azure Function Docker image

## Deploying to Kubernetes