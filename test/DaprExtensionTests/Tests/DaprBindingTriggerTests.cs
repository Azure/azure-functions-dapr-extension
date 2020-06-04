namespace DaprExtensionTests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Microsoft.Azure.WebJobs;
    using Dapr.AzureFunctions.Extension;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Xunit;
    using Xunit.Abstractions;

    public class DaprBindingTriggerTests : DaprTestBase
    {
        public DaprBindingTriggerTests(ITestOutputHelper output) 
            : base(output)
        {
            this.AddFunctions(typeof(Functions));
        }

        [Theory]
        [MemberData(nameof(GetTheoryDataInputs))]
        public async Task BindingTests_HttpClient(string methodName, object input)
        {
            using HttpResponseMessage response = await this.SendRequestAsync(
                HttpMethod.Post,
                $"http://localhost:3001/{methodName}",
                jsonContent: input);

            Assert.NotNull(response.Content);
            string result = await response.Content.ReadAsStringAsync();

            string serializedInput = JsonConvert.SerializeObject(input, Formatting.None);
            Assert.Equal(serializedInput, result);
        }

        [Fact]
        public async Task ExplicitMethodNameInAttribute()
        {
            string input = TriggerDataInput;
            using HttpResponseMessage response = await this.SendRequestAsync(
                HttpMethod.Post,
                $"http://localhost:3001/daprTriggerName",
                input);

            Assert.NotNull(response.Content);
            string result = await response.Content.ReadAsStringAsync();

            string serializedInput = JsonConvert.SerializeObject(input, Formatting.None);
            Assert.Equal(serializedInput, result);
        }

        // The Binding trigger handles the same data as the Service Invocation trigger does
        public static IEnumerable<object[]> GetTheoryDataInputs() => DaprServiceInvocationTriggerTests.GetTheoryDataInputs();

        private readonly static string TriggerDataInput = JsonConvert.SerializeObject(new
        {
            Metadata = new Dictionary<string, string>()
            {
                { "field1", "value1" },
                { "field2", "value2" }
            },
            Data = new CustomType()
            {
                P1 = "field1",
                P2 = 5,
                P3 = new DateTime(0)
            }
        });

        static class Functions
        {
            public static int ReturnInt([DaprBindingTrigger] int input) => input;

            public static bool ReturnBoolean([DaprBindingTrigger] bool input) => input;

            public static double ReturnDouble([DaprBindingTrigger] double input) => input;

            public static string ReturnString([DaprBindingTrigger] string input) => input;

            public static DateTime ReturnDateTime([DaprBindingTrigger] DateTime input) => input;

            public static Stream ReturnStream([DaprBindingTrigger] Stream input) => input;

            public static byte[] ReturnBytes([DaprBindingTrigger] byte[] input) => input;

            public static JObject ReturnJObject([DaprBindingTrigger] JObject input) => input;

            public static CustomType ReturnCustomType([DaprBindingTrigger] CustomType input) => input;

            public static object ReturnUnknownType([DaprBindingTrigger] object input) => input;

            public static object DotNetMethodName([DaprBindingTrigger(BindingName = "DaprTriggerName")] string input) => input;

            [FunctionName("Add")]
            public static string Sample([DaprServiceInvocationTrigger] JObject args, ILogger log)
            {
                log.LogInformation("C# processed a method request from the Dapr runtime");

                double result = (double)args["arg1"] + (double)args["arg2"];
                return result.ToString();
            }
        }

        class CustomType
        {
            public string? P1 { get; set; }
            public int P2 { get; set; }
            public DateTime P3 { get; set; }
        }
    }
}
