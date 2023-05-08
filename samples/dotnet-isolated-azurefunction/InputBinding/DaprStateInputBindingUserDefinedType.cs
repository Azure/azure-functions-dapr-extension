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
            var log = functionContext.GetLogger("DaprStateInputBindingUserDefinedType");
            log.LogInformation("C# function processed a DaprStateInputBindingUserDefinedType request from the Dapr Runtime.");

            //print the fetched state value
            log.LogInformation(JsonSerializer.Serialize(data));
        }
    }
}