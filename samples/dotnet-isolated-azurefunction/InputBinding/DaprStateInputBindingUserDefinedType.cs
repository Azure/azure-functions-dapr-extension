// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// ------------------------------------------------------------

namespace dotnet_azurefunction
{
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using Microsoft.Azure.Functions.Worker;
    using Microsoft.Azure.Functions.Worker.Extensions.Dapr;
    using Microsoft.Extensions.Logging;

    public static class DaprStateInputBindingUserDefinedType
    {
        /// <summary>
        /// Example to use Dapr Service Invocation Trigger and Dapr State input binding to retrieve a saved state from statestore
        /// </summary>
        [Function("DaprStateInputBindingUserDefinedType")]
        public static void Run(
            [DaprServiceInvocationTrigger] object args,
            [DaprStateInput("%StateStoreName%", Key = "product")] Product data, FunctionContext functionContext)
        {
            var log = functionContext.GetLogger("StateInputBinding");
            log.LogInformation("C# function processed a DaprStateInputBindingUserDefinedType request from the Dapr Runtime.");

            // print the fetched state value
            log.LogInformation(JsonSerializer.Serialize(data));
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