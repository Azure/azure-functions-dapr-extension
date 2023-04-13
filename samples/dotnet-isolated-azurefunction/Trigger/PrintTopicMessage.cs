// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// ------------------------------------------------------------

namespace dotnet_isolated_azurefunction
{
    using Azure.Messaging;
    using Microsoft.Azure.Functions.Worker;
    using Microsoft.Azure.Functions.Worker.Extensions.Dapr;
    using Microsoft.Extensions.Logging;

    public static class PrintTopicMessage
    {
        /// <summary>
        /// Sample to use Dapr Publish trigger to print any new message arrived on the subscribed topic.
        /// </summary>
        [Function("PrintTopicMessage")]
        public static void Run(
            [DaprTopicTrigger("%PubSubName%", Topic = "B")] CloudEvent subEvent,
            ILogger log)
        {
            log.LogInformation("C# function processed a PrintTopicMessage request from the Dapr Runtime.");
            log.LogInformation($"Topic B received a message: {subEvent.Data}.");
        }
    }
}
