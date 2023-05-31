namespace EndToEndTests
{
    using EndToEndTests.Tester;
    using Xunit;

    [Collection("AppCollection")]
    public class StateInputBindingTests
    {
        AppFixture appFixture;

        public StateInputBindingTests(AppFixture appFixture)
        {
            this.appFixture = appFixture;
        }

        private string FunctionsAppUri => $"{appFixture.TestApp.Host}:{appFixture.TestApp.Port}";

        [Theory]
        [MemberData(nameof(GetStateInputBindingTestData))]
        public async Task RetrieveData(string key, string expectedValue)
        {
            // Arrange
            var client = new HttpClient();
            var url = $"{FunctionsAppUri}/api/state/{key}";

            // Act
            var response = await client.GetAsync(url);

            // Assert
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();

            Assert.Equal(expectedValue, responseString);
        }

        public static IEnumerable<string[]> GetStateInputBindingTestData() =>
            new List<string[]>
            {
                new string[] { "key1", "\"value1\"" },
            };
    }
}