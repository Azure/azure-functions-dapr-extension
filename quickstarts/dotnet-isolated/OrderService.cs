namespace OrderServiceDaprFunc
{
    using Microsoft.Azure.Functions.Extensions.Dapr.Core;
    using Microsoft.Azure.Functions.Worker;
    using Microsoft.Azure.Functions.Worker.Extensions.Dapr;
    using Microsoft.Azure.Functions.Worker.Http;
    using Microsoft.Extensions.Logging;
    using System.IO;
    using System.Text.Json;
    using System.Threading.Tasks;

    public static class OrderService
    {
        /// <summary>
        /// Sample to use a Dapr Invoke Output Binding to perform a Dapr Server Invocation operation hosted in another Darp'd app.
        /// Here this function acts like a proxy
        /// </summary>
        [Function("OrderService")]
        [DaprInvokeOutput(AppId = "{appId}", MethodName = "{methodName}", HttpVerb = "post")]
        public static async Task<InvokeMethodParameters> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "invoke/{appId}/{methodName}")] HttpRequestData req, 
            FunctionContext functionContext)
        {
            var log = functionContext.GetLogger("OrderService");
            log.LogInformation("C# HTTP trigger function processed a request.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            //print the received payload
            log.LogInformation($"Received Payload OrderService: {requestBody}");

            var outputContent = new InvokeMethodParameters
            {
                Body = requestBody
            };

            return outputContent;
        }
    }
}