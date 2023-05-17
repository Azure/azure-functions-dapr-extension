// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// ------------------------------------------------------------

namespace dotnet_azurefunction
{
    using CloudNative.CloudEvents;
    using Microsoft.Azure.Functions.Extensions.Dapr.Core;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.Dapr;
    using Microsoft.Extensions.Logging;

    public static class TransferEventBetweenTopics
    {
        /// <summary>
        /// Sample to use Dapr Topic Trigger and Dapr Publish Output Binding to subscribe to a message bus 
        /// and then republish it to another topic with edited message content
        /// </summary>
        [FunctionName("TransferEventBetweenTopics")]
        public static void Run(
            [DaprTopicTrigger("%PubSubName%", Topic = "A")] CloudEvent subEvent,
            [DaprPublish(PubSubName = "%PubSubName%", Topic = "B")] out DaprPubSubEvent pubEvent,
            ILogger log)
        {
            log.LogInformation("C# function processed a TransferEventBetweenTopics request from the Dapr Runtime.");


            pubEvent = new DaprPubSubEvent("Transfer from Topic A: " + subEvent.Data);
        }
    }
}
