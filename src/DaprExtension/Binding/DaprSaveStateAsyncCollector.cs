// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Microsoft.Azure.WebJobs.Extensions.Dapr
{
    internal class DaprSaveStateAsyncCollector : IAsyncCollector<SaveStateOptions>
    {
        private readonly ConcurrentQueue<SaveStateOptions> _requests = new ConcurrentQueue<SaveStateOptions>();
        private readonly DaprService _daprService;
        private DaprStateAttribute attr;

        public DaprSaveStateAsyncCollector(DaprStateAttribute attr, DaprService daprService)
        {
            this.attr = attr;
            _daprService = daprService;
        }

        public Task AddAsync(SaveStateOptions item, CancellationToken cancellationToken = default)
        {
            if(item.StateStore == null)
            {
                item.StateStore = attr.StateStore;
            }

            if(item.Key == null)
            {
                item.Key = attr.Key;
            }

            _requests.Enqueue(item);
            return Task.CompletedTask;
        }

        public async Task FlushAsync(CancellationToken cancellationToken = default)
        {
            while (_requests.TryDequeue(out SaveStateOptions item))
            {
                var stateContent = new StateContent() {
                    Key = item.Key,
                    Value = item.Value
                };
                // this create will initiate the send operation
                await _daprService.SaveStateAsync(attr.DaprAddress, item.StateStore, stateContent);
            }
        }
    }

    internal class StateContent 
    {
        public string Key { get; set; }
        public JToken Value { get; set; }
    }
}