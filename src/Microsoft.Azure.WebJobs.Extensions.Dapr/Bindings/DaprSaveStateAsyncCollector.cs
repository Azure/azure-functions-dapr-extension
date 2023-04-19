// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// ------------------------------------------------------------

namespace Microsoft.Azure.WebJobs.Extensions.Dapr
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.Dapr.Services;

    class DaprSaveStateAsyncCollector : IAsyncCollector<DaprStateRecord>
    {
        readonly ConcurrentBag<DaprStateRecord> requests = new ConcurrentBag<DaprStateRecord>();

        readonly IDaprServiceClient daprClient;
        readonly DaprStateAttribute attr;

        public DaprSaveStateAsyncCollector(DaprStateAttribute attr, IDaprServiceClient daprClient)
        {
            this.attr = attr;
            this.daprClient = daprClient;
        }

        public Task AddAsync(DaprStateRecord item, CancellationToken cancellationToken = default)
        {
            if (item.Key == null)
            {
                item.Key = this.attr.Key ?? throw new ArgumentException("No key information was found. Make sure it is configured either in the binding properties or in the data payload.", nameof(item));
            }

            this.requests.Add(item);

            return Task.CompletedTask;
        }

        public Task FlushAsync(CancellationToken cancellationToken = default)
        {
            return this.daprClient.SaveStateAsync(
                this.attr.DaprAddress,
                this.attr.StateStore,
                this.requests.Take(this.requests.Count),
                cancellationToken);
        }
    }
}