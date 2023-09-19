namespace DaprExtensionTests.Tests
{
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Microsoft.Azure.WebJobs.Extensions.Dapr;
    using Xunit;
    using Xunit.Abstractions;

    public class ExtensionUnsupportedHostingPlanTests : DaprTestBase
    {
        private static readonly IDictionary<string, string> EnvironmentVariables = new Dictionary<string, string>()
        {
            { Constants.EnvironmentKeys.AzureWebsiteInstanceId, "someValue" }
        };

        public ExtensionUnsupportedHostingPlanTests(ITestOutputHelper output)
            : base(output, EnvironmentVariables)
        {
            this.AddFunctions(typeof(Functions));
        }

        public static IEnumerable<object[]> GetTheoryDataInputs() => new List<object[]>
        {
            new object[] { nameof(Functions.ReturnInt), 42 }
        };

        [Theory]
        [MemberData(nameof(GetTheoryDataInputs))]
        public async Task ValidateDaprExtensionNotSupportedOnAppServiceHostingPlan(string methodName, object input)
        {
            await Assert.ThrowsAsync<HttpRequestException>(async () => await this.SendRequestAsync(
                HttpMethod.Post,
                $"http://localhost:3001/{methodName}",
                jsonContent: input));
        }
    }

    static class Functions
    {
        public static int ReturnInt([DaprBindingTrigger] int input) => input;
    }
}
