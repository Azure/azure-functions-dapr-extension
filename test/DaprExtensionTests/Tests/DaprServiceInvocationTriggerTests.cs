// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// ------------------------------------------------------------

namespace DaprExtensionTests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Dapr.AzureFunctions.Extension;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Xunit;
    using Xunit.Abstractions;

    public class DaprServiceInvocationTriggerTests : DaprTestBase
    {
        private static readonly IDictionary<string, string> EnvironmentVariables = new Dictionary<string, string>()
        {
            { "DaprMethodName", "MyBoundMethodName" }
        };

        public DaprServiceInvocationTriggerTests(ITestOutputHelper output)
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
            new object[] { nameof(Functions.ReturnJObject), new { arg1 = 2, arg2 = 3 } },
            new object[] { nameof(Functions.ReturnCustomType), new CustomType { P1 = "Hello, world", P2 = 3, P3 = DateTime.UtcNow } },
            new object[] { nameof(Functions.ReturnJObject), new { arg1 = 2, arg2 = 3 } },
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
            
            string serializedInput = JsonConvert.SerializeObject(input, Formatting.None);
            Assert.Equal(serializedInput, result);
        }

        [Fact]
        public async Task BindingTests_DaprState_SimpleValue()
        {
            string savedValue = Guid.NewGuid().ToString();
            this.SaveStateForUnitTetsing("store1", "key1", savedValue);

            using HttpResponseMessage response = await this.SendRequestAsync(
                HttpMethod.Post,
                "http://localhost:3001/getstate1",
                "key1");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Content);
            string resultJson = await response.Content.ReadAsStringAsync();

            string serializedValue = JsonConvert.SerializeObject(savedValue, Formatting.None);
            string result = (string)JsonConvert.DeserializeObject(resultJson);
            Assert.Equal(serializedValue, result);
        }


        [Fact]
        public async Task BindingTests_DaprState_ComplexValue()
        {
            string savedValue = Guid.NewGuid().ToString();
            this.SaveStateForUnitTetsing("store1", "key1", savedValue);

            using HttpResponseMessage response = await this.SendRequestAsync(
                HttpMethod.Post,
                "http://localhost:3001/getstate2",
                new { stateKey = "key1"});

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Content);
            string resultJson = await response.Content.ReadAsStringAsync();

            string serializedValue = JsonConvert.SerializeObject(savedValue, Formatting.None);
            string result = (string)JsonConvert.DeserializeObject(resultJson);
            Assert.Equal(serializedValue, result);
        }

        [Fact]
        public async Task ExplicitMethodNameInAttribute()
        {
            string input = "hello";
            using HttpResponseMessage response = await this.SendRequestAsync(
                HttpMethod.Post,
                $"http://localhost:3001/daprMethodName",
                input);

            Assert.NotNull(response.Content);
            string result = await response.Content.ReadAsStringAsync();

            string serializedInput = JsonConvert.SerializeObject(input, Formatting.None);
            Assert.Equal(serializedInput, result);
        }

        [Fact]
        public async Task MethodNameInAttributeBindingExpression()
        {
            string input = "hello";
            using HttpResponseMessage response = await this.SendRequestAsync(
                HttpMethod.Post,
                $"http://localhost:3001/myBoundMethodName",
                input);

            Assert.NotNull(response.Content);
            string result = await response.Content.ReadAsStringAsync();

            string serializedInput = JsonConvert.SerializeObject(input, Formatting.None);
            Assert.Equal(serializedInput, result);
        }

        // TODO: Error response tests

        [Fact]
        public async Task ValidateSample()
        {
            using HttpResponseMessage response = await this.SendRequestAsync(
                HttpMethod.Post,
                "http://localhost:3001/add",
                jsonContent: new { arg1 = 2, arg2 = 3 });

            Assert.NotNull(response.Content);
            string result = await response.Content.ReadAsStringAsync();
            Assert.Equal("\"5\"", result);
        }

        static class Functions
        {
            public static int ReturnInt([DaprServiceInvocationTrigger] int input) => input;

            public static double ReturnDouble([DaprServiceInvocationTrigger] double input) => input;

            public static bool ReturnBoolean([DaprServiceInvocationTrigger] bool input) => input;

            public static string ReturnString([DaprServiceInvocationTrigger] string input) => input;

            public static DateTime ReturnDateTime([DaprServiceInvocationTrigger] DateTime input) => input;

            public static Stream ReturnStream([DaprServiceInvocationTrigger] Stream input) => input;

            public static byte[] ReturnBytes([DaprServiceInvocationTrigger] byte[] input) => input;

            public static JObject ReturnJObject([DaprServiceInvocationTrigger] JObject input) => input;

            public static CustomType ReturnCustomType([DaprServiceInvocationTrigger] CustomType input) => input;

            public static object ReturnUnknownType([DaprServiceInvocationTrigger] object input) => input;

            public static object DotNetMethodName([DaprServiceInvocationTrigger(MethodName = "DaprMethodName")] string input) => input;

            public static object DotNetBindingExpression([DaprServiceInvocationTrigger(MethodName = "%DaprMethodName%")] string input) => input;

            [FunctionName("Add")]
            public static string Sample([DaprServiceInvocationTrigger] JObject args, ILogger log)
            {
                log.LogInformation("C# processed a method request from the Dapr runtime");

                double result = (double)args["arg1"] + (double)args["arg2"];
                return result.ToString();
            }

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
