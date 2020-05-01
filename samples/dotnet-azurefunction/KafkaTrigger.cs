using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.WebJobs.Extensions.Dapr;
using Newtonsoft.Json.Linq;

namespace dotnet_azurefunction
{
    public static class KafkaTrigger
    {
        // The function is triggered by Kafka messages in the Kafka instance referenced by
        // the Kafka binding configured under components/kafka-bindings.yaml
        // Can be used as an alternative for the node-app in the Dapr Bindings sample
        // found at https://github.com/dapr/samples/tree/master/5.bindings/nodeapp
        [FunctionName("KafkaTrigger")]
        public static void Run(
            // Note: the value of BindingName must match the binding name in components/kafka-bindings.yaml
            [DaprBindingTrigger(BindingName = "sample-topic")] JObject triggerData,
            ILogger log)
        {
            log.LogInformation("Hello from Kafka!");

            log.LogInformation($"Trigger data: {triggerData}");
        }
    }
}
