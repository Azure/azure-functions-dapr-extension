// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// ------------------------------------------------------------

namespace DaprExtensionTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.Json;
    using System.Threading.Tasks;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extension.Dapr;
    using Microsoft.Extensions.Logging;
    using Xunit;
    using Xunit.Abstractions;

    // TODO: Need to test variations of JsonElement where stateStore and key are defined in the binding attribute
    public class DaprStateBindingTests : DaprTestBase
    {
        public DaprStateBindingTests(ITestOutputHelper output)
            : base(output)
        {
            this.AddFunctions(typeof(Functions));
        }

        [Theory]
        [MemberData(nameof(GetObjectAsyncCollectorInputs))]
        public async Task SaveState_ObjectAsyncCollector(object input)
        {
            await this.CallFunctionAsync(nameof(Functions.SaveState_ObjectAsyncCollector), "input", input);
            SavedHttpRequest req = this.GetSingleSaveStateRequest();

            string expectedValue = JsonSerializer.Serialize(input);
            Assert.Equal("/v1.0/state/store1", req.Path);
            Assert.Equal(@$"[{{""key"":""key1"",""value"":{expectedValue}}}]", req.ContentAsString);
        }

        [Theory]
        [InlineData("MyKey")]
        [InlineData("{7c532697-7080-49c6-b39a-a1d39edecf8e}")]
        [InlineData("http://dapr.io/keys/MyKey")]
        public async Task SaveState_BindToKeyName(string keyName)
        {
            await this.CallFunctionAsync(nameof(Functions.SaveState_BindToKeyName), "key", keyName);
            SavedHttpRequest req = this.GetSingleSaveStateRequest();

            Assert.Equal("/v1.0/state/store1", req.Path);
            Assert.Equal(@$"[{{""key"":""{keyName}"",""value"":42}}]", req.ContentAsString);

            this.ValidatePersistedState(42, "store1", keyName);
        }

        [Theory]
        [InlineData("MyKey")]
        [InlineData("{7c532697-7080-49c6-b39a-a1d39edecf8e}")]
        [InlineData("http://dapr.io/keys/MyStore")]
        public async Task SaveState_BindToStateStoreName(string storeName)
        {
            await this.CallFunctionAsync(nameof(Functions.SaveState_BindToStoreName), "storeName", storeName);
            SavedHttpRequest req = this.GetSingleSaveStateRequest();

            Assert.Equal($"/v1.0/state/{Uri.EscapeDataString(storeName)}", req.Path);
            Assert.Equal(@$"[{{""key"":""key1"",""value"":42}}]", req.ContentAsString);

            this.ValidatePersistedState(42, storeName, "key1");
        }

        [Fact]
        public async Task SaveState_BindToJObject_AllFields()
        {
            var parameters = new { stateStore = "store1", key = "key1", value = "value1" };
            await this.CallFunctionAsync(
                nameof(Functions.SaveState_BindToJObject_AllFields),
                "saveStateParameters",
                JsonDocument.Parse(JsonSerializer.Serialize(parameters)).RootElement);

            SavedHttpRequest req = this.GetSingleSaveStateRequest();

            Assert.Equal($"/v1.0/state/{parameters.stateStore}", req.Path);
            Assert.Equal(@$"[{{""key"":""{parameters.key}"",""value"":""{parameters.value}""}}]", req.ContentAsString);

            this.ValidatePersistedState(parameters.value, parameters.stateStore, parameters.key);
        }

        [Fact]
        public async Task SaveState_BatchMultipleCalls()
        {
            var inputs = new Dictionary<string, int>()
            {
                { "key1", 1 },
                { "key2", 2 },
                { "key3", 3 },
            };

            await this.CallFunctionAsync(nameof(Functions.SaveState_BatchMultipleCalls), "inputs", inputs);
            SavedHttpRequest req = this.GetSingleSaveStateRequest();

            Assert.Equal($"/v1.0/state/store1", req.Path);

            // Sort the elements since the order may change.
            // This is preferred over DeepEquals because we get more debug info from xunit when comparing collections
            JsonElement[] expected = JsonDocument.Parse(@"[{""key"":""key1"",""value"":1},{""key"":""key2"",""value"":2},{""key"":""key3"",""value"":3}]").RootElement.EnumerateArray().ToArray();
            JsonElement[] actual = JsonDocument.Parse(req.ContentAsString).RootElement.EnumerateArray().ToArray();
            var comparer = new Comparison<JsonElement>((t1, t2) => JsonSerializer.Serialize(t1).CompareTo(JsonSerializer.Serialize(t2)));
            Array.Sort(expected, comparer);
            Array.Sort(actual, comparer);

            Assert.Equal(expected, actual);

            foreach (var kvp in inputs)
            {
                this.ValidatePersistedState(kvp.Value, "store1", kvp.Key);
            }
        }

        [Fact]
        public async Task GetState_BindToKeyName()
        {
            string keyName = "myStateKey";
            await this.CallFunctionAsync(nameof(Functions.SaveState_BindToKeyName), "key", keyName);
            await this.CallFunctionAsync(nameof(Functions.GetState_BindToKeyName), "key", keyName);

            SavedHttpRequest[] requests = this.GetDaprRequests();
            Assert.Equal(2, requests.Length);
            Assert.Single(requests, r => r.Method == "GET");
            Assert.Single(requests, r => r.Method == "POST");
            Assert.All(requests, r => r.Path.EndsWith(keyName));

            IEnumerable<string> functionLogs = this.GetFunctionLogs(nameof(Functions.GetState_BindToKeyName));
            Assert.Contains("42", functionLogs);
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

        SavedHttpRequest GetSingleSaveStateRequest()
        {
            SavedHttpRequest[] requests = this.GetDaprRequests();
            SavedHttpRequest req = Assert.Single(requests);
            Assert.StartsWith("application/json", req.ContentType);
            Assert.Equal("POST", req.Method);
            return req;
        }

        void ValidatePersistedState(object expectedState, string targetStateStore, string targetKey)
        {
            object? actualState = this.FetchSavedStateForUnitTesting(targetStateStore, targetKey);
            Assert.NotNull(actualState);
            Assert.Equal(expectedState, actualState);
        }

        static class Functions
        {
            [NoAutomaticTrigger]
            public static Task SaveState_ObjectAsyncCollector(
                object input,
                [DaprState("store1", Key = "key1")] IAsyncCollector<object> state)
            {
                // Any JSON-serializeable type should be supported
                return state.AddAsync(input);
            }

            [NoAutomaticTrigger]
            public static Task SaveState_BindToKeyName(
                string key,
                [DaprState("store1", Key = "{key}")] IAsyncCollector<int> state)
            {
                return state.AddAsync(42);
            }

            [NoAutomaticTrigger]
            public static Task SaveState_BindToStoreName(
                string storeName,
                [DaprState("{storeName}", Key = "key1")] IAsyncCollector<int> state)
            {
                return state.AddAsync(42);
            }

            [NoAutomaticTrigger]
            public static Task SaveState_BindToJObject_AllFields(
                JsonElement saveStateParameters,
                [DaprState("{saveStateParameters.stateStore}")] IAsyncCollector<JsonElement> state)
            {
                return state.AddAsync(saveStateParameters);
            }

            [NoAutomaticTrigger]
            public static async Task SaveState_BatchMultipleCalls(
                Dictionary<string, int> inputs,
                [DaprState("store1")] IAsyncCollector<DaprStateRecord> records)
            {
                foreach ((string key, int value) in inputs)
                {
                    await records.AddAsync(new DaprStateRecord(key, value));
                }
            }

            [NoAutomaticTrigger]
            public static void GetState_BindToKeyName(
                string key,
                [DaprState("store1", Key = "{key}")] DaprStateRecord state,
                ILogger log)
            {
                log.LogInformation(state.Value?.ToString() ?? string.Empty);
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
