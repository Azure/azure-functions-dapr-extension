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

        public async Task<TestApp> StartAsync(string appName)
        {
            // Set the functions container image
            return new TestApp("", 1);
        }

        public Task StopAsync(string appName)
        {
            throw new NotImplementedException();
        }
    }
}