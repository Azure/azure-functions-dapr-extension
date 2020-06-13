// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// ------------------------------------------------------------

namespace DaprExtensionTests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using CloudNative.CloudEvents;
    using Microsoft.Azure.WebJobs;
    using Dapr.AzureFunctions.Extension;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Xunit;
    using Xunit.Abstractions;

    public class DaprTopicTriggerTests : DaprTestBase
    {
        public DaprTopicTriggerTests(ITestOutputHelper output)
            : base(output)
        {
            this.AddFunctions(typeof(Functions));
        }

        [Fact]
        public async Task ExplicitTopicNameInAttribute()
        {
            int input = 42;

            // The method name is DotNetMethodName
            // The function name is FunctionName
            // The topic name is MyTopic
            using HttpResponseMessage response = await this.SendRequestAsync(
                HttpMethod.Post,
                "http://localhost:3001/MyTopic",
                jsonContent: CreateCloudEventMessage(input));

            Assert.Equal(0, response.Content.Headers.ContentLength);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            IEnumerable<string> functionLogs = this.GetFunctionLogs("MyFunctionName");
            Assert.Contains(input.ToString(), functionLogs);
        }

        [Fact]
        public async Task InvokeSubscriptionEndpoint()
        {
            using HttpResponseMessage response = await this.SendRequestAsync(
                HttpMethod.Get,
                "http://localhost:3001/dapr/subscribe");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("application/json", response.Content.Headers.ContentType?.ToString());
            
            JToken result =  JToken.Parse(await response.Content.ReadAsStringAsync());
            Assert.Equal(JTokenType.Array, result.Type);

            JArray array = Assert.IsType<JArray>(result);
            Assert.NotEmpty(array);

            // verify the topic name is correctly registered
            IEnumerable<string> topics = array.Select(item => (JObject)item).Select(obj => (string)obj.GetValue("topic"));          
            Assert.Contains(nameof(Functions.IntTopic), topics);
            Assert.Contains(nameof(Functions.CustomTypeTopic), topics);
            Assert.Contains(nameof(Functions.StringTopic), topics);
            Assert.Contains(nameof(Functions.StreamTopic), topics);
            Assert.Contains(nameof(Functions.BytesTopic), topics);
            Assert.Contains(nameof(Functions.JObjectTopic), topics);
            Assert.Contains(nameof(Functions.CloudEventTopic), topics);

            // verify the route is correctly registered
            // Version 0.8 only support route to match topic name
            IEnumerable<string> routes = array.Select(item => (JObject)item).Select(obj => (string)obj.GetValue("route"));
            Assert.Contains(nameof(Functions.IntTopic), routes);
            Assert.Contains(nameof(Functions.CustomTypeTopic), routes);
            Assert.Contains(nameof(Functions.StringTopic), routes);
            Assert.Contains(nameof(Functions.StreamTopic), routes);
            Assert.Contains(nameof(Functions.BytesTopic), routes);
            Assert.Contains(nameof(Functions.JObjectTopic), routes);
            Assert.Contains(nameof(Functions.CloudEventTopic), routes);

            // Make sure the explicit topic names are handled correctly
            Assert.Contains("MyTopic", topics);
            Assert.DoesNotContain(nameof(Functions.DotNetMethodName), topics);
            Assert.DoesNotContain("MyFunctionName", topics);
        }

        [Theory]
        [MemberData(nameof(GetTheoryDataInputs))]
        public async Task PublishDataFormats(string topicName, object input, bool expectEnvelope)
        {
            object cloudEventInput = CreateCloudEventMessage(input);
            using HttpResponseMessage response = await this.SendRequestAsync(
                HttpMethod.Post,
                $"http://localhost:3001/{topicName}",
                jsonContent: cloudEventInput);

            Assert.Equal(0, response.Content.Headers.ContentLength);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            string expectedOutput = expectEnvelope ? 
                JsonConvert.SerializeObject(cloudEventInput) :
                JsonConvert.SerializeObject(input);

            IEnumerable<string> functionLogs = this.GetFunctionLogs(topicName);
            Assert.Contains(expectedOutput, functionLogs);
        }

        public static IEnumerable<object[]> GetTheoryDataInputs() => new List<object[]>
        {
            new object[] { nameof(Functions.IntTopic), 42, false },
            new object[] { nameof(Functions.CustomTypeTopic), new CustomType { P1 = "hi", P2 = 42, P3 = DateTime.Now }, false },
            new object[] { nameof(Functions.StringTopic), Guid.NewGuid().ToString(), true },
            new object[] { nameof(Functions.StreamTopic), Guid.NewGuid(), true }, // Any data works for Stream
            new object[] { nameof(Functions.BytesTopic), Guid.NewGuid(), true }, // Any data works for bytes
            new object[] { nameof(Functions.JObjectTopic), new { arg1 = 2, arg2 = 3 }, true },
            new object[] { nameof(Functions.CloudEventTopic), "<much wow=\"xml\"/>", false }, // The test just logs the data payload
        };

        static object CreateCloudEventMessage(object payload)
        {
            // https://github.com/cloudevents/spec/blob/v1.0/spec.md#example
            var message = new
            {
                id = Guid.NewGuid().ToString("N"),
                specversion = "1.0",
                type = "io.dapr.functions.testing",
                source = "urn:uuid:6e8bc430-9c3a-11d9-9669-0800200c9a67",
                time = DateTime.UtcNow,
                data = payload,
            };

            return message;
        }

        static class Functions
        {
            public static void IntTopic(
                [DaprTopicTrigger] int input,
                ILogger log) => log.LogInformation(input.ToString());

            public static void StringTopic(
                [DaprTopicTrigger] string input,
                ILogger log) => log.LogInformation(input);

            public static async Task StreamTopic(
                [DaprTopicTrigger] Stream input,
                ILogger log) => log.LogInformation(await new StreamReader(input).ReadToEndAsync());

            public static void BytesTopic(
                [DaprTopicTrigger] byte[] input,
                ILogger log) => log.LogInformation(Encoding.UTF8.GetString(input));

            public static void JObjectTopic(
                [DaprTopicTrigger] JObject input,
                ILogger log) => log.LogInformation(input.ToString(Formatting.None));

            public static void CloudEventTopic(
                [DaprTopicTrigger] CloudEvent input,
                ILogger log) => log.LogInformation(JsonConvert.SerializeObject(input.Data));

            public static void CustomTypeTopic(
                [DaprTopicTrigger] CustomType input,
                ILogger log) => log.LogInformation(JsonConvert.SerializeObject(input));

            [FunctionName("MyFunctionName")]
            public static void DotNetMethodName(
                [DaprTopicTrigger(Topic = "MyTopic")] int input,
                ILogger log) => log.LogInformation(input.ToString());
        }

        class CustomType
        {
            public string? P1 { get; set; }
            public int P2 { get; set; }
            public DateTime P3 { get; set; }
        }
    }
}
