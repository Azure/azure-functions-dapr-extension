namespace EndToEndTests.Framework
{
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// This class is used to setup and teardown the test app.
    /// A test app contains functions and a Dapr sidecar, 
    /// that is tested by the end-to-end tests.
    /// </summary>
    public class AppFixture : IDisposable
    {
        private readonly ILogger<AppFixture> logger;

        private readonly ITestEnvironment testEnvironment;

        public AppFixture()
        {
            string environmentType = Utils.GetEnvironmentVariable(Constants.EnvironmentKeys.TEST_APP_ENVIRONMENT);
            string containerRegistry = Utils.GetEnvironmentVariable(Constants.EnvironmentKeys.TEST_APP_REGISTRY);
            string containerTag = Utils.GetEnvironmentVariable(Constants.EnvironmentKeys.TEST_APP_TAG);

            // Setup logging
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
            });
            logger = loggerFactory.CreateLogger<AppFixture>();

            // Get the right test environment
            switch (environmentType)
            {
                case "local":     // Local developer machine
                    testEnvironment = new LocalTestEnvironment(logger, containerRegistry, containerTag);
                    break;
                case "funccapps": // Functions on Container Apps
                    testEnvironment = new FunctionsAcaTestEnvironment(logger, containerRegistry, containerTag);
                    break;
                default:
                    throw new ArgumentException($"Environment variable {Constants.EnvironmentKeys.TEST_APP_ENVIRONMENT} is not set to a valid value."
                        + " Valid values are: local, funccapps.");
            }

            // Start the test app
            // TODO: Make this configurable, so we can test different apps.
            TestApp = testEnvironment.StartAsync("csharpapp").Result;
        }

        public void Dispose()
        {
            // TODO: Make this configurable, so we can test different apps.
            testEnvironment.StopAsync("csharpapp").Wait();
        }

        public TestApp TestApp { get; private set; }
    }
}