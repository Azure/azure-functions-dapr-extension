// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace DaprExtensionTests
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.Dapr;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Xunit;
    using Xunit.Abstractions;
    using static DaprExtensionTests.DaprStateBindingTests;

    public class DaprActorStateBindingTests : DaprTestBase
    {
        public DaprActorStateBindingTests(ITestOutputHelper output)
            : base(output)
        {
            this.AddFunctions(typeof(Functions));
        }

        public static readonly JObject testStoredState = JObject.Parse(@"{""PropertyA"": ""ValueA"", ""PropertyB"": ""ValueB""}");

        [Fact]
        public async Task GetActorState_ExplicitSettings()
        {
            await this.CallFunctionAsync(nameof(Functions.GetActorState_ExplicitSettings));

            SavedHttpRequest req = this.GetActorStateRequest();
            Assert.Equal("/v1.0/actors/myActor/abc/state/key1", req.Path);
            
            IEnumerable<string> functionLogs = this.GetFunctionLogs(nameof(Functions.GetActorState_ExplicitSettings));
            Assert.Contains(testStoredState.ToString(), functionLogs);
        }

        [Fact]
        public async Task GetActorState_BindToActorType()
        {
            string actorType = "myActor";
            await this.CallFunctionAsync(nameof(Functions.GetActorState_BindToActorType), "actorType", actorType);

            SavedHttpRequest req = this.GetActorStateRequest();
            Assert.Equal("/v1.0/actors/myActor/abc/state/key1", req.Path);

            IEnumerable<string> functionLogs = this.GetFunctionLogs(nameof(Functions.GetActorState_BindToActorType));
            Assert.Contains(testStoredState.ToString(), functionLogs);
        }

        [Fact]
        public async Task GetActorState_BindToActorId()
        {
            string actorId = "abc";
            await this.CallFunctionAsync(nameof(Functions.GetActorState_BindToActorId), "actorId", actorId);

            SavedHttpRequest req = this.GetActorStateRequest();
            Assert.Equal("/v1.0/actors/myActor/abc/state/key1", req.Path);

            IEnumerable<string> functionLogs = this.GetFunctionLogs(nameof(Functions.GetActorState_BindToActorId));
            Assert.Contains(testStoredState.ToString(), functionLogs);
        }


        [Fact]
        public async Task GetActorState_BindToKeyName()
        {
            string key = "key1";
            await this.CallFunctionAsync(nameof(Functions.GetActorState_BindToKey), "key", key);

            SavedHttpRequest req = this.GetActorStateRequest();
            Assert.Equal("/v1.0/actors/myActor/abc/state/key1", req.Path);

            IEnumerable<string> functionLogs = this.GetFunctionLogs(nameof(Functions.GetActorState_BindToKey));
            Assert.Contains(testStoredState.ToString(), functionLogs);
        }

        [Fact]
        public async Task GetActorState_BindToString()
        {
            await this.CallFunctionAsync(nameof(Functions.GetActorState_BindToString));

            SavedHttpRequest req = this.GetActorStateRequest();
            Assert.Equal("/v1.0/actors/myActor/abc/state/key1", req.Path);

            IEnumerable<string> functionLogs = this.GetFunctionLogs(nameof(Functions.GetActorState_BindToString));
            Assert.Contains(JsonConvert.SerializeObject(testStoredState), functionLogs);
        }

        [Fact]
        public async Task GetActorState_BindToByteArray()
        {
            await this.CallFunctionAsync(nameof(Functions.GetActorState_BindToByteArray));

            SavedHttpRequest req = this.GetActorStateRequest();
            Assert.Equal("/v1.0/actors/myActor/abc/state/key1", req.Path);

            IEnumerable<string> functionLogs = this.GetFunctionLogs(nameof(Functions.GetActorState_BindToByteArray));
            Assert.Contains(JsonConvert.SerializeObject(testStoredState), functionLogs);
        }

        [Theory]
        [MemberData(nameof(GetObjectAsyncCollectorInputs))]
        public async Task SaveActorState_ObjectAsyncCollector(object input)
        {
            await this.CallFunctionAsync(nameof(Functions.SaveActorState_ObjectAsyncCollector), "input", input);
            SavedHttpRequest req = this.GetSingleSaveActorStateRequest();
            Assert.Equal("/v1.0/actors/myActor/abc/state/key1", req.Path);

            string expectedValue = JsonConvert.SerializeObject(input);
            Assert.Equal(expectedValue, req.ContentAsString);
        }

        [Theory]
        [InlineData("MyKey")]
        public async Task SaveState_BindToKeyName(string keyName)
        {
            await this.CallFunctionAsync(nameof(Functions.SaveActorState_BindToKey), "key", keyName);
            SavedHttpRequest req = this.GetSingleSaveActorStateRequest();

            Assert.Equal($"/v1.0/actors/myActor/abc/state/{keyName}", req.Path);
            Assert.Equal(JsonConvert.SerializeObject(testStoredState), req.ContentAsString);
        }


        [Fact]
        public async Task SaveActorState_BindToJObject_AllFields()
        {
            var parameters = new { actorType = "myActor", actorId = "abc", key = "key1", value = testStoredState};
            await this.CallFunctionAsync(
                nameof(Functions.SaveActorState_BindToJObject_AllFields),
                "saveStateParameters",
                JObject.FromObject(parameters));

            SavedHttpRequest req = this.GetSingleSaveActorStateRequest();
            Assert.Equal("/v1.0/actors/myActor/abc/state/key1", req.Path);
            Assert.Equal(JsonConvert.SerializeObject(testStoredState), req.ContentAsString);
        }

        SavedHttpRequest GetActorStateRequest()
        {
            SavedHttpRequest[] requests = this.GetDaprRequests();
            SavedHttpRequest req = Assert.Single(requests);
            Assert.Equal("GET", req.Method);
            return req;
        }

        SavedHttpRequest GetSingleSaveActorStateRequest()
        {
            SavedHttpRequest[] requests = this.GetDaprRequests();
            SavedHttpRequest req = Assert.Single(requests);
            Assert.StartsWith("application/json", req.ContentType);
            Assert.Equal("POST", req.Method);
            return req;
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

        // WARNING: In spite of what these test functions are doing, it's never a good idea to log secrets.
        static class Functions
        {
            public static void GetActorState_ExplicitSettings(
                [DaprActorState("myActor", "abc", Key = "key1")] JObject state,
                ILogger log) => log.LogInformation(state.ToString());


            public static void GetActorState_BindToActorType(
                string actorType,
                [DaprActorState("{actorType}", "abc", Key = "key1")] JObject state,
                ILogger log) => log.LogInformation(state.ToString());

            public static void GetActorState_BindToActorId(
                string actorId,
                [DaprActorState("myActor", "{actorId}", Key = "key1")] JObject state,
                ILogger log) => log.LogInformation(state.ToString());

            public static void GetActorState_BindToKey(
                string key,
                [DaprActorState("myActor", "abc", Key = "{key}")] JObject state,
                ILogger log) => log.LogInformation(state.ToString());


            public static void GetActorState_BindToString(
                [DaprActorState("myActor", "abc", Key = "key1")] string state,
                ILogger log) => log.LogInformation(state);

            public static void GetActorState_BindToByteArray(
                [DaprActorState("myActor", "abc", Key = "key1")] byte[] state,
                ILogger log) => log.LogInformation(Encoding.UTF8.GetString(state));

            [NoAutomaticTrigger]
            public static Task SaveActorState_ObjectAsyncCollector(
                object input,
                [DaprActorState("myActor", "abc", Key = "key1")] IAsyncCollector<object> state)
            {
                // Any JSON-serializeable type should be supported
                return state.AddAsync(input);
            }

            [NoAutomaticTrigger]
            public static Task SaveActorState_BindToKey(
                string key,
                [DaprActorState("myActor", "abc", Key = "{key}")] IAsyncCollector<object> state)
            {
                // Any JSON-serializeable type should be supported
                return state.AddAsync(testStoredState);
            }

            [NoAutomaticTrigger]
            public static Task SaveActorState_BindToJObject_AllFields(
                JObject saveStateParameters,
                [DaprActorState("{saveStateParameters.actorType}", "{saveStateParameters.actorId}")] IAsyncCollector<JObject> state)
            {
                return state.AddAsync(saveStateParameters);
            }
        }
    }
}
