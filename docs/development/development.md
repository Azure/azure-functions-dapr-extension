# Setup Local Development

This guide will help you set up the Azure Functions Dapr Extension in your local development environment. Follow these steps to get started.

## Prerequisites

Before you begin, ensure you have the following prerequisites installed:

- Setup Dapr: Follow instructions to [download and install the Dapr CLI](https://docs.dapr.io/getting-started/install-dapr-cli/) and [initialize Dapr](https://docs.dapr.io/getting-started/install-dapr-selfhost/)
- [Azure Functions Core Tools](https://docs.microsoft.com/en-us/azure/azure-functions/functions-run-local)
- [.NET SDK](https://dotnet.microsoft.com/download)
- [Docker](https://www.docker.com/get-started)
- [Setup Dapr components](https://github.com/Azure/azure-functions-dapr-extension/tree/master/samples/components) if you need one

## Clone the Repository

To start, clone the Azure Functions Dapr Extension repository to your local machine. Use the following command in your terminal or Git client:

```bash
git clone https://github.com/Azure/azure-functions-dapr-extension.git
```

## Build the project
Once you have cloned the repository, navigate to the project directory and build the project. Use the following commands:
```
cd azure-functions-dapr-extension
dotnet build
```

## Run tests
To ensure everything is functioning correctly, it's a good practice to run the tests. Use the following command to run the tests:
```
dotnet test
```
## Debugging
To debug the Azure Functions Dapr Extension locally, you can use your preferred development environment, such as Visual Studio Code or Visual Studio. Set breakpoints in your code where needed, and use the appropriate debugging tools for your chosen development environment.

### Debugging in Visual Studio Code
1. Create/reuse existing samples to debug the extension.
2. Run the function app with below command.
    
    > [!WARNING]
    > Provide ` --app-port 3001` only when you have Dapr trigger(s) in your Azure function

    ```
    dapr run --app-id functionapp --app-port 3001 --dapr-http-port 3501 -- func host start
    ```
3. Open Visual Studio Code, navigate to the project folder, and open the relevant code file.
4. Add breakpoints in your code where needed.
5. In Visual Studio Code, open the "Run and Debug" sidebar, select the configuration for your function, and click the "Run" button.
6. Visual Studio Code will automatically attach the debugger to the running process, and you can enjoy debugging.

### Debugging in Visual Studio
1. Create/reuse existing samples to debug the extension.
2. Run the function app with below command.

    > [!WARNING]
    > Provide ` --app-port 3001` only when you have Dapr trigger(s) in your Azure function
    ```
    dapr run --app-id functionapp --app-port 3001 --dapr-http-port 3501 -- func host start
    ```
3. Open Visual Studio, navigate to the project folder, and open the relevant code file.
4. Add breakpoints in your code where needed.
5. In Visual Studio, select the "Debug" menu, then choose "Attach to Process..."
6. Choose the func process or provide the corresponding process ID.
7. Click the "Attach" button to attach the debugger.


That's it! You're now set up for local development with the Azure Functions Dapr Extension. You can start building and testing your Azure Functions with Dapr support locally.