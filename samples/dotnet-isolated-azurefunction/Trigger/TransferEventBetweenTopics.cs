// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// ------------------------------------------------------------

namespace dotnet_isolated_azurefunction
{
    using CloudNative.CloudEvents;
    using Microsoft.Azure.Functions.Extensions.Dapr.Core;
    using Microsoft.Azure.Functions.Worker;
    using Microsoft.Azure.Functions.Worker.Extensions.Dapr;
    using Microsoft.Extensions.Logging;

    public static class TransferEventBetweenTopics
    {
        /// <summary>
        /// Sample to use Dapr Topic Trigger and Dapr Publish Output Binding to subscribe to a message bus 
        /// and then republish it to another topic with edited message content
        /// </summary>
        [Function("TransferEventBetweenTopics")]
        [DaprPublishOutput(PubSubName = "%PubSubName%", Topic = "B")]
        public static DaprPubSubEvent Run(
            [DaprTopicTrigger("%PubSubName%", Topic = "A")] CloudEvent subEvent, FunctionContext functionContext)
        {
            var log = functionContext.GetLogger("TransferEventBetweenTopics");
            log.LogInformation("C# function processed a TransferEventBetweenTopics request from the Dapr Runtime.");

            return new DaprPubSubEvent("Transfer from Topic A: " + subEvent.Data);
        }
    }
}
