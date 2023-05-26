namespace bindings_state
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.Dapr;
    using Microsoft.Azure.WebJobs.Extensions.Http;
    using Microsoft.Extensions.Logging;

    public static class StateInputBinding
    {
        [FunctionName("StateInputBinding")]
        public static IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "state/{key}")] HttpRequest req,
            [DaprState("statestore", Key = "{key}")] string state,
            ILogger log)
        {
            log.LogInformation("StateInputBinding: C# HTTP trigger function processed a request.");

            return state != null
                ? (ActionResult)new OkObjectResult(state)
                : new NotFoundObjectResult($"State not found, route: {req.Path}");
        }
    }
}