// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Azure.WebJobs.Extensions.Dapr
{
    class DaprSaveStateAsyncCollector : IAsyncCollector<SaveStateParameters>
    {
        readonly ConcurrentQueue<SaveStateParameters> requests = new ConcurrentQueue<SaveStateParameters>();
        readonly DaprServiceClient daprService;
        readonly DaprStateAttribute attr;

        public DaprSaveStateAsyncCollector(DaprStateAttribute attr, DaprServiceClient daprService)
        {
            this.attr = attr;
            this.daprService = daprService;
        }

        public Task AddAsync(SaveStateParameters item, CancellationToken cancellationToken = default)
        {
            if (item.StateStore == null)
            {
                item.StateStore = this.attr.StateStore;
            }

            if (item.Key == null)
            {
                item.Key = this.attr.Key;
            }

            this.requests.Enqueue(item);
            return Task.CompletedTask;
        }

        // TODO: Optimize this method to minimize the number of SaveStateAsync calls
        public async Task FlushAsync(CancellationToken cancellationToken = default)
        {
            while (this.requests.TryDequeue(out SaveStateParameters item))
            {
                var stateList = new[] { new StateContent(item.Key, item.Value) };

                await this.daprService.SaveStateAsync(this.attr.DaprAddress, item.StateStore, stateList);
            }
        }
    }
}