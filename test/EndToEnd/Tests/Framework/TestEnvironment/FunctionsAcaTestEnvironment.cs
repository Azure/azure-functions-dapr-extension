namespace EndToEndTests.Framework
{
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Represents a test environment for Functions on Azure Container Apps.
    /// Runs the test app as a container in Azure Container Apps, where Dapr is injected as a sidecar automatically.
    /// </summary>
    class FunctionsAcaTestEnvironment : ITestEnvironment
    {
        private const string MANAGE_AZURE_SCRIPT_RELATIVE_PATH = "./Framework/Scripts/manage-azfunc.sh";

        private ILogger logger;
        private string containerRegistry;
        private string containerTag;
        private string azResourceGroup;
        private string azFunctionsApp;

        /// <summary>
        /// Initializes a new instance of the <see cref="FunctionsAcaTestEnvironment"/> class.
        /// </summary>
        /// <param name="logger">The logger to use.</param>
        /// <param name="containerRegistry">The container registry where the test app is located.</param>
        /// <param name="containerTag">The tag to use for the test app image.</param>
        public FunctionsAcaTestEnvironment(ILogger logger, string containerRegistry, string containerTag)
        {
            this.logger = logger;
            this.containerRegistry = containerRegistry;
            this.containerTag = containerTag;

            this.azResourceGroup = Utils.GetEnvironmentVariable(Constants.EnvironmentKeys.TEST_FUNCCAPPS_RESOURCE_GROUP);
            this.azFunctionsApp = Utils.GetEnvironmentVariable(Constants.EnvironmentKeys.TEST_FUNCCAPPS_NAME);
        }

        /// <summary>
        /// Waits for the function app to be ready.
        /// TODO: Instead of waiting a fixed amount of time, we should poll the function app until it is ready.
        /// </summary>
        private static async Task WaitForFunctionAppAsync()
        {
            var functionAppWaitTimeSeconds = 10;
            try
            {
                functionAppWaitTimeSeconds = int.Parse(Utils.GetEnvironmentVariable(Constants.EnvironmentKeys.TEST_FUNCCAPPS_APP_WAIT_TIME));
            }
            catch (System.Exception)
            {
                // Ignore
            }

            await Task.Delay(functionAppWaitTimeSeconds * 1000);
        }

        public async Task<TestApp> StartAsync(string appName)
        {
            try
            {
                this.logger.LogInformation($"Deploying function app {appName} to Azure Container Apps.");

                string scriptPath = System.IO.Path.Combine(
                    System.IO.Directory.GetCurrentDirectory(),
                    "../../../",
                    MANAGE_AZURE_SCRIPT_RELATIVE_PATH);

                // Deploy the function app
                var res = Script.InvokeScript(scriptPath, new string[] { "deploy", appName });
                if (res.ExitCode != 0)
                {
                    throw new System.Exception($"Failed to deploy function app {appName} to Azure Container Apps, stderr: {res.StdErr}");
                }

                this.logger.LogInformation($"Successfully deployed function app {appName} to Azure Container Apps.");

                this.logger.LogInformation("Waiting for function app to be ready (10s)...");
                await WaitForFunctionAppAsync();

                this.logger.LogInformation($"Getting URL for function app {appName}.");

                // Get the URL of the function app
                res = Script.InvokeScript(scriptPath, new string[] { "geturl" });
                if (res.ExitCode != 0)
                {
                    this.logger.LogError($"Failed to get URL for function app {appName}, stderr: {res.StdErr}");
                    throw new System.Exception($"Failed to get URL for function app {appName}, stderr: {res.StdErr}");
                }

                string url = res.StdOut?.Trim() ?? new System.Exception($"Failed to get URL for function app {appName}.").ToString();
                return new TestApp("https://" + url, 443);
            }
            catch (System.Exception e)
            {
                this.logger.LogError($"Failed to deploy function app {appName} to Azure Container Apps, exception: {e.Message}");
                throw;
            }
        }

        public Task StopAsync(string appName)
        {
            // Do nothing, the environment cleanup will take care of this.
            return Task.CompletedTask;
        }
    }
}