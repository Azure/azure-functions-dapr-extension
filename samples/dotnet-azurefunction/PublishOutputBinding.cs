// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// ------------------------------------------------------------

namespace dotnet_azurefunction
{
    using System.IO;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extension.Dapr;
    using Microsoft.Azure.WebJobs.Extensions.Http;
    using Microsoft.Extensions.Logging;

    public static class PublishOutputBinding
    {
        [FunctionName("PublishOutputBinding")]
        public static void Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "topic/{topicName}")] HttpRequest req,
            [DaprPublish(PubSubName = "%PubSubName%", Topic = "{topicName}")] out DaprPubSubEvent pubSubEvent,
            ILogger log)
        {
            string requestBody = new StreamReader(req.Body).ReadToEnd();
            pubSubEvent = new DaprPubSubEvent(requestBody);
        }
    }
}
