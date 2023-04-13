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
    using Newtonsoft.Json.Linq;

    public static class TransferEventBetweenTopics
    {
        /// <summary>
        /// Sample to use Dapr Topic Trigger and Dapr Publish Output Binding to subscribe to a message bus 
        /// and then republish it to another topic with edited message content
        /// </summary>
        [Function("TransferEventBetweenTopics")]
        public static void Run(
            [DaprTopicTrigger("%PubSubName%", Topic = "A")] CloudEvent subEvent,
            //[DaprPublishOutput(PubSubName = "%PubSubName%", Topic = "B")] out object pubEvent,
            ILogger log)
        {
            log.LogInformation("C# function processed a TransferEventBetweenTopics request from the Dapr Runtime.");

            //TODO: add DaprPubSubEvent
            //pubEvent = new DaprPubSubEvent("Transfer from Topic A: " + subEvent.Data);
        }
    }
}
