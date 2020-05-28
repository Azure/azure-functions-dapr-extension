// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// ------------------------------------------------------------

namespace Microsoft.Azure.WebJobs.Extensions.Dapr
{
    using System;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.WebJobs;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    class DaprSecretConverter :
        IAsyncConverter<DaprSecretAttribute, JObject>,
        IAsyncConverter<DaprSecretAttribute, string?>,
        IAsyncConverter<DaprSecretAttribute, byte[]>
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

        async Task<string?> IAsyncConverter<DaprSecretAttribute, string?>.ConvertAsync(
            DaprSecretAttribute input,
            CancellationToken cancellationToken)
        {
            JObject result = await this.GetSecretsAsync(input, cancellationToken);
            return result.PropertyValues().FirstOrDefault()?.ToString();
        }

        async Task<byte[]> IAsyncConverter<DaprSecretAttribute, byte[]>.ConvertAsync(
            DaprSecretAttribute input,
            CancellationToken cancellationToken)
        {
            // Just return the first value in the object.
            JObject result = await this.GetSecretsAsync(input, cancellationToken);
            JToken jsonData = result.PropertyValues().FirstOrDefault();
            if (jsonData == null)
            {
                return Array.Empty<byte>();
            }

            if (jsonData.Type == JTokenType.Bytes)
            {
                return (byte[])jsonData;
            }
            else if (jsonData.Type == JTokenType.String)
            {
                return Encoding.UTF8.GetBytes((string)jsonData);
            }

            return Encoding.UTF8.GetBytes(jsonData.ToString(Formatting.None));
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
