namespace EndToEndTests.Tester
{
    public interface ITestEnvironment
    {
        /// <summary>
        /// Starts a test app with the given name.
        /// </summary>
        /// <param name="appName">The name of the app to start.</param>
        /// <param name="appPort">The port where the app listens on.</param>
        /// <returns>The <see cref="TestApp"/> instance.</returns>
        Task<TestApp> StartAsync(string appName, int appPort);

        /// <summary>
        /// Stops the test app with the given name.
        /// </summary>
        /// <param name="appName">The name of the app to stop.</param>
        Task StopAsync(string appName);
    }
}