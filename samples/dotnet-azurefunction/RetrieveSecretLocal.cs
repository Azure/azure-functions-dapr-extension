// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// ------------------------------------------------------------

namespace dotnet_azurefunction
{
    using Microsoft.Azure.WebJobs;
    using Microsoft.Extensions.Logging;
    using Microsoft.Azure.WebJobs.Extensions.Dapr;
    using System.Collections.Generic;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    public static class RetrieveSecretLocal
    {
        /// <summary>
        /// Example to use Dapr Service Invocation Trigger and Dapr Secret input binding to retrieve a saved state from statestore
        /// </summary>
        [FunctionName("RetrieveSecretLocal")]
        public static void Run(
            [DaprServiceInvocationTrigger] object args,
            [DaprSecret("localsecretstore", "mysecret", Metadata = "metadata.namespace=default")] Data secret,
            ILogger log)
        {
            log.LogInformation("C# function processed a RetrieveSecret request from the Dapr Runtime.");

            // print the fetched secret value
            // this is only for demo purpose
            // please do not log any real secret in your production code        
            //foreach (var kvp in secret)
            //{
            //    log.LogInformation("Stored secret: Key = {0}, Value = {1}", kvp.Key, kvp.Value);
            //}

            log.LogInformation(JsonSerializer.Serialize(secret));

        }

        public class Data
        {
            [JsonPropertyName("mysecret")]
            public Secret mysecret { get; set; }
        }

        public class Secret
        {
            [JsonPropertyName("token")]
            public string Token { get; set; }
        }
    }
}