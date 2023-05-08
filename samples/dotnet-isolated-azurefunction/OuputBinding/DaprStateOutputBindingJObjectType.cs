// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// ------------------------------------------------------------

namespace dotnet_isolated_azurefunction
{
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using Microsoft.Azure.Functions.Worker;
    using Microsoft.Azure.Functions.Worker.Extensions.Dapr;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json.Linq;

    public static class DaprStateOutputBindingJObjectType
    {
        /// <summary>
        /// Example to use Dapr Service Invocation Trigger and Dapr State Output binding to persist a new state into statestore
        /// </summary>
        [Function("DaprStateOutputBindingJObjectType")]
        [DaprStateOutput("%StateStoreName%", Key = "DaprStateBindingJObjectType")]
        public static JObject Run(
            [DaprServiceInvocationTrigger] JObject payload,
            FunctionContext functionContext)
        {
            var log = functionContext.GetLogger("DaprStateOutputBindingJObjectType");
            log.LogInformation("C# function processed a DaprStateOutputBindingJObjectType request from the Dapr Runtime.");

            return payload;
        }
    }
}