// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// ------------------------------------------------------------

namespace dotnet_azurefunction
{
    using Microsoft.Azure.WebJobs;
    using Microsoft.Extensions.Logging;
    using Microsoft.Azure.WebJobs.Extensions.Dapr;

    public static class RetrieveOrder
    {
        /// <summary>
        /// Example to use Dapr Service Invocation Trigger and Dapr State input binding to retrieve a saved state from statestore
        /// </summary>
        [FunctionName("RetrieveOrder")]
        public static void Run(
            [DaprServiceInvocationTrigger] object args,
            [DaprState("%StateStoreName%", Key = "order")] string data,
            ILogger log)
        {
            log.LogInformation("C# function processed a RetrieveOrder request from the Dapr Runtime.");

            // print the fetched state value
            log.LogInformation(data);
        }
    }
}