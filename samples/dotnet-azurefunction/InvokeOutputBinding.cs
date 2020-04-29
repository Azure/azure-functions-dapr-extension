// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// ------------------------------------------------------------

namespace dotnet_azurefunction
{
    using System.IO;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.Http;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using Microsoft.Azure.WebJobs.Extensions.Dapr;
    using Newtonsoft.Json.Linq;

    public static class InvokeOutputBinding
    {
        [FunctionName("InvokeOutputBinding")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "invoke/{methodName}")] HttpRequest req,
            [DaprInvoke(AppId = "function2", MethodName = "{methodName}", HttpVerb = "post")] IAsyncCollector<InvokeMethodParameters> output,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            var outputContent = new InvokeMethodParameters
            {
                Body = JObject.FromObject(new 
                {
                    message = requestBody
                })
            };

            await output.AddAsync(outputContent);

            return new OkResult();
        }
    }
}