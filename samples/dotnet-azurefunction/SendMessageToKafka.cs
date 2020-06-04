// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// ------------------------------------------------------------

namespace dotnet_azurefunction
{
    using Microsoft.Azure.WebJobs;
    using Microsoft.Extensions.Logging;
    using Dapr.AzureFunctions.Extension;
    using Newtonsoft.Json.Linq;
    using System.Threading.Tasks;

    public static class SendMessageToKafka
    {
        [FunctionName("SendMessageToKafka")]
        public static async Task Run(
            [DaprServiceInvocationTrigger] JObject payload,
            [DaprBinding(BindingName = "%KafkaBindingName%")] IAsyncCollector<object> messages,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            await messages.AddAsync(payload);
        }
    }
}