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
    public static class StateOutputBinding
    {
        [FunctionName("StateOutputBinding")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "state/{key}")] HttpRequest req,
            [DaprState(StateStore = "statestore", Key = "{key}")] IAsyncCollector<SaveStateOptions> state,
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
