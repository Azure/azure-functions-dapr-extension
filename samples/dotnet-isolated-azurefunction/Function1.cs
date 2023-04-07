namespace dotnet_isolated_azurefunction
{
    using System.Net;
    using Google.Protobuf.WellKnownTypes;
    using Grpc.Net.Client.Configuration;
    using Microsoft.Azure.Functions.Worker;
    using Microsoft.Azure.Functions.Worker.Extensions.Dapr;
    using Microsoft.Azure.Functions.Worker.Http;
    using Microsoft.Extensions.Logging;

    public class Function1
    {
        private readonly ILogger _logger;

        public Function1(ILoggerFactory loggerFactory)
        {
            this._logger = loggerFactory.CreateLogger<Function1>();
        }

        [Function("Function1")]
        public HttpResponseData Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req)
        {
            this._logger.LogInformation("C# HTTP trigger function processed a request.");

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

            response.WriteString("Welcome to Azure Functions!");

            return response;
        }
    }
}
