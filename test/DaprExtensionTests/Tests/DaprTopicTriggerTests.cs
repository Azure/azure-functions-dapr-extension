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
    using System.Text.Json;
    using System.Threading.Tasks;
    using CloudNative.CloudEvents;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extension.Dapr;
    using Microsoft.Extensions.Logging;
    using Xunit;
    using Xunit.Abstractions;

    public class DaprTopicTriggerTests : DaprTestBase
    {
        private static readonly IDictionary<string, string> EnvironmentVariables = new Dictionary<string, string>()
        {
            { "PubSubName", "MyBoundPubSub" },
            { "TopicName", "MyBoundTopic" }
        };

        public DaprTopicTriggerTests(ITestOutputHelper output)
            : base(output, EnvironmentVariables)
        {
            this.AddFunctions(typeof(Functions));
        }

        [Fact]
        public async Task ExplicitTopicNameInAttribute()
        {
            int input = 42;

            // The method name is ExplicitTopicNameInAttribute
            // The function name is FunctionName
            // The topic name is MyRoute
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
        public async Task ExplicitRouteNameInAttribute()
        {
            int input = 42;

            // The method name is ExplicitRouteInAttribute
            // The function name is MyOtherFunctionName
            // The topic name is MyOtherTopic
            // The route is MyRoute
            using HttpResponseMessage response = await this.SendRequestAsync(
                HttpMethod.Post,
                "http://localhost:3001/MyRoute",
                jsonContent: CreateCloudEventMessage(input));

            Assert.Equal(0, response.Content.Headers.ContentLength);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            IEnumerable<string> functionLogs = this.GetFunctionLogs("MyOtherFunctionName");
            Assert.Contains(input.ToString(), functionLogs);
        }

        [Fact]
        public async Task TopicNameInAttributeAsBindingExpression()
        {
            int input = 42;

            // The method name is DotNetBindingResolution
            // The function name is DotNetBindingResolution
            // The topic name is MyBoundTopic
            // The route is MyBoundTopic
            using HttpResponseMessage response = await this.SendRequestAsync(
                HttpMethod.Post,
                "http://localhost:3001/MyBoundTopic",
                jsonContent: CreateCloudEventMessage(input));

            Assert.Equal(0, response.Content.Headers.ContentLength);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            IEnumerable<string> functionLogs = this.GetFunctionLogs(nameof(Functions.DotNetBindingResolution));
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

            JsonElement result = JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;
            Assert.Equal(JsonValueKind.Array, result.ValueKind);

            var array = result.EnumerateArray().ToArray();
            Assert.NotEmpty(array);

            var subscriptions = array
                .Select(obj => (
                    pubSubname: obj.GetProperty("pubsubname").GetString() ?? string.Empty,
                    topic: obj.GetProperty("topic").GetString() ?? string.Empty,
                    route: obj.GetProperty("route").GetString() ?? string.Empty))
                .OrderBy(t => t.topic)
                .ToArray();

            Assert.Collection(
                subscriptions,
                s =>
                {
                    // This one has a custom configuration
                    Assert.Equal("MyOtherPubSub", s.pubSubname);
                    Assert.Equal("AnotherTopic", s.topic);
                    Assert.Equal("/MyRoute", s.route);
                },
                s => AssertDefaults(s, nameof(Functions.BytesTopic)),
                s => AssertDefaults(s, nameof(Functions.CloudEventTopic)),
                s => AssertDefaults(s, nameof(Functions.CustomTypeTopic)),
                s => AssertDefaults(s, nameof(Functions.IntTopic)),
                s => AssertDefaults(s, nameof(Functions.JsonElementTopic)),
                s =>
                {
                    // This one has a custom configuration with env-vars
                    Assert.Equal("MyBoundPubSub", s.pubSubname);
                    Assert.Equal("MyBoundTopic", s.topic);
                    Assert.Equal("/MyBoundTopic", s.route);
                },
                s =>
                {
                    // This one has a custom configuration
                    Assert.Equal("MyOtherPubSub", s.pubSubname);
                    Assert.Equal("MyTopic", s.topic);
                    Assert.Equal("/MyTopic", s.route);
                },
                s => AssertDefaults(s, nameof(Functions.StreamTopic)),
                s => AssertDefaults(s, nameof(Functions.StringTopic)));

            Assert.DoesNotContain(nameof(Functions.ExplicitTopicNameInAttribute), subscriptions.Select(s => s.topic));
            Assert.DoesNotContain("MyFunctionName", subscriptions.Select(s => s.topic));

            void AssertDefaults((string pubSubname, string topic, string route) s, string methodName)
            {
                // by default the method name is the topic and the route
                Assert.Equal("MyPubSub", s.pubSubname);
                Assert.Equal(methodName, s.topic);
                Assert.Equal("/" + methodName, s.route);
            }
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
                JsonSerializer.Serialize(cloudEventInput) :
                JsonSerializer.Serialize(input);

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
            new object[] { nameof(Functions.JsonElementTopic), new { arg1 = 2, arg2 = 3 }, true },
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
                [DaprTopicTrigger("MyPubSub")] int input,
                ILogger log) => log.LogInformation(input.ToString());

            public static void StringTopic(
                [DaprTopicTrigger("MyPubSub")] string input,
                ILogger log) => log.LogInformation(input);

            public static async Task StreamTopic(
                [DaprTopicTrigger("MyPubSub")] Stream input,
                ILogger log) => log.LogInformation(await new StreamReader(input).ReadToEndAsync());

            public static void BytesTopic(
                [DaprTopicTrigger("MyPubSub")] byte[] input,
                ILogger log) => log.LogInformation(Encoding.UTF8.GetString(input));

            public static void JsonElementTopic(
                [DaprTopicTrigger("MyPubSub")] JsonElement input,
                ILogger log) => log.LogInformation(JsonSerializer.Serialize(input));

            public static void CloudEventTopic(
                [DaprTopicTrigger("MyPubSub")] CloudEvent input,
                ILogger log) => log.LogInformation(JsonSerializer.Serialize(input.Data));

            public static void CustomTypeTopic(
                [DaprTopicTrigger("MyPubSub")] CustomType input,
                ILogger log) => log.LogInformation(JsonSerializer.Serialize(input));

            [FunctionName("MyFunctionName")]
            public static void ExplicitTopicNameInAttribute(
                [DaprTopicTrigger("MyOtherPubSub", Topic = "MyTopic")] int input,
                ILogger log) => log.LogInformation(input.ToString());

            [FunctionName("MyOtherFunctionName")]
            public static void ExplicitRouteInAttribute(
                [DaprTopicTrigger("MyOtherPubSub", Topic = "AnotherTopic", Route = "MyRoute")] int input,
                ILogger log) => log.LogInformation(input.ToString());

            public static void DotNetBindingResolution(
                [DaprTopicTrigger("%PubSubName%", Topic = "%TopicName%")] int input,
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
