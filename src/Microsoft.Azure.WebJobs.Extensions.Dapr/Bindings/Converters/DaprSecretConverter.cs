// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// ------------------------------------------------------------

namespace Microsoft.Azure.WebJobs.Extensions.Dapr.Bindings.Converters
{
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Functions.Extensions.Dapr.Core.Utils;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.Dapr.Services;
    using Newtonsoft.Json.Linq;

    class DaprSecretConverter :
        IAsyncConverter<DaprSecretAttribute, string?>,
        IAsyncConverter<DaprSecretAttribute, JsonElement>,
        IAsyncConverter<DaprSecretAttribute, JObject>,
        IAsyncConverter<DaprSecretAttribute, JToken>
    {
        readonly IDaprServiceClient daprClient;

        public DaprSecretConverter(IDaprServiceClient daprClient)
        {
            this.daprClient = daprClient;
        }

        async Task<string?> IAsyncConverter<DaprSecretAttribute, string?>.ConvertAsync(
            DaprSecretAttribute input,
            CancellationToken cancellationToken)
        {
            JsonDocument result = await this.GetSecretsAsync(input, cancellationToken);
            return JsonSerializer.Serialize(result, JsonUtils.DefaultSerializerOptions);
        }

        async Task<JsonElement> IAsyncConverter<DaprSecretAttribute, JsonElement>.ConvertAsync(
            DaprSecretAttribute input,
            CancellationToken cancellationToken)
        {
            return (await this.GetSecretsAsync(input, cancellationToken)).RootElement;
        }

        async Task<JObject> IAsyncConverter<DaprSecretAttribute, JObject>.ConvertAsync(
            DaprSecretAttribute input,
            CancellationToken cancellationToken)
        {
            JsonDocument result = await this.GetSecretsAsync(input, cancellationToken);
            return JObject.Parse(JsonSerializer.Serialize(result, JsonUtils.DefaultSerializerOptions));
        }

        async Task<JToken> IAsyncConverter<DaprSecretAttribute, JToken>.ConvertAsync(
            DaprSecretAttribute input,
            CancellationToken cancellationToken)
        {
            JsonDocument result = await this.GetSecretsAsync(input, cancellationToken);
            return JToken.Parse(JsonSerializer.Serialize(result, JsonUtils.DefaultSerializerOptions));
        }

        private Task<JsonDocument> GetSecretsAsync(DaprSecretAttribute input, CancellationToken cancellationToken)
        {
            return this.daprClient.GetSecretAsync(
                input.DaprAddress,
                input.SecretStoreName,
                input.Key,
                input.Metadata,
                cancellationToken);
        }
    }
}