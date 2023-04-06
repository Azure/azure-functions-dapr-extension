// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// ------------------------------------------------------------

namespace Microsoft.Azure.WebJobs.Extensions.Dapr
{
    using System;
    using System.Collections.Concurrent;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.Dapr.Services;

    class DaprInvokeMethodAsyncCollector : IAsyncCollector<InvokeMethodParameters>
    {
        readonly ConcurrentQueue<InvokeMethodParameters> requests = new ConcurrentQueue<InvokeMethodParameters>();
        readonly DaprInvokeAttribute attr;
        readonly DaprServiceClient daprService;

        public DaprInvokeMethodAsyncCollector(DaprInvokeAttribute attr, DaprServiceClient daprService)
        {
            this.attr = attr;
            this.daprService = daprService;
        }

        public Task AddAsync(InvokeMethodParameters item, CancellationToken cancellationToken = default)
        {
            if (item.AppId == null)
            {
                item.AppId = this.attr.AppId ?? throw new ArgumentException("A non-null app ID must be specified.");
            }

            if (item.MethodName == null)
            {
                item.MethodName = this.attr.MethodName ?? throw new ArgumentException("A non-null method name must be specified.");
            }

            if (item.HttpVerb == null)
            {
                item.HttpVerb = this.attr.HttpVerb ?? throw new ArgumentException("A non-null method verb must be specified.");
            }

            this.requests.Enqueue(item);
            return Task.CompletedTask;
        }

        public async Task FlushAsync(CancellationToken cancellationToken = default)
        {
            while (this.requests.TryDequeue(out InvokeMethodParameters item))
            {
                await this.daprService.InvokeMethodAsync(
                    this.attr.DaprAddress,
                    item.AppId!,
                    item.MethodName!,
                    item.HttpVerb,
                    item.Body,
                    cancellationToken);
            }
        }
    }
}