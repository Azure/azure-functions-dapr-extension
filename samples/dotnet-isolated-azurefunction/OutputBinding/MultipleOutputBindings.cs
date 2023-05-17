namespace dotnet_isolated_azurefunction.OuputBinding
{
    using System.Net;
    using System.Text.Json;
    using Microsoft.Azure.Functions.Worker;
    using Microsoft.Azure.Functions.Worker.Extensions.Dapr;
    using Microsoft.Azure.Functions.Worker.Http;

    public class MultipleOutputBindings
    {
        [Function("MultiOutput")]
        public static MyOutputType Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "{key}")] HttpRequestData req,
        FunctionContext context)
        {
            var response = req.CreateResponse(HttpStatusCode.OK);
            response.WriteString("Success!");

            //Http trigger URI should look like : http://localhost:7071/api/anykey?somekey=somevalue
            var jsonDocument = JsonSerializer.SerializeToDocument(new { value = req.Url.Query });

            return new MyOutputType()
            {
                Data = jsonDocument.RootElement,
                HttpResponse = response
            };
        }
    }

    public class MyOutputType
    {
        [DaprStateOutput("%StateStoreName%", Key = "MultiOutputKey")]
        public JsonElement Data { get; set; }

        public HttpResponseData? HttpResponse { get; set; }
    }
}
