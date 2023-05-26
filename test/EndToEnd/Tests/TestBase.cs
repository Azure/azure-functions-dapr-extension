namespace EndToEndTests
{
    public class TestBase
    {
        protected const string DEFAULT_FUNCTIONS_APP_URI = "http://localhost:7071";

        protected string FUNCTIONS_APP_URI;

        public TestBase()
        {
            FUNCTIONS_APP_URI = Environment.GetEnvironmentVariable("FUNCTIONS_APP_URI") ?? DEFAULT_FUNCTIONS_APP_URI;
        }
    }
}