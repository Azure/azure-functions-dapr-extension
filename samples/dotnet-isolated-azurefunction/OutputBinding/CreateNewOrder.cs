// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// ------------------------------------------------------------

namespace dotnet_isolated_azurefunction
{
    using System.Text.Json;
    using Microsoft.Azure.Functions.Worker;
    using Microsoft.Azure.Functions.Worker.Extensions.Dapr;
    using Microsoft.Extensions.Logging;

    public static class CreateNewOrder
    {
        /// <summary>
        /// Example to use Dapr Service Invocation Trigger and Dapr State Output binding to persist a new state into statestore
        /// </summary>
        [Function("CreateNewOrder")]
        [DaprStateOutput("%StateStoreName%", Key = "order")]
        public static JsonElement Run(
            [DaprServiceInvocationTrigger] JsonElement payload, 
            FunctionContext functionContext)
        {
            var log = functionContext.GetLogger("CreateNewOrder");
            log.LogInformation("C# function processed a CreateNewOrder request from the Dapr Runtime.");

            // payload must be of the format { "data": { "value": "some value" } }
            payload.TryGetProperty("data", out JsonElement data);

            return data;
        }
    }
}