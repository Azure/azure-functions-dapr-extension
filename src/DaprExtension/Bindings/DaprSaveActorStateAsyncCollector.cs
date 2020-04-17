// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.Azure.WebJobs.Extensions.Dapr
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    class DaprSaveActorStateAsyncCollector : IAsyncCollector<DaprActorStateRecord>
    {
        readonly ConcurrentQueue<DaprActorStateRecord> requests = new ConcurrentQueue<DaprActorStateRecord>();

        readonly DaprServiceClient daprClient;
        readonly DaprActorStateAttribute attr;

        public DaprSaveActorStateAsyncCollector(DaprActorStateAttribute attr, DaprServiceClient daprClient)
        {
            this.attr = attr;
            this.daprClient = daprClient;
        }

        public Task AddAsync(DaprActorStateRecord item, CancellationToken cancellationToken = default)
        {
            this.requests.Enqueue(item);

            return Task.CompletedTask;
        }

        public async Task FlushAsync(CancellationToken cancellationToken = default)
        {
            while (this.requests.TryDequeue(out DaprActorStateRecord item))
            {
                await this.daprClient.SaveActorStateAsync(
                 this.attr.DaprAddress,
                 this.attr.ActorType,
                 this.attr.ActorId,
                 this.attr.Key ?? item.Key, // if attribute did not specify, check if key is binded into the record
                 item,
                 cancellationToken);
            }
        }
    }
}