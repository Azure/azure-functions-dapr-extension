using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.WebJobs.Extensions.Dapr;
using Microsoft.Azure.WebJobs.Extensions.Dapr.Bindings;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace dotnet_azurefunction
{
    public static class KafkaTrigger
    {
        [FunctionName("KafkaTrigger")]
        public static void Run(
            [DaprTrigger(TriggerName = "sample-topic")] JObject triggerData,
            ILogger log)
        {
            log.LogInformation("Hello from Kafka!");

            log.LogInformation($"Trigger data: {triggerData}");
        }
    }
}
