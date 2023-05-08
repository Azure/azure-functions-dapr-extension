// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// ------------------------------------------------------------

namespace dotnet_isolated_azurefunction
{
    using Microsoft.Azure.Functions.Worker;
    using Microsoft.Azure.Functions.Worker.Extensions.Dapr;
    using Microsoft.Azure.Functions.Worker.Http;
    using Microsoft.Extensions.Logging;

    public static class StateInputBinding
    {
        [Function("StateInputBinding")]
        public static string Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "state/{key}")] HttpRequestData req,
            [DaprStateInput("statestore", Key = "{key}")] string state, 
            FunctionContext functionContext)
        {
            var log = functionContext.GetLogger("StateInputBinding");
            log.LogInformation("C# HTTP trigger function processed a request.");

            return state;
        }
    }
}