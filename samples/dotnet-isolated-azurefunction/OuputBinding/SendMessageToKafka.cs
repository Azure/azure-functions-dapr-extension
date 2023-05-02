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
        [DaprBindingOutput(BindingName = "%KafkaBindingName%", Operation = "create")]
        public static JsonElement Run(
            [DaprServiceInvocationTrigger] JsonElement payload, FunctionContext functionContext)
        {
            var log = functionContext.GetLogger("SendMessageToKafka");
            log.LogInformation("C#  function processed a SendMessageToKafka request.");

            return payload;
        }
    }
}