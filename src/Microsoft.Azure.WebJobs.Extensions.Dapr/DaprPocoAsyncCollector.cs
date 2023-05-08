// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// ------------------------------------------------------------

namespace Microsoft.Azure.WebJobs.Extensions.Dapr
{
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.Dapr.Services;

    internal class DaprPocoAsyncCollector<T> : IAsyncCollector<T>
    {
        readonly ConcurrentBag<DaprStateRecord> requests = new ConcurrentBag<DaprStateRecord>();
        private readonly IDaprServiceClient daprServiceClient;
        private readonly DaprStateAttribute attribute;

        public DaprPocoAsyncCollector(IDaprServiceClient daprServiceClient, DaprStateAttribute attribute)
        {
            this.daprServiceClient = daprServiceClient;
            this.attribute = attribute;
        }

        public Task AddAsync(T item, CancellationToken cancellationToken = default)
        {
            var stateRecord = new DaprStateRecord(this.attribute.Key!, item!);

            this.requests.Add(stateRecord);

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