namespace EndToEndTests
{
    using Xunit;

    public class StateInputBindingTests : TestBase
    {
        [Theory]
        [MemberData(nameof(GetStateInputBindingTestData))]
        public async Task RetrieveData(string key, string expectedValue)
        {
            // Arrange
            var client = new HttpClient();
            var url = $"{base.FunctionsAppUri}/api/state/{key}";

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
                new string[] { "key1" },
                new string[] { "key2" },
                new string[] { "key3" },
            };
    }
}