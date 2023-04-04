// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// ------------------------------------------------------------

namespace DaprExtensionTests
{
    using System;
    using System.Collections.Generic;
    using System.Text.Json;
    using System.Threading.Tasks;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extension.Dapr;
    using Microsoft.Azure.WebJobs.Host;
    using Xunit;
    using Xunit.Abstractions;

    public class DaprTopicPublishTests : DaprTestBase
    {
        private static readonly IDictionary<string, string> EnvironmentVariables = new Dictionary<string, string>()
        {
            { "PubSubName", "MyBoundPubSub" },
            { "TopicName", "MyBoundTopic" },
        };

        public DaprTopicPublishTests(ITestOutputHelper output)
            : base(output, EnvironmentVariables)
        {
            this.AddFunctions(typeof(Functions));
        }

        [Theory]
        [MemberData(nameof(GetObjectAsyncCollectorInputs))]
        public async Task Publish_ObjectAsyncCollector(object input)
        {
            await this.CallFunctionAsync(nameof(Functions.ObjectAsyncCollector), "input", input);
            SavedHttpRequest req = this.GetSinglePublishRequest();

            string expectedValue = JsonSerializer.Serialize(input);
            Assert.Equal("/v1.0/publish/MyPubSub/TopicA", req.Path);
            Assert.Equal(expectedValue, req.ContentAsString);
        }

        [Theory]
        [MemberData(nameof(GetObjectAsyncCollectorInputs))]
        public async Task Publish_OutputParameter(object input)
        {
            await this.CallFunctionAsync(nameof(Functions.ObjectOutputParameter), "input", input);
            SavedHttpRequest req = this.GetSinglePublishRequest();

            string expectedValue = JsonSerializer.Serialize(input);
            Assert.Equal("/v1.0/publish/MyPubSub/TopicA", req.Path);
            Assert.Equal(expectedValue, req.ContentAsString);
        }

        [Fact]
        public async Task Publish_ReturnValue()
        {
            var input = new DaprPubSubEvent("Hello, world!", pubSubName: "MyPubSub", topic: "TopicB");

            await this.CallFunctionAsync(nameof(Functions.DaprPubSubEventReturnValueAnyTopic), "input", input);
            SavedHttpRequest req = this.GetSinglePublishRequest();

            string expectedValue = JsonSerializer.Serialize(input.Payload);
            Assert.Equal($"/v1.0/publish/MyPubSub/{input.Topic}", req.Path);
            Assert.Equal(expectedValue, req.ContentAsString);
        }

        [Fact]
        public async Task Publish_ReturnValue_Bound()
        {
            var input = new DaprPubSubEvent("Hello, world!");

            await this.CallFunctionAsync(nameof(Functions.DaprPubSubEventReturnValueBound), "input", input);
            SavedHttpRequest req = this.GetSinglePublishRequest();

            string expectedValue = JsonSerializer.Serialize(input.Payload);
            Assert.Equal($"/v1.0/publish/MyBoundPubSub/MyBoundTopic", req.Path);
            Assert.Equal(expectedValue, req.ContentAsString);
        }

        [Fact]
        public async Task Publish_NoTopicSpecified()
        {
            // No topic is specified
            var input = new DaprPubSubEvent("Hello, world!");
            FunctionInvocationException error = await Assert.ThrowsAsync<FunctionInvocationException>(() =>
                this.CallFunctionAsync(nameof(Functions.DaprPubSubEventReturnValueAnyTopic), "input", input));

            // The exception message should reflect the fact that no topic was specified
            ArgumentException innerError = Assert.IsType<ArgumentException>(error.GetBaseException());
            Assert.Contains("No topic", innerError.Message);

            // No requests should have been sent
            Assert.Empty(this.GetDaprRequests());
        }

        [Fact]
        public async Task Publish_MultipleEvents()
        {
            await this.CallFunctionAsync(nameof(Functions.AsyncCollectorMultipleItems), "input", null);

            // Two events are published, each to a separate topic
            SavedHttpRequest[] requests = this.GetDaprRequests();
            Assert.Equal(2, requests.Length);
            Assert.All(requests, req => Assert.Equal("POST", req.Method));
            Assert.All(requests, req => Assert.StartsWith("application/json", req.ContentType));

            // The order of the requests is not guaranteed
            SavedHttpRequest req1 = Assert.Single(requests, req => req.Path == "/v1.0/publish/MyPubSub/TopicA");
            Assert.Equal("1", req1.ContentAsString);

            SavedHttpRequest req2 = Assert.Single(requests, req => req.Path == "/v1.0/publish/MyPubSub/TopicB");
            Assert.Equal("2", req2.ContentAsString);
        }

        public static IEnumerable<object[]> GetObjectAsyncCollectorInputs() => new List<object[]>
        {
            new object[] { 42 },
            new object[] { true },
            new object[] { Math.PI },
            new object[] { "Hello, world!" },
            new object[] { DateTime.Now },
            new object[] { Guid.NewGuid() },
            new object[] { Guid.NewGuid().ToByteArray() },
            new object[] { new { arg1 = 2, arg2 = 3 } },
            new object[] { new UserDefinedType { P1 = "Hello, world!", P2 = 3, P3 = DateTime.UtcNow } },
        };

        SavedHttpRequest GetSinglePublishRequest()
        {
            SavedHttpRequest[] requests = this.GetDaprRequests();
            SavedHttpRequest req = Assert.Single(requests);
            Assert.StartsWith("application/json", req.ContentType);
            Assert.Equal("POST", req.Method);
            return req;
        }

        static class Functions
        {
            [NoAutomaticTrigger]
            public static Task ObjectAsyncCollector(
                object input,
                [DaprPublish(PubSubName = "MyPubSub", Topic = "TopicA")] IAsyncCollector<object> events)
            {
                // Any JSON-serializeable type should be supported
                return events.AddAsync(input);
            }

            [NoAutomaticTrigger]
            public static void ObjectOutputParameter(
                object input,
                [DaprPublish(PubSubName = "MyPubSub", Topic = "TopicA")] out object eventData) => eventData = input;

            [NoAutomaticTrigger]
            [return: DaprPublish(PubSubName = "MyPubSub")]
            public static DaprPubSubEvent DaprPubSubEventReturnValueAnyTopic(DaprPubSubEvent input) => input;

            [NoAutomaticTrigger]
            [return: DaprPublish(PubSubName = "%PubSubName%", Topic = "%TopicName%")]
            public static DaprPubSubEvent DaprPubSubEventReturnValueBound(DaprPubSubEvent input) => input;

            [NoAutomaticTrigger]
            public static async Task AsyncCollectorMultipleItems(
                object input,
                [DaprPublish(PubSubName = "MyPubSub", Topic = "TopicA")] IAsyncCollector<DaprPubSubEvent> events)
            {
                await events.AddAsync(new DaprPubSubEvent(1));
                await events.AddAsync(new DaprPubSubEvent(2, topic: "TopicB"));
            }
        }

        class UserDefinedType
        {
            public string? P1 { get; set; }
            public int P2 { get; set; }
            public DateTime P3 { get; set; }
        }
    }
}
