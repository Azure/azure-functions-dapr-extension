namespace EndToEndTests.Tests
{
    using EndToEndTests.Framework;
    using Xunit;

    /// <summary>
    /// Base class for all tests
    /// </summary>
    [Collection("AppCollection")]
    public class TestBase
    {
        AppFixture appFixture;

        public TestBase(AppFixture appFixture)
        {
            this.appFixture = appFixture;
        }

        protected string FunctionsAppUri => $"{appFixture.TestApp.Host}:{appFixture.TestApp.Port}";
    }
}