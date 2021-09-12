// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// ------------------------------------------------------------

namespace Dapr.AzureFunctions.Extension
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.WebJobs;

    class DaprSaveStateAsyncCollector : IAsyncCollector<DaprStateRecord>
    {
        readonly ConcurrentQueue<DaprStateRecord> requests = new ConcurrentQueue<DaprStateRecord>();

        readonly DaprServiceClient daprClient;
        readonly DaprStateAttribute attr;

        public DaprSaveStateAsyncCollector(DaprStateAttribute attr, DaprServiceClient daprClient)
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

            this.requests.Enqueue(item);

            return Task.CompletedTask;
        }

        public Task FlushAsync(CancellationToken cancellationToken = default)
        {
            Dictionary<string, DaprStateRecord> requests = new Dictionary<string, DaprStateRecord>();
            while (this.requests.TryDequeue(out DaprStateRecord item))
            {
                requests[item.Key!] = item;
            }

            return this.daprClient.SaveStateAsync(
                this.attr.DaprAddress,
                this.attr.StateStore,
                requests.Values,
                cancellationToken);
        }
    }
}