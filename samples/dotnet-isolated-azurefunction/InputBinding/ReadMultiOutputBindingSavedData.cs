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

    public static class ReadMultiOutputBindingSavedData
    {
        /// <summary>
        /// Example to use Dapr Service Invocation Trigger and Dapr State input binding to retrieve a saved state from statestore
        /// </summary>
        [Function("ReadMultiOutputBindingSavedData")]
        public static void Run(
            [DaprServiceInvocationTrigger] object args,
            [DaprStateInput("%StateStoreName%", Key = "MultiOutputKey")] string data, FunctionContext functionContext)
        {
            var log = functionContext.GetLogger("ReadMultiOutputBindingSavedData");
            log.LogInformation("C# function processed a ReadMultiOutputBindingSavedData request from the Dapr Runtime.");

            //print the fetched state value
            log.LogInformation(data);
        }
    }
}