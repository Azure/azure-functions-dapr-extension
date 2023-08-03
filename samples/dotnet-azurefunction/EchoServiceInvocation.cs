// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// ------------------------------------------------------------

namespace dotnet_azurefunction
{
    using System;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.Dapr;
    using Microsoft.Extensions.Logging;

    public static class EchoServiceInvocation
    {
        // The function is triggered the service invocation Dapr trigger,
        // and is echoes back the same message via a HTTP output binding.
        [FunctionName("EchoServiceInvocation")]
        public static IActionResult Run(
            [DaprServiceInvocationTrigger] string message,
            ILogger log)
        {
            log.LogInformation("C# function received a EchoServiceInvocation request.");

            log.LogInformation($"Echoing message: {message}");

            // Return the same payload.
            return !String.IsNullOrWhiteSpace(message) ?
                (ActionResult)new OkObjectResult(message) :
                new BadRequestObjectResult("Please pass a message in the request body.");
        }
    }
}