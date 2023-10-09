namespace OrderServiceDaprFunc
{
    using System.Text.Json;
    using Microsoft.Azure.Functions.Worker;
    using Microsoft.Azure.Functions.Worker.Extensions.Dapr;
    using Microsoft.Azure.Functions.Worker.Http;
    using Microsoft.Extensions.Logging;

    public static class RetrieveOrder
    {
        /// <summary>
        /// Example to use Dapr Service Invocation Trigger and Dapr State input binding to retrieve a saved state from statestore
        /// </summary>
        [Function("RetrieveOrder")]
        public static JsonElement Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "RetrieveOrder")] HttpRequestData req,
            [DaprStateInput("%StateStoreName%", Key = "order")] JsonElement data, 
            FunctionContext functionContext)
        {
            var log = functionContext.GetLogger("RetrieveOrder");
            log.LogInformation("C# function processed a RetrieveOrder request from the Dapr Runtime.");

            //print the fetched state value
            log.LogInformation($"Retrieved order: {JsonSerializer.Serialize(data)}");

            return data;
        }
    }
}
