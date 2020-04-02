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

    public class DaprSecretBindingTests : DaprTestBase
    {
        public DaprSecretBindingTests(ITestOutputHelper output)
            : base(output)
        {
            this.AddFunctions(typeof(Functions));
        }

        [Fact]
        public async Task GetSecret_ExplicitSettings_Metadata()
        {
            await this.CallFunctionAsync(nameof(Functions.GetSecret_ExplicitSettings_Metadata));

            SavedHttpRequest req = this.GetSingleGetSecretRequest();
            Assert.Equal("/v1.0/secrets/store1/key1", req.Path);
            Assert.Equal("?metadata.version_id=1&metadata.version_stage=2", req.Query.ToString());

            IEnumerable<string> functionLogs = this.GetFunctionLogs(nameof(Functions.GetSecret_ExplicitSettings_Metadata));
            Assert.Contains("secret!", functionLogs);
        }

        [Fact]
        public async Task GetSecret_ExplicitSettings_NoMetadata()
        {
            await this.CallFunctionAsync(nameof(Functions.GetSecret_ExplicitSettings_NoMetadata));

            SavedHttpRequest req = this.GetSingleGetSecretRequest();
            Assert.Equal("/v1.0/secrets/store1/key1", req.Path);
            Assert.Equal(QueryString.Empty, req.Query);

            IEnumerable<string> functionLogs = this.GetFunctionLogs(nameof(Functions.GetSecret_ExplicitSettings_NoMetadata));
            Assert.Contains("secret!", functionLogs);
        }

        [Fact]
        public async Task GetSecret_BindToSecretStoreName()
        {
            string storeName = "store1";
            await this.CallFunctionAsync(nameof(Functions.GetSecret_BindToSecretStoreName), "store", storeName);

            SavedHttpRequest req = this.GetSingleGetSecretRequest();
            Assert.Equal($"/v1.0/secrets/{storeName}/key1", req.Path);
            Assert.Equal(QueryString.Empty, req.Query);

            IEnumerable<string> functionLogs = this.GetFunctionLogs(nameof(Functions.GetSecret_BindToSecretStoreName));
            Assert.Contains("secret!", functionLogs);
        }

        [Fact]
        public async Task GetSecret_BindToKeyName()
        {
            string keyName = "key1";
            await this.CallFunctionAsync(nameof(Functions.GetSecret_BindToKeyName), "key", keyName);

            SavedHttpRequest req = this.GetSingleGetSecretRequest();
            Assert.Equal($"/v1.0/secrets/store1/{keyName}", req.Path);
            Assert.Equal(QueryString.Empty, req.Query);

            IEnumerable<string> functionLogs = this.GetFunctionLogs(nameof(Functions.GetSecret_BindToKeyName));
            Assert.Contains("secret!", functionLogs);
        }

        [Fact]
        public async Task GetSecret_BindToMetadata()
        {
            JObject metadata = JObject.FromObject(
                new
                {
                    version_id = 3,
                    version_stage = 4,
                });
            await this.CallFunctionAsync(nameof(Functions.GetSecret_BindToMetadata), "metadata", metadata);

            SavedHttpRequest req = this.GetSingleGetSecretRequest();
            Assert.Equal("/v1.0/secrets/store1/key1", req.Path);
            Assert.Equal("?metadata.version_id=3&metadata.version_stage=4", req.Query.ToString());

            IEnumerable<string> functionLogs = this.GetFunctionLogs(nameof(Functions.GetSecret_BindToMetadata));
            Assert.Contains("secret!", functionLogs);
        }

        [Fact]
        public async Task GetSecret_BindToJObject()
        {
            await this.CallFunctionAsync(nameof(Functions.GetSecret_BindToJObject));

            SavedHttpRequest req = this.GetSingleGetSecretRequest();
            Assert.Equal("/v1.0/secrets/store1/key1", req.Path);
            Assert.Equal(QueryString.Empty, req.Query);

            IEnumerable<string> functionLogs = this.GetFunctionLogs(nameof(Functions.GetSecret_BindToJObject));
            Assert.Contains(@"{""key1"":""secret!""}", functionLogs);
        }

        [Fact]
        public async Task GetSecret_BindToByteArray()
        {
            await this.CallFunctionAsync(nameof(Functions.GetSecret_BindToByteArray));

            SavedHttpRequest req = this.GetSingleGetSecretRequest();
            Assert.Equal("/v1.0/secrets/store1/key1", req.Path);
            Assert.Equal(QueryString.Empty, req.Query);

            IEnumerable<string> functionLogs = this.GetFunctionLogs(nameof(Functions.GetSecret_BindToByteArray));
            Assert.Contains("secret!", functionLogs);
        }

        SavedHttpRequest GetSingleGetSecretRequest()
        {
            SavedHttpRequest[] requests = this.GetDaprRequests();
            SavedHttpRequest req = Assert.Single(requests);
            Assert.Equal("GET", req.Method);
            return req;
        }

        // WARNING: In spite of what these test functions are doing, it's never a good idea to log secrets.
        static class Functions
        {
            public static void GetSecret_ExplicitSettings_Metadata(
                [DaprSecret("store1", "key1", Metadata = "metadata.version_id=1&metadata.version_stage=2")] string secret,
                ILogger log) => log.LogInformation(secret);

            public static void GetSecret_ExplicitSettings_NoMetadata(
                [DaprSecret("store1", "key1")] string secret,
                ILogger log) => log.LogInformation(secret);

            public static void GetSecret_BindToSecretStoreName(
                string store,
                [DaprSecret("{store}", "key1")] string secret,
                ILogger log) => log.LogInformation(secret);

            public static void GetSecret_BindToKeyName(
                string key,
                [DaprSecret("store1", "{key}")] string secret,
                ILogger log) => log.LogInformation(secret);

            public static void GetSecret_BindToMetadata(
                JObject metadata,
                [DaprSecret("store1", "key1", Metadata = "metadata.version_id={metadata.version_id}&metadata.version_stage={metadata.version_stage}")] string secret,
                ILogger log) => log.LogInformation(secret);

            public static void GetSecret_BindToJObject(
                [DaprSecret("store1", "key1")] JObject secret,
                ILogger log) => log.LogInformation(secret.ToString(Formatting.None));

            public static void GetSecret_BindToByteArray(
                [DaprSecret("store1", "key1")] byte[] secret,
                ILogger log) => log.LogInformation(Encoding.UTF8.GetString(secret));
        }
    }
}
