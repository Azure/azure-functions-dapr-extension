namespace EndToEndTests.Tester
{
    public abstract class TestEnvironmentBase
    {
        /// <summary>
        /// Sets up the test environment.
        /// Any resources that are needed for the test environment should be created here.
        /// </summary>
        abstract public void Setup();

        /// <summary>
        /// Tears down the test environment.
        /// Any resources that were created in the Setup method should be destroyed here.
        /// </summary>
        abstract public void TearDown();

        /// <summary>
        /// Starts the specified test app, and sets DAPR_TEST_FUNCTIONS_APP_URI.
        /// </summary>
        abstract public void Start(TestApp app);

        /// <summary>
        /// Stops the specified test app.
        /// </summary>
        abstract public void Stop(TestApp app);
    }
}