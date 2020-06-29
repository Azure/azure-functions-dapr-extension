// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// ------------------------------------------------------------

namespace DaprExtensionTests
{
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Dapr.AzureFunctions.Extension;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Xunit;
    using Xunit.Abstractions;
    using System.Linq;

    public class DaprSecretBindingTests : DaprTestBase
    {
        static readonly string ExpectedSecret = JObject.Parse(@$"{{""key1"":""secret!"", ""key2"":""another secret!""}}").ToString(Formatting.None);
        
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
            Assert.Equal("/v1.0/secrets/store1/key", req.Path);
            Assert.Equal("?metadata.version_id=1&metadata.version_stage=2", req.Query.ToString());

            IEnumerable<string> functionLogs = this.GetFunctionLogs(nameof(Functions.GetSecret_ExplicitSettings_Metadata));
            Assert.Contains(ExpectedSecret, functionLogs);
        }

        [Fact]
        public async Task GetSecret_ExplicitSettings_NoMetadata()
        {
            await this.CallFunctionAsync(nameof(Functions.GetSecret_ExplicitSettings_NoMetadata));

            SavedHttpRequest req = this.GetSingleGetSecretRequest();
            Assert.Equal("/v1.0/secrets/store1/key", req.Path);
            Assert.Equal(QueryString.Empty, req.Query);

            IEnumerable<string> functionLogs = this.GetFunctionLogs(nameof(Functions.GetSecret_ExplicitSettings_NoMetadata));
            Assert.Contains(ExpectedSecret, functionLogs);
        }

        [Fact]
        public async Task GetSecret_BindToSecretStoreName()
        {
            string storeName = "store1";
            await this.CallFunctionAsync(nameof(Functions.GetSecret_BindToSecretStoreName), "store", storeName);

            SavedHttpRequest req = this.GetSingleGetSecretRequest();
            Assert.Equal($"/v1.0/secrets/{storeName}/key", req.Path);
            Assert.Equal(QueryString.Empty, req.Query);

            IEnumerable<string> functionLogs = this.GetFunctionLogs(nameof(Functions.GetSecret_BindToSecretStoreName));
            Assert.Contains(ExpectedSecret, functionLogs);
        }

        [Fact]
        public async Task GetSecret_BindToKeyName()
        {
            string keyName = "key";
            await this.CallFunctionAsync(nameof(Functions.GetSecret_BindToKeyName), "key", keyName);

            SavedHttpRequest req = this.GetSingleGetSecretRequest();
            Assert.Equal($"/v1.0/secrets/store1/{keyName}", req.Path);
            Assert.Equal(QueryString.Empty, req.Query);

            IEnumerable<string> functionLogs = this.GetFunctionLogs(nameof(Functions.GetSecret_BindToKeyName));
            Assert.Contains(ExpectedSecret, functionLogs);
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
            Assert.Equal("/v1.0/secrets/store1/key", req.Path);
            Assert.Equal("?metadata.version_id=3&metadata.version_stage=4", req.Query.ToString());

            IEnumerable<string> functionLogs = this.GetFunctionLogs(nameof(Functions.GetSecret_BindToMetadata));
            Assert.Contains(ExpectedSecret, functionLogs);
        }

        [Fact]
        public async Task GetSecret_BindToJObject()
        {
            await this.CallFunctionAsync(nameof(Functions.GetSecret_BindToJObject));

            SavedHttpRequest req = this.GetSingleGetSecretRequest();
            Assert.Equal("/v1.0/secrets/store1/key", req.Path);
            Assert.Equal(QueryString.Empty, req.Query);

            IEnumerable<string> functionLogs = this.GetFunctionLogs(nameof(Functions.GetSecret_BindToJObject));
            Assert.Contains(ExpectedSecret, functionLogs);
        }

        [Fact]
        public async Task GetSecret_BindToDictionary()
        {
            await this.CallFunctionAsync(nameof(Functions.GetSecret_BindToDictionary));

            SavedHttpRequest req = this.GetSingleGetSecretRequest();
            Assert.Equal("/v1.0/secrets/store1/key", req.Path);
            Assert.Equal(QueryString.Empty, req.Query);

            IEnumerable<string> functionLogs = this.GetFunctionLogs(nameof(Functions.GetSecret_BindToDictionary));
            Assert.Contains(@"key1: secret!, key2: another secret!", functionLogs);
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
                [DaprSecret("store1", "key", Metadata = "metadata.version_id=1&metadata.version_stage=2")] string secret,
                ILogger log) => log.LogInformation(secret);

            public static void GetSecret_ExplicitSettings_NoMetadata(
                [DaprSecret("store1", "key")] string secret,
                ILogger log) => log.LogInformation(secret);

            public static void GetSecret_BindToSecretStoreName(
                string store,
                [DaprSecret("{store}", "key")] string secret,
                ILogger log) => log.LogInformation(secret);

            public static void GetSecret_BindToKeyName(
                string key,
                [DaprSecret("store1", "{key}")] string secret,
                ILogger log) => log.LogInformation(secret);

            public static void GetSecret_BindToMetadata(
                JObject metadata,
                [DaprSecret("store1", "key", Metadata = "metadata.version_id={metadata.version_id}&metadata.version_stage={metadata.version_stage}")] string secret,
                ILogger log) => log.LogInformation(secret);

            public static void GetSecret_BindToJObject(
                [DaprSecret("store1", "key")] JObject secret,
                ILogger log) => log.LogInformation(secret.ToString(Formatting.None));

            public static void GetSecret_BindToByteArray(
                [DaprSecret("store1", "key")] byte[] secret,
                ILogger log) => log.LogInformation(Encoding.UTF8.GetString(secret));

            public static void GetSecret_BindToDictionary(
                [DaprSecret("store1", "key")] IDictionary<string, string> secret,
                ILogger log) => log.LogInformation(string.Join(", ", secret.Select(kvp => @$"{kvp.Key}: {kvp.Value}")));
        }
    }
}
