// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Azure.WebJobs.Extensions.Dapr
{
    internal class DaprInvokeMethodAsyncCollector : IAsyncCollector<InvokeMethodOptions>
    {
        private readonly ConcurrentQueue<InvokeMethodOptions> _requests = new ConcurrentQueue<InvokeMethodOptions>();
        private readonly DaprInvokeAttribute _attr;
        private readonly DaprService _daprService;

        public DaprInvokeMethodAsyncCollector(DaprInvokeAttribute attr, DaprService daprService)
        {
            _attr = attr;
            _daprService = daprService;
        }

        public Task AddAsync(InvokeMethodOptions item, CancellationToken cancellationToken = default)
        {
            if(item.AppId == null)
            {
                item.AppId = _attr.AppId;
            }

            if(item.MethodName == null)
            {
                item.MethodName = _attr.MethodName;
            }

            if(item.HttpVerb == null)
            {
                item.HttpVerb = _attr.HttpVerb;
            }

            _requests.Enqueue(item);
            return Task.CompletedTask;
        }

        public async Task FlushAsync(CancellationToken cancellationToken = default)
        {
            while (_requests.TryDequeue(out InvokeMethodOptions item))
            {
                await _daprService.InvokeMethodAsync(_attr.DaprAddress, item.AppId, item.MethodName, item.HttpVerb, item.Body);
            }
        }
    }
}