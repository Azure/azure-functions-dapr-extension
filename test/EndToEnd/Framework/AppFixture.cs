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
            string environmentType = GetEnvironmentVariable(Constants.EnvironmentKeys.TEST_APP_ENVIRONMENT);
            string containerRegistry = GetEnvironmentVariable(Constants.EnvironmentKeys.TEST_APP_REGISTRY);
            string containerTag = GetEnvironmentVariable(Constants.EnvironmentKeys.TEST_APP_TAG);

            // Setup logging
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
            });
            logger = loggerFactory.CreateLogger<AppFixture>();

            // Get the right test environment
            switch (environmentType)
            {
                case "local":
                    testEnvironment = new LocalTestEnvironment(logger, containerRegistry, containerTag);
                    break;
                default:
                    throw new ArgumentException($"Environment variable {Constants.EnvironmentKeys.TEST_APP_ENVIRONMENT} is not set to a valid value."
                        + " Valid values are: local");
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

        /// <summary>
        /// Gets the value of an environment variable.
        /// Throws an exception if the environment variable is not set.
        /// </summary>
        /// <param name="key">The name of the environment variable.</param>
        /// <returns>The value of the environment variable.</returns>
        /// <exception cref="SystemException">Thrown if the environment variable is not set.</exception>
        private string GetEnvironmentVariable(string key)
        {
            return Environment.GetEnvironmentVariable(key) ??
                throw new SystemException($"Environment variable {key} is not set.");
        }

        public TestApp TestApp { get; private set; }
    }
}