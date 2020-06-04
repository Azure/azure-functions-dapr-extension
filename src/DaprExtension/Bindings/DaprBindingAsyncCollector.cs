// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// ------------------------------------------------------------

namespace Dapr.AzureFunctions.Extension
{
    using System;
    using System.Collections.Concurrent;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.WebJobs;

    class DaprBindingAsyncCollector : IAsyncCollector<DaprBindingMessage>
    {
        readonly ConcurrentQueue<DaprBindingMessage> requests = new ConcurrentQueue<DaprBindingMessage>();
        readonly DaprBindingAttribute attr;
        readonly DaprServiceClient daprService;

        public DaprBindingAsyncCollector(DaprBindingAttribute attr, DaprServiceClient daprService)
        {
            this.attr = attr;
            this.daprService = daprService;
        }

        public Task AddAsync(DaprBindingMessage item, CancellationToken cancellationToken = default)
        {
            if (item.BindingName == null)
            {
                item.BindingName = this.attr.BindingName ?? throw new ArgumentException("A non-null binding name must be specified.");
            }

            this.requests.Enqueue(item);
            return Task.CompletedTask;
        }

        public async Task FlushAsync(CancellationToken cancellationToken = default)
        {
            while (this.requests.TryDequeue(out DaprBindingMessage item))
            {
                await this.daprService.SendToDaprBindingAsync(
                    this.attr.DaprAddress,
                    item!,
                    cancellationToken);
            }
        }
    }
}