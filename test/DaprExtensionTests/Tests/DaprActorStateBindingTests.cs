// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace DaprExtensionTests.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Azure.WebJobs.Extensions.Dapr;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Xunit;
    using Xunit.Abstractions;

    public class DaprActorStateBindingTests : DaprTestBase
    {
        public DaprActorStateBindingTests(ITestOutputHelper output)
            : base(output)
        {
            this.AddFunctions(typeof(Functions));
        }

        static readonly JObject testStoredState = JObject.Parse(@"{""PropertyA"": ""ValueA"", ""PropertyB"": ""ValueB""}");

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
        public async Task GetActorState_BindToJObject()
        {
            await this.CallFunctionAsync(nameof(Functions.GetActorState_BindToJObject));

            SavedHttpRequest req = this.GetActorStateRequest();
            Assert.Equal("/v1.0/actors/myActor/abc/state/key1", req.Path);

            IEnumerable<string> functionLogs = this.GetFunctionLogs(nameof(Functions.GetActorState_BindToJObject));
            Assert.Contains(testStoredState.ToString(), functionLogs);
        }

        SavedHttpRequest GetActorStateRequest()
        {
            SavedHttpRequest[] requests = this.GetDaprRequests();
            SavedHttpRequest req = Assert.Single(requests);
            Assert.Equal("GET", req.Method);
            return req;
        }

        // WARNING: In spite of what these test functions are doing, it's never a good idea to log secrets.
        static class Functions
        {
            public static void GetActorState_ExplicitSettings(
                [DaprActorState("myActor", "abc", "key1")] JObject state,
                ILogger log) => log.LogInformation(state.ToString());


            public static void GetActorState_BindToActorType(
                string actorType,
                [DaprActorState("{actorType}", "abc", "key1")] JObject state,
                ILogger log) => log.LogInformation(state.ToString());

            public static void GetActorState_BindToActorId(
                string actorId,
                [DaprActorState("myActor", "{actorId}", "key1")] JObject state,
                ILogger log) => log.LogInformation(state.ToString());

            public static void GetActorState_BindToKey(
                string key,
                [DaprActorState("myActor", "abc", "{key}")] JObject state,
                ILogger log) => log.LogInformation(state.ToString());


            public static void GetActorState_BindToJObject(
                [DaprActorState("myActor", "abc", "key1")] string state,
                ILogger log) => log.LogInformation(state);
        }
    }
}
