// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// ------------------------------------------------------------

namespace dotnet_isolated_azurefunction
{
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using Microsoft.Azure.Functions.Worker;
    using Microsoft.Azure.Functions.Worker.Extensions.Dapr;
    using Microsoft.Extensions.Logging;

    public static class DaprStateOutputBindingJsonElementType
    {
        /// <summary>
        /// Example to use Dapr Service Invocation Trigger and Dapr State Output binding to persist a new state into statestore
        /// </summary>
        [Function("DaprStateOutputBindingJsonElementType")]
        [DaprStateOutput("%StateStoreName%", Key = "DaprStateBindingJsonElementType")]
        public static JsonElement Run(
            [DaprServiceInvocationTrigger] JsonElement payload,
            FunctionContext functionContext)
        {
            var log = functionContext.GetLogger("DaprStateOutputBindingJsonElementType");
            log.LogInformation("C# function processed a DaprStateOutputBindingJsonElementType request from the Dapr Runtime.");

            return payload;
        }
    }
}