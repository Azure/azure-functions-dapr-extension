// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// ------------------------------------------------------------

namespace dotnet_azurefunction
{
    using Microsoft.Azure.WebJobs;
    using Microsoft.Extensions.Logging;
    using Microsoft.Azure.WebJobs.Extensions.Dapr;
    using Newtonsoft.Json.Linq;
    using System.Threading.Tasks;

    public static class SendMessageToKafka
    {
        [FunctionName("SendMessageToKafka")]
        public static async Task Run(
            [DaprServiceInvocationTrigger] JObject payload,
            [DaprBinding(BindingName = "%KafkaBindingName%", Operation = "create")] IAsyncCollector<object> messages,
            ILogger log)
        {
            log.LogInformation("C#  function processed a SendMessageToKafka request.");

            await messages.AddAsync(payload);
        }
    }
}