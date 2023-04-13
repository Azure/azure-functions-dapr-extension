// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// ------------------------------------------------------------

namespace dotnet_isolated_azurefunction
{
    using Microsoft.Azure.Functions.Worker;
    using Microsoft.Azure.Functions.Worker.Extensions.Dapr;
    using Microsoft.Extensions.Logging;

    public static class RetrieveOrder
    {
        /// <summary>
        /// Example to use Dapr Service Invocation Trigger and Dapr State input binding to retrieve a saved state from statestore
        /// </summary>
        [Function("RetrieveOrder")]
        public static void Run(
            [DaprServiceInvocationTrigger] object args,
            [DaprStateInput("%StateStoreName%", Key = "order")] string data, FunctionContext functionContext)
        {
            var log = functionContext.GetLogger("RetrieveOrder");
            log.LogInformation("C# function processed a RetrieveOrder request from the Dapr Runtime.");

            //print the fetched state value
            log.LogInformation(data);
        }
    }
}