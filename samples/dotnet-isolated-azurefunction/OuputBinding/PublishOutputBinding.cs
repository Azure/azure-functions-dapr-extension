// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// ------------------------------------------------------------

namespace dotnet_isolated_azurefunction
{
    //using System.IO;
    //using Microsoft.AspNetCore.Http;
    //using Microsoft.Azure.Functions.Worker;
    //using Microsoft.Azure.Functions.Worker.Extensions.Dapr;
    //using Microsoft.Azure.Functions.Worker.Http;
    //using Microsoft.Extensions.Logging;

    //public static class PublishOutputBinding
    //{
    //    [Function("PublishOutputBinding")]
    //    public static void Run(
    //        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "topic/{topicName}")] HttpRequestData req,
    //        [DaprPublishOutput(PubSubName = "%PubSubName%", Topic = "{topicName}")] out DaprPubSubEvent pubSubEvent,
    //        FunctionContext functionContext)
    //    {
    //        string requestBody = new StreamReader(req.Body).ReadToEnd();
    //        pubSubEvent = new DaprPubSubEvent(requestBody);
    //    }
    //}
}
