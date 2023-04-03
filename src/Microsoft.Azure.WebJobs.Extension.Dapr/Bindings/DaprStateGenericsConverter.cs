// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// ------------------------------------------------------------

namespace Microsoft.Azure.WebJobs.Extension.Dapr
{
    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.WebJobs;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <typeparam name="T">A generic type.</typeparam>
    internal class DaprStateGenericsConverter<T> : IAsyncConverter<DaprStateAttribute, T>
    {
        readonly DaprServiceClient daprClient;

        public DaprStateGenericsConverter(DaprServiceClient daprClient)
        {
            this.daprClient = daprClient;
        }

        /// <summary>
        /// Converts state store data to generic type.
        /// </summary>
        /// <param name="attribute">
        /// Contains the information about dapr state store.
        /// </param>
        /// <param name="cancellationToken">Contains cancellationToken.</param>
        /// <returns>Custom type.</returns>
        async Task<T> IAsyncConverter<DaprStateAttribute, T>.ConvertAsync(DaprStateAttribute attribute, CancellationToken cancellationToken)
        {
            string content = await this.GetStringContentAsync(attribute, cancellationToken);
            if (string.IsNullOrEmpty(content))
            {
                return default!;
            }
            else
            {
                return JsonConvert.DeserializeObject<T>(content)!;
            }
        }

        private async Task<string> GetStringContentAsync(DaprStateAttribute input, CancellationToken cancellationToken)
        {
            DaprStateRecord stateRecord = await this.GetStateRecordAsync(input, cancellationToken);
            using StreamReader reader = new StreamReader(stateRecord.ContentStream);
            return await reader.ReadToEndAsync();
        }

        private async Task<DaprStateRecord> GetStateRecordAsync(DaprStateAttribute input, CancellationToken cancellationToken)
        {
            DaprStateRecord stateRecord = await this.daprClient.GetStateAsync(
                input.DaprAddress,
                input.StateStore ?? throw new ArgumentException("No state store name was specified.", nameof(input.StateStore)),
                input.Key ?? throw new ArgumentException("No state store key was specified.", nameof(input.Key)),
                cancellationToken);
            return stateRecord;
        }
    }
}