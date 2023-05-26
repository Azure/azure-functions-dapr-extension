namespace EndToEndTests.Tester
{
    public class LocalTestEnvironment : TestEnvironmentBase
    {
        public override void Setup()
        {
            Environment.SetEnvironmentVariable(Constants.ENVKEY_HOST_URI, "http://localhost");
            Environment.SetEnvironmentVariable(Constants.ENVKEY_APP_PORT, "7071");
            Environment.SetEnvironmentVariable(Constants.ENVKEY_DAPR_HTTP_PORT, "3500");
        }

        public override void Start(TestApp app)
        {
            // Run the following command to start the test app with a dapr sidecar:
            // dapr run --app-id <app-name> --app-port <port> --dapr-http-port <port> --components-path <path> -- func host start


        }

        public override void Stop(TestApp app)
        {
            throw new NotImplementedException();
        }

        public override void TearDown()
        {
            // Nothing to do here.
        }
    }
}