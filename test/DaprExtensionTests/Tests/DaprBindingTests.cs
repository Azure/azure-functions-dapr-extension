// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// ------------------------------------------------------------

namespace DaprExtensionTests
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Azure.WebJobs;
    using Dapr.AzureFunctions.Extension;
    using Microsoft.Azure.WebJobs.Host;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Xunit;
    using Xunit.Abstractions;

    public class DaprBindingTests : DaprTestBase
    {
        public DaprBindingTests(ITestOutputHelper output)
            : base(output)
        {
            this.AddFunctions(typeof(Functions));
        }

        [Theory]
        [MemberData(nameof(GetObjectAsyncCollectorInputs))]
        public async Task SendMessage_ObjectAsyncCollector(object input)
        {
            await this.CallFunctionAsync(nameof(Functions.ObjectAsyncCollector), "input", input);
            SavedHttpRequest req = this.GetSingleSendMessgaeRequest();

            JObject expectedPayload = JObject.Parse(
                $@"{{
                        ""data"": {JsonConvert.SerializeObject(input)},
                        ""metadata"": {{
                            ""key"": ""myKey""
                        }}
                   }}");

            Assert.Equal("/v1.0/bindings/myBinding", req.Path);
            Assert.Equal(JsonConvert.SerializeObject(expectedPayload), req.ContentAsString);
        }

        [Theory]
        [MemberData(nameof(GetObjectAsyncCollectorInputs))]
        public async Task SendMessage_JObjectAsyncCollector(object message)
        {
            JObject input = JObject.Parse(
                $@"{{
                        ""data"": {JsonConvert.SerializeObject(message)},
                        ""metadata"": {{
                            ""key"": ""myKey""
                        }},
                        ""bindingName"": ""myBinding"",
                   }}");

            JObject expectedPayload = JObject.Parse(
                $@"{{
                        ""data"": {JsonConvert.SerializeObject(message)},
                        ""metadata"": {{
                            ""key"": ""myKey""
                        }}
                   }}");

            await this.CallFunctionAsync(nameof(Functions.JObjectAsyncCollector), "input", input);
            SavedHttpRequest req = this.GetSingleSendMessgaeRequest();

            Assert.Equal("/v1.0/bindings/myBinding", req.Path);
            Assert.Equal(JsonConvert.SerializeObject(expectedPayload), req.ContentAsString);
        }

        [Theory]
        [MemberData(nameof(GetObjectAsyncCollectorInputs))]
        public async Task SendMessage_OutputParameter(object inputMessage)
        {          
            await this.CallFunctionAsync(nameof(Functions.ObjectOutputParameter), "input", inputMessage);
            SavedHttpRequest req = this.GetSingleSendMessgaeRequest();

            JObject expectedPayload = JObject.Parse(
                $@"{{
                       ""data"": {JsonConvert.SerializeObject(inputMessage)}
                   }}");

            Assert.Equal("/v1.0/bindings/myBinding", req.Path);
            Assert.Equal(JsonConvert.SerializeObject(expectedPayload), req.ContentAsString);
        }

        [Fact]
        public async Task SendMessage_ReturnValue()
        {
            var input = new DaprBindingMessage("hello", new Dictionary<string, object> { { "key", "myKey" } }, "myBinding" );

            await this.CallFunctionAsync(nameof(Functions.DaprConnectorReturnValueAnyMessage), "input", input);
            SavedHttpRequest req = this.GetSingleSendMessgaeRequest();

            JObject expectedPayload = JObject.Parse($@"{{""data"": ""hello"", ""metadata"": {{""key"": ""myKey""}}}}");
            
            Assert.Equal("/v1.0/bindings/myBinding", req.Path);
            Assert.Equal(JsonConvert.SerializeObject(expectedPayload), req.ContentAsString);
        }

        [Fact]
        public async Task SendMessage_NoBindingNameSpecified()
        {
            // No binding name is specified in the attribute or in the message
            var input = new DaprBindingMessage("Hello, world!", new Dictionary<string, object>{ { "key", "myKey" } });
            FunctionInvocationException error = await Assert.ThrowsAsync<FunctionInvocationException>(() =>
                this.CallFunctionAsync(nameof(Functions.DaprConnectorReturnValueAnyMessage), "input", input));

            // The exception message should reflect the fact that no binding name was specified
            ArgumentException innerError = Assert.IsType<ArgumentException>(error.GetBaseException());
            Assert.Contains("A non-null binding name must be specified", innerError.Message);

            // No requests should have been sent
            Assert.Empty(this.GetDaprRequests());
        }

        [Fact]
        public async Task SendMessage_MultipleEvents()
        {
            await this.CallFunctionAsync(nameof(Functions.AsyncCollectorMultipleItems), "input", null);

            // Two events are published, each to a separate topic
            SavedHttpRequest[] requests = this.GetDaprRequests();
            Assert.Equal(2, requests.Length);
            Assert.All(requests, req => Assert.Equal("POST", req.Method));
            Assert.All(requests, req => Assert.StartsWith("application/json", req.ContentType));

            JObject expectedPayload1 = JObject.Parse($@"{{""data"": 1, ""metadata"": {{""key"": ""myKey""}}}}");
            JObject expectedPayload2 = JObject.Parse($@"{{""data"": 2, ""metadata"": {{""key"": ""myKey""}}}}");

            // The order of the requests is not guaranteed
            SavedHttpRequest req1 = Assert.Single(requests, req => req.Path == "/v1.0/bindings/myBinding1");
            Assert.Equal(JsonConvert.SerializeObject(expectedPayload1), req1.ContentAsString);

            SavedHttpRequest req2 = Assert.Single(requests, req => req.Path == "/v1.0/bindings/myBinding2");
            Assert.Equal(JsonConvert.SerializeObject(expectedPayload2), req2.ContentAsString);
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

        SavedHttpRequest GetSingleSendMessgaeRequest()
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
                [DaprBinding(BindingName = "myBinding")] IAsyncCollector<DaprBindingMessage> events)
            {
                return events.AddAsync(new DaprBindingMessage(input, new Dictionary<string, object> { { "key", "myKey" } }));
            }

            [NoAutomaticTrigger]
            public static Task JObjectAsyncCollector(
                JObject input,
                [DaprBinding] IAsyncCollector<JObject> events) => events.AddAsync(input);

            [NoAutomaticTrigger]
            public static void ObjectOutputParameter(
                object input,
               [DaprBinding(BindingName = "myBinding")] out object eventData) => eventData = input;

            [NoAutomaticTrigger]
            [return: DaprBinding]
            public static DaprBindingMessage DaprConnectorReturnValueAnyMessage(DaprBindingMessage input) => input;

            [NoAutomaticTrigger]
            public static async Task AsyncCollectorMultipleItems(
                [DaprBinding(BindingName = "myBinding")] IAsyncCollector<DaprBindingMessage> events)
            {
                await events.AddAsync(new DaprBindingMessage(1, new Dictionary<string, object> { { "key", "myKey" } }, "myBinding1"));
                await events.AddAsync(new DaprBindingMessage(2, new Dictionary<string, object> { { "key", "myKey" } }, "myBinding2"));
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