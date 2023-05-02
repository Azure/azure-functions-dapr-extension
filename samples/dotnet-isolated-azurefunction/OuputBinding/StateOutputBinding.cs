// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// ------------------------------------------------------------

namespace dotnet_isolated_azurefunction
{
    using System.IO;
    using Microsoft.Azure.Functions.Worker;
    using Microsoft.Azure.Functions.Worker.Extensions.Dapr;
    using Microsoft.Azure.Functions.Worker.Http;
    using Microsoft.Extensions.Logging;

    public static class StateOutputBinding
    {
        [Function("StateOutputBinding")]
        [DaprStateOutput("statestore", Key = "{key}")]
        public static string Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "state/{key}")] HttpRequestData req,
            FunctionContext functionContext)
        {
            var log = functionContext.GetLogger("StateOutputBinding");
            log.LogInformation("C# HTTP trigger function processed a request.");

            string requestBody = new StreamReader(req.Body).ReadToEnd();

            return requestBody;
        }
    }
}