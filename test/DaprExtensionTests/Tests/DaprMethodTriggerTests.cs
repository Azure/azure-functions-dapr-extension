// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

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

    public class DaprMethodTriggerTests : DaprTestBase
    {
        public DaprMethodTriggerTests(ITestOutputHelper output)
            : base(output)
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
            public static int ReturnInt([DaprMethodTrigger] int input) => input;

            public static double ReturnDouble([DaprMethodTrigger] double input) => input;

            public static bool ReturnBoolean([DaprMethodTrigger] bool input) => input;

            public static string ReturnString([DaprMethodTrigger] string input) => input;

            public static DateTime ReturnDateTime([DaprMethodTrigger] DateTime input) => input;

            public static Stream ReturnStream([DaprMethodTrigger] Stream input) => input;

            public static byte[] ReturnBytes([DaprMethodTrigger] byte[] input) => input;

            public static JObject ReturnJObject([DaprMethodTrigger] JObject input) => input;

            public static CustomType ReturnCustomType([DaprMethodTrigger] CustomType input) => input;

            public static object ReturnUnknownType([DaprMethodTrigger] object input) => input;

            public static object DotNetMethodName([DaprMethodTrigger(MethodName = "DaprMethodName")] string input) => input;

            [FunctionName("Add")]
            public static string Sample([DaprMethodTrigger] JObject args, ILogger log)
            {
                log.LogInformation("C# processed a method request from the Dapr runtime");

                double result = (double)args["arg1"] + (double)args["arg2"];
                return result.ToString();
            }

            // TODO: Write tests for these
            [FunctionName(nameof(GetState1))]
            public static string GetState1(
                [DaprMethodTrigger] string stateKey,
                [DaprState("store1", Key = "{stateKey}")] string existingState)
            {
                return existingState;
            }

            [FunctionName(nameof(GetState2))]
            public static string GetState2(
                [DaprMethodTrigger] JObject input,
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
