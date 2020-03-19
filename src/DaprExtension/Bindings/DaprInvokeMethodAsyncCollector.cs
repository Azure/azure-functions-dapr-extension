// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Azure.WebJobs.Extensions.Dapr
{
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
                item.AppId = this.attr.AppId;
            }

            if (item.MethodName == null)
            {
                item.MethodName = this.attr.MethodName;
            }

            if (item.HttpVerb == null)
            {
                item.HttpVerb = this.attr.HttpVerb;
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
                    item.AppId,
                    item.MethodName,
                    item.HttpVerb,
                    item.Body);
            }
        }
    }
}