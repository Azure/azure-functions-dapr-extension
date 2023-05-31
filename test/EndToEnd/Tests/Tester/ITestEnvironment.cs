namespace EndToEndTests.Tester
{
    public interface ITestEnvironment
    {
        /// <summary>
        /// Starts a test app with the given name.
        /// </summary>
        /// <param name="appName">The name of the app to start.</param>
        /// <returns>The <see cref="TestApp"/> instance.</returns>
        Task<TestApp> StartAsync(string appName);

        /// <summary>
        /// Stops the test app with the given name.
        /// </summary>
        /// <param name="appName">The name of the app to stop.</param>
        Task StopAsync(string appName);
    }
}