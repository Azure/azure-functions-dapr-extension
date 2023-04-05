// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// ------------------------------------------------------------

namespace Microsoft.Azure.WebJobs.Extension.Dapr
{
    using System.Text.Encodings.Web;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;

    internal class DaprSecretsGenericsConverter<T> : DaprGenericsConverterBase<DaprSecretAttribute, T>
    {
        private static readonly JsonSerializerOptions SerializerOptions = new JsonSerializerOptions()
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        };

        readonly DaprServiceClient daprClient;

        public DaprSecretsGenericsConverter(DaprServiceClient daprClient)
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
            return JsonSerializer.Serialize(secret, SerializerOptions);
        }
    }
}