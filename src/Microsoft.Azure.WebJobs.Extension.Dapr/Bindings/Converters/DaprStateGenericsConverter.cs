// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// ------------------------------------------------------------

namespace Microsoft.Azure.WebJobs.Extension.Dapr
{
    using System;
    using System.Text.Encodings.Web;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;

    internal class DaprStateGenericsConverter<T> : DaprGenericsConverterBase<DaprStateAttribute, T>
    {
        private static readonly JsonSerializerOptions SerializerOptions = new JsonSerializerOptions()
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        };

        readonly DaprServiceClient daprClient;

        public DaprStateGenericsConverter(DaprServiceClient daprClient)
            : base(daprClient)
        {
            this.daprClient = daprClient;
        }

        /// <summary>
        /// Gets the string representation of DaprStateAttribute.
        /// </summary>
        /// <param name="input">The DaprStateAttribute to be serialized.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        public async override Task<string> GetStringContentAsync(DaprStateAttribute input, CancellationToken cancellationToken)
        {
            DaprStateRecord stateRecord = await this.daprClient.GetStateAsync(
                input.DaprAddress,
                input.StateStore ?? throw new ArgumentException("No state store name was specified.", nameof(input.StateStore)),
                input.Key ?? throw new ArgumentException("No state store key was specified.", nameof(input.Key)),
                cancellationToken);

            var contentJson = await JsonDocument.ParseAsync(stateRecord.ContentStream);
            return JsonSerializer.Serialize(contentJson, SerializerOptions);
        }
    }
}