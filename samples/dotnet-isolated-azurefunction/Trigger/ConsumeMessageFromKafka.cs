// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// ------------------------------------------------------------

namespace dotnet_isolated_azurefunction
{
    using System.Text.Json;
    using Microsoft.Azure.Functions.Worker;
    using Microsoft.Azure.Functions.Worker.Extensions.Dapr;
    using Microsoft.Extensions.Logging;

    public static class ConsumeMessageFromKafka
    {
        // The function is triggered by Kafka messages in the Kafka instance referenced by
        // the Kafka binding configured under components/kafka-bindings.yaml
        // Can be used as an alternative for the node-app in the Dapr Bindings sample
        // found at https://github.com/dapr/quickstarts/tree/master/bindings/nodeapp
        [Function("ConsumeMessageFromKafka")]
        public static void Run(
            // Note: the value of BindingName must match the binding name in components/kafka-bindings.yaml
            [DaprBindingTrigger(BindingName = "%KafkaBindingName%")] JsonElement triggerData,
            FunctionContext functionContext)
        {
            var log = functionContext.GetLogger("ConsumeMessageFromKafka");

            log.LogInformation("Hello from Kafka!");

            log.LogInformation($"Trigger data: {triggerData}");
        }
    }
}