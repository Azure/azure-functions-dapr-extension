namespace DaprExtensionTests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net.Http;
    using System.Text.Encodings.Web;
    using System.Text.Json;
    using System.Threading.Tasks;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extension.Dapr;
    using Microsoft.Extensions.Logging;
    using Xunit;
    using Xunit.Abstractions;

    public class DaprBindingTriggerTests : DaprTestBase
    {
        private static readonly JsonSerializerOptions SerializerOptions = new JsonSerializerOptions()
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        };

        private static readonly IDictionary<string, string> EnvironmentVariables = new Dictionary<string, string>()
        {
            { "BindingName", "MyBoundBindingName" }
        };

        public DaprBindingTriggerTests(ITestOutputHelper output)
            : base(output, EnvironmentVariables)
        {
            this.AddFunctions(typeof(Functions));
        }

        public static IEnumerable<object[]> GetTheoryDataInputs() => new List<object[]>
        {
            new object[] { nameof(Functions.ReturnInt), 42 },
            new object[] { nameof(Functions.ReturnBoolean), true },
            new object[] { nameof(Functions.ReturnDouble), Math.PI },
            new object[] { nameof(Functions.ReturnString), Guid.NewGuid().ToString() },
            new object[] { nameof(Functions.ReturnDateTime), DateTime.Now },
            new object[] { nameof(Functions.ReturnStream), Guid.NewGuid() }, // Any data works for Stream
            new object[] { nameof(Functions.ReturnBytes), Guid.NewGuid() }, // Any data works for bytes
            new object[] { nameof(Functions.ReturnJsonElement), new { arg1 = 2, arg2 = 3 } },
            new object[] { nameof(Functions.ReturnCustomType), new CustomType { P1 = "Hello, world", P2 = 3, P3 = DateTime.UtcNow } },
            new object[] { nameof(Functions.ReturnUnknownType), new { arg1 = 2, arg2 = 3 } },
        };

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

            string serializedInput = JsonSerializer.Serialize(input, SerializerOptions);
            Assert.Equal(serializedInput, result);
        }

        [Fact]
        public async Task ExplicitMethodNameInAttribute()
        {
            string input = TriggerDataInput;
            using HttpResponseMessage response = await this.SendRequestAsync(
                HttpMethod.Post,
                $"http://localhost:3001/daprBindingName",
                input);

            Assert.NotNull(response.Content);
            string result = await response.Content.ReadAsStringAsync();

            string serializedInput = JsonSerializer.Serialize(input, SerializerOptions);
            Assert.Equal(serializedInput, result);
        }

        [Fact]
        public async Task MethodNameInAttributeBindingExpression()
        {
            string input = TriggerDataInput;
            using HttpResponseMessage response = await this.SendRequestAsync(
                HttpMethod.Post,
                $"http://localhost:3001/myBoundBindingName",
                input);

            Assert.NotNull(response.Content);
            string result = await response.Content.ReadAsStringAsync();

            string serializedInput = JsonSerializer.Serialize(input, SerializerOptions);
            Assert.Equal(serializedInput, result);
        }

        private readonly static string TriggerDataInput = JsonSerializer.Serialize(new
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
        }, SerializerOptions);

        static class Functions
        {
            public static int ReturnInt([DaprBindingTrigger] int input) => input;

            public static bool ReturnBoolean([DaprBindingTrigger] bool input) => input;

            public static double ReturnDouble([DaprBindingTrigger] double input) => input;

            public static string ReturnString([DaprBindingTrigger] string input) => input;

            public static DateTime ReturnDateTime([DaprBindingTrigger] DateTime input) => input;

            public static Stream ReturnStream([DaprBindingTrigger] Stream input) => input;

            public static byte[] ReturnBytes([DaprBindingTrigger] byte[] input) => input;

            public static JsonElement ReturnJsonElement([DaprBindingTrigger] JsonElement input) => input;

            public static CustomType ReturnCustomType([DaprBindingTrigger] CustomType input) => input;

            public static object ReturnUnknownType([DaprBindingTrigger] object input) => input;

            public static object DotNetMethodName([DaprBindingTrigger(BindingName = "DaprBindingName")] string input) => input;

            public static object DotNetBindingExpression([DaprBindingTrigger(BindingName = "%BindingName%")] string input) => input;

            [FunctionName("Add")]
            public static string Sample([DaprServiceInvocationTrigger] JsonElement args, ILogger log)
            {
                log.LogInformation("C# processed a method request from the Dapr runtime");
                if (!args.TryGetProperty("arg1", out JsonElement arg1))
                {
                    throw new ArgumentException("Missing arg1");
                }
                if (!args.TryGetProperty("arg2", out JsonElement arg2))
                {
                    throw new ArgumentException("Missing arg2");
                }
                double result = arg1.GetDouble() + arg2.GetDouble();
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
