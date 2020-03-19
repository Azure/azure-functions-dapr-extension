// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using DaprExtensionTests.Functions;
using Microsoft.Azure.WebJobs.Extensions.Dapr;
using Newtonsoft.Json;
using Xunit;
using Xunit.Abstractions;

namespace DaprExtensionTests
{
    public class DaprMethodTriggerTests : DaprExtensionTest
    {
        public DaprMethodTriggerTests(ITestOutputHelper output)
            : base(output)
        {
            this.AddFunctions(typeof(DaprMethodTriggerFunctions));
        }

        [Fact]
        public void DaprExtensionLoads()
        {
            // Use the log output to determine whether the extension loaded
            IEnumerable<string> logMessages = this.GetExtensionLogs().Select(entry => entry.Message);
            Assert.Contains("Registered dapr extension", logMessages, StringComparer.OrdinalIgnoreCase);
        }

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

        [Fact]
        public async Task BindToStream()
        {
            var input = new { arg1 = 2, arg2 = 3 };
            var inputJson = JsonConvert.SerializeObject(input, Formatting.None);

            using HttpResponseMessage response = await this.SendRequestAsync(
                HttpMethod.Post,
                "http://localhost:3001/echostream",
                jsonContent: input);

            Assert.NotNull(response.Content);
            string result = await response.Content.ReadAsStringAsync();
            Assert.Equal(inputJson, result);
        }

        [Fact]
        public async Task BindToBytes()
        {
            var input = new { arg1 = 2, arg2 = 3 };
            var inputJson = JsonConvert.SerializeObject(input, Formatting.None);

            using HttpResponseMessage response = await this.SendRequestAsync(
                HttpMethod.Post,
                "http://localhost:3001/echobytes",
                jsonContent: input);

            Assert.NotNull(response.Content);
            string result = await response.Content.ReadAsStringAsync();
            Assert.Equal(inputJson, result);
        }


        [Fact]
        public async Task BindToPOCO()
        {
            var input = new { P1 = "Hello, world", P2 = 3, P3 = DateTime.UtcNow };
            var inputJson = JsonConvert.SerializeObject(input, Formatting.None);

            using HttpResponseMessage response = await this.SendRequestAsync(
                HttpMethod.Post,
                "http://localhost:3001/echopoco",
                jsonContent: input);

            Assert.NotNull(response.Content);
            string result = await response.Content.ReadAsStringAsync();
            Assert.Equal(inputJson, result);
        }
    }

    // TODO: Refactor above tests into single test with parameters
}
