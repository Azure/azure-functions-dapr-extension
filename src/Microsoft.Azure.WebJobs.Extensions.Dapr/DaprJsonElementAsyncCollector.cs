// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// ------------------------------------------------------------

namespace Microsoft.Azure.WebJobs.Extensions.Dapr
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.Dapr.Services;
    using Newtonsoft.Json.Linq;

    internal class DaprJsonElementAsyncCollector : IAsyncCollector<JsonElement>
    {
        readonly ConcurrentBag<DaprStateRecord> requests = new ConcurrentBag<DaprStateRecord>();
        private readonly IDaprServiceClient daprServiceClient;
        private readonly DaprStateAttribute attribute;

        public DaprJsonElementAsyncCollector(IDaprServiceClient daprServiceClient, DaprStateAttribute attribute)
        {
            this.daprServiceClient = daprServiceClient;
            this.attribute = attribute;
        }

        public Task AddAsync(JsonElement parametersJson, CancellationToken cancellationToken = default)
        {
            if (!parametersJson!.TryGetProperty("value", out JsonElement value))
            {
                throw new ArgumentException("A 'value' parameter is required for save-state operations.", nameof(parametersJson));
            }

            var parameters = new DaprStateRecord(value);

            if (parametersJson.TryGetProperty("key", out JsonElement key))
            {
                parameters.Key = key.GetString();
            }

            this.requests.Add(parameters);

            return Task.CompletedTask;
        }

        public Task FlushAsync(CancellationToken cancellationToken = default)
        {
            return this.daprServiceClient.SaveStateAsync(
                    this.attribute.DaprAddress,
                    this.attribute.StateStore,
                    this.requests.Take(this.requests.Count),
                    cancellationToken);
        }
    }
}