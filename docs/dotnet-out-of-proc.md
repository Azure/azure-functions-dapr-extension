# Azure Functions Dapr Extension - Isolated worker support

Azure function Dapr extension supports .Net isolated mode. In isolated mode your functions runs in an isolated worker process in Azure. This allows you to run your .NET class library functions on a version of .NET that is different from the version used by the Functions host process. To learn more about .Net isolated process follow [this link](https://learn.microsoft.com/en-us/azure/azure-functions/dotnet-isolated-process-guide).

To learn the differences between isolated worker model and in-process model .NET Azure Functions follow [this link](https://learn.microsoft.com/en-us/azure/azure-functions/dotnet-isolated-in-process-differences)

## Getting Started

To run the sample Azure Functions Dapr Extension in Isolated process, follow [these steps][dotnet-out-of-proc-samples]

[dotnet-out-of-proc-samples]: ../samples/dotnet-isolated-azurefunction/README.md

