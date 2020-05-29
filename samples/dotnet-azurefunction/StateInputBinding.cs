// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// ------------------------------------------------------------

namespace dotnet_azurefunction
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.Http;
    using Microsoft.AspNetCore.Http;
    using Dapr.AzureFunctions.Extension;
    using Microsoft.Extensions.Logging;

    public static class StateInputBinding
    {
        [FunctionName("StateInputBinding")]
        public static IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "state/{key}")] HttpRequest req,
            [DaprState("statestore", Key = "{key}")] string state,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            return new OkObjectResult(state);
        }
    }
}