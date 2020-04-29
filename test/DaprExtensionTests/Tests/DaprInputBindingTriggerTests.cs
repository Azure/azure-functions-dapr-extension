namespace DaprExtensionTests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.Dapr;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Xunit;
    using Xunit.Abstractions;

    public class DaprInputBindingTriggerTests : DaprTestBase
    {
        public DaprInputBindingTriggerTests(ITestOutputHelper output) 
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

        public static IEnumerable<object[]> GetTheoryDataInputs() => new List<object[]>
        {
            new object[] { nameof(Functions.ReturnInt), 42 },
            new object[] { nameof(Functions.ReturnBoolean), true },
            new object[] { nameof(Functions.ReturnDouble), Math.PI },
            new object[] { nameof(Functions.ReturnString), Guid.NewGuid().ToString() },
            new object[] { nameof(Functions.ReturnDateTime), DateTime.Now },
            new object[] { nameof(Functions.ReturnStream), Guid.NewGuid() }, // Any data works for Stream
            new object[] { nameof(Functions.ReturnBytes), Guid.NewGuid() }, // Any data works for bytes
            new object[] { nameof(Functions.ReturnJObject), new { arg1 = 2, arg2 = 3 } },
            new object[] { nameof(Functions.ReturnCustomType), new CustomType { P1 = "Hello, world", P2 = 3, P3 = DateTime.UtcNow } },
            new object[] { nameof(Functions.ReturnJObject), new { arg1 = 2, arg2 = 3 } },
        };

        static class Functions
        {
            public static int ReturnInt([DaprInputBindingTrigger] int input) => input;

            public static bool ReturnBoolean([DaprInputBindingTrigger] bool input) => input;

            public static double ReturnDouble([DaprInputBindingTrigger] double input) => input;

            public static string ReturnString([DaprInputBindingTrigger] string input) => input;

            public static DateTime ReturnDateTime([DaprInputBindingTrigger] DateTime input) => input;

            public static Stream ReturnStream([DaprInputBindingTrigger] Stream input) => input;

            public static byte[] ReturnBytes([DaprInputBindingTrigger] byte[] input) => input;

            public static JObject ReturnJObject([DaprInputBindingTrigger] JObject input) => input;

            public static CustomType ReturnCustomType([DaprInputBindingTrigger] CustomType input) => input;

            public static object ReturnUnknownType([DaprInputBindingTrigger] object input) => input;

            public static object DotNetMethodName([DaprInputBindingTrigger(TriggerName = "DaprTriggerName")] string input) => input;

            [FunctionName("Add")]
            public static string Sample([DaprServiceInvocationTrigger] JObject args, ILogger log)
            {
                log.LogInformation("C# processed a method request from the Dapr runtime");

                double result = (double)args["arg1"] + (double)args["arg2"];
                return result.ToString();
            }

            // TODO: Write tests for these
            [FunctionName(nameof(GetState1))]
            public static string GetState1(
                [DaprServiceInvocationTrigger] string stateKey,
                [DaprState("store1", Key = "{stateKey}")] string existingState)
            {
                return existingState;
            }

            [FunctionName(nameof(GetState2))]
            public static string GetState2(
                [DaprServiceInvocationTrigger] JObject input,
                [DaprState("store1", Key = "{input.stateKey}")] string existingState)
            {
                // TODO: Not sure yet if this binding expression will work - needs testing.
                return existingState;
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
