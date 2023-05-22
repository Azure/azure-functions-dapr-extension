# Azure Functions Dapr Extension - Out-of-Proc Support

This repository contains sample code demonstrating the out-of-proc support in the Dapr extension for Azure Functions using the .NET Isolated process.

## Overview

The out-of-proc support allows you to host your Azure Functions runtime in a separate process from Dapr, enabling better isolation and scalability. With this approach, the Azure Functions runtime and Dapr run as separate processes but communicate with each other using gRPC.

## Getting Started

To run the sample Azure Functions with out-of-proc support, follow [these steps][dotnet-out-of-proc-samples]

## Note

**Required fields in the payload for dapr bindings and triggers in out-of-proc execution model.**

# Function Triggers
[daprBindingTrigger][binding-trigger-docs]

| Field | Required | Example |
| -- | -- | -- |
| data | Y | ```'{\"data\":{\"message\": \"hello!\" }}'``` |

## Function output Bindings

[daprState][state-output-docs]

| Field | Required | Example |
| -- | -- | -- |
| value | Y | ```'{\"value\":{\"message\": \"hello!\" }}'``` |

[daprPublish][publish-output-docs]

| Field | Required | Example |
| -- | -- | -- |
| payload | Y | ```'{\"payload\":{\"message\": \"hello!\" }}'``` |

[daprBinding][binding-output-docs]

| Field | Required | Example |
| -- | -- | -- |
| data | Y | ```'{\"data\":{\"message\": \"hello!\" }}'``` |

[dotnet-out-of-proc-samples]: ../samples/dotnet-isolated-azurefunction/README.md

