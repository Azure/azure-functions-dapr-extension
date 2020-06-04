// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// ------------------------------------------------------------

namespace dotnet_azurefunction
{
    using CloudNative.CloudEvents;
    using Microsoft.Azure.WebJobs;
    using Dapr.AzureFunctions.Extension;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json.Linq;

    public static class TransferEventBetweenTopics
    {
        /// <summary>
        /// Sample to use Dapr Topic Trigger and Dapr Publish Output Binding to subscribe to a message bus 
        /// and then republish it to another topic with edited message content
        /// </summary>
        [FunctionName("TransferEventBetweenTopics")]
        public static void Run(
            [DaprTopicTrigger(Topic = "A")] CloudEvent subEvent,
            [DaprPublish(Topic = "B")] out object pubEvent,
            ILogger log)
        {
            log.LogInformation("C# function processed a TransferEventBetweenTopics request from the Dapr Runtime.");


            pubEvent = new DaprPubSubEvent("Transfer from Topic A: " + subEvent.Data);
        }
    }
}
