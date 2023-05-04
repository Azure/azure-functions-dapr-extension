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

    public static class DaprStateInputBindingJsonElementType
    {
        /// <summary>
        /// Example to use Dapr Service Invocation Trigger and Dapr State input binding to retrieve a saved state from statestore
        /// </summary>
        [Function("DaprStateInputBindingJsonElementType")]
        public static void Run(
            [DaprServiceInvocationTrigger] object args,
            [DaprStateInput("%StateStoreName%", Key = "DaprStateBindingJsonElementType")] JsonElement data, FunctionContext functionContext)
        {
            var log = functionContext.GetLogger("DaprStateInputBindingJsonElementType");
            log.LogInformation("C# function processed a DaprStateInputBindingJsonElementType request from the Dapr Runtime.");

            //print the fetched state value
            log.LogInformation(JsonSerializer.Serialize(data));
        }
    }
}