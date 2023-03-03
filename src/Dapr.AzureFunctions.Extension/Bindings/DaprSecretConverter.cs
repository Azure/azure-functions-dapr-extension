// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// ------------------------------------------------------------

namespace Dapr.AzureFunctions.Extension
{
    using System.Collections.Generic;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.WebJobs;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    class DaprSecretConverter :
        IAsyncConverter<DaprSecretAttribute, JObject>,
        IAsyncConverter<DaprSecretAttribute, IDictionary<string, string>>,
        IAsyncConverter<DaprSecretAttribute, byte[]>,
        IAsyncConverter<DaprSecretAttribute, string?>
    {
        readonly DaprServiceClient daprClient;

        public DaprSecretConverter(DaprServiceClient daprClient)
        {
            this.daprClient = daprClient;
        }

        Task<JObject> IAsyncConverter<DaprSecretAttribute, JObject>.ConvertAsync(
            DaprSecretAttribute input,
            CancellationToken cancellationToken)
        {
            return this.GetSecretsAsync(input, cancellationToken);
        }

        async Task<IDictionary<string, string>> IAsyncConverter<DaprSecretAttribute, IDictionary<string, string>>.ConvertAsync(
            DaprSecretAttribute input,
            CancellationToken cancellationToken)
        {
            JObject result = await this.GetSecretsAsync(input, cancellationToken);
            var obj = result.ToObject<Dictionary<string, string>>();
            return obj ?? new Dictionary<string, string>();
        }

        async Task<byte[]> IAsyncConverter<DaprSecretAttribute, byte[]>.ConvertAsync(
            DaprSecretAttribute input,
            CancellationToken cancellationToken)
        {
            JObject result = await this.GetSecretsAsync(input, cancellationToken);
            return Encoding.UTF8.GetBytes(result.ToString(Formatting.None));
        }

        async Task<string?> IAsyncConverter<DaprSecretAttribute, string?>.ConvertAsync(
            DaprSecretAttribute input,
            CancellationToken cancellationToken)
        {
            JObject result = await this.GetSecretsAsync(input, cancellationToken);
            return result?.ToString(Formatting.None);
        }

        Task<JObject> GetSecretsAsync(DaprSecretAttribute input, CancellationToken cancellationToken)
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
