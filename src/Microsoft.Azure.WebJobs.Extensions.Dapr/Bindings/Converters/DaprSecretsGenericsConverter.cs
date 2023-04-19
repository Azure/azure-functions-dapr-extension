// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// ------------------------------------------------------------

namespace Microsoft.Azure.WebJobs.Extensions.Dapr.Bindings.Converters
{
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.WebJobs.Extensions.Dapr.Services;
    using Microsoft.Azure.WebJobs.Extensions.Dapr.Utils;

    internal class DaprSecretsGenericsConverter<T> : DaprGenericsConverterBase<DaprSecretAttribute, T>
    {
        readonly IDaprServiceClient daprClient;

        public DaprSecretsGenericsConverter(IDaprServiceClient daprClient)
            : base(daprClient)
        {
            this.daprClient = daprClient;
        }

        /// <summary>
        /// Gets the string representation of DaprSecretAttribute.
        /// </summary>
        /// <param name="input">The DaprSecretAttribute to be serialized.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        public async override Task<string> GetStringContentAsync(DaprSecretAttribute input, CancellationToken cancellationToken)
        {
            var secret = await this.daprClient.GetSecretAsync(
                    input.DaprAddress,
                    input.SecretStoreName,
                    input.Key,
                    input.Metadata,
                    cancellationToken);
            return JsonSerializer.Serialize(secret, JsonUtils.DefaultSerializerOptions);
        }
    }
}