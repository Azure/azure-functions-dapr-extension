// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// ------------------------------------------------------------

namespace dotnet_isolated_azurefunction
{
    using System.IO;
    using Microsoft.Azure.Functions.Extensions.Dapr.Core;
    using Microsoft.Azure.Functions.Worker;
    using Microsoft.Azure.Functions.Worker.Extensions.Dapr;
    using Microsoft.Azure.Functions.Worker.Http;

    public static class PublishOutputBinding
    {
        [Function("PublishOutputBinding")]
        [DaprPublishOutput(PubSubName = "%PubSubName%", Topic = "{topicName}")]
        public static DaprPubSubEvent Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "topic/{topicName}")] HttpRequestData req,
            FunctionContext functionContext)
        {
            string requestBody = new StreamReader(req.Body).ReadToEnd();

            return new DaprPubSubEvent(requestBody);
        }
    }
}
