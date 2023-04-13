// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// ------------------------------------------------------------

namespace dotnet_isolated_azurefunction
{
    using Microsoft.Azure.Functions.Worker;
    using Microsoft.Azure.Functions.Worker.Extensions.Dapr;
    using Microsoft.Extensions.Logging;
    using System.Text.Json;
    using System.Threading.Tasks;

    public static class SendMessageToKafka
    {
        [Function("SendMessageToKafka")]
        public static void Run(
            [DaprServiceInvocationTrigger] JsonElement payload,
            [DaprBindingOutput(BindingName = "%KafkaBindingName%", Operation = "create")] out object messages,
            ILogger log)
        {
            log.LogInformation("C#  function processed a SendMessageToKafka request.");

            messages = payload;
        }
    }
}