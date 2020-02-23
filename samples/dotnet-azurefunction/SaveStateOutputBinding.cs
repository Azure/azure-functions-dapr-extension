using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.WebJobs.Extensions.Dapr;

namespace dotnet_azurefunction
{
    public static class SaveStateOutputBinding
    {
        [FunctionName("SaveStateOutputBinding")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            // [DaprState(StateStore = "statestore", Key = "test")] IAsyncCollector<string> state,
            [DaprState(StateStore = "stateStore", Key = "hello")] IAsyncCollector<SaveStateOptions> state,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            await state.AddAsync(new SaveStateOptions() {
                Value = requestBody
            });

            return new OkResult();
        }
    }
}
