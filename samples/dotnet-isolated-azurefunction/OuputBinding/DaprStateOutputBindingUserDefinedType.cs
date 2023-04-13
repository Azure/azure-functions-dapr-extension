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

    public static class DaprStateOutputBindingUserDefinedType
    {
        /// <summary>
        /// Example to use Dapr Service Invocation Trigger and Dapr State Output binding to persist a new state into statestore
        /// </summary>
        [Function("DaprStateOutputBindingUserDefinedType")]
        [DaprStateOutput("%StateStoreName%", Key = "product")]
        public static Product Run(
            [DaprServiceInvocationTrigger] Product payload,
            FunctionContext functionContext)
        {
            var log = functionContext.GetLogger("DaprStateOutputBindingUserDefinedType");
            log.LogInformation("C# function processed a DaprStateOutputBindingUserDefinedType request from the Dapr Runtime.");

            return payload;
        }
    }

    public class Product
    {
        [JsonPropertyName("Name")]
        public string? Name { get; set; }
        [JsonPropertyName("Description")]
        public string? Description { get; set; }
        [JsonPropertyName("Quantity")]
        public int Quantity { get; set; }
    }
}