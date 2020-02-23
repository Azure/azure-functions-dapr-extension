// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Azure.WebJobs.Extensions.Dapr
{
    internal class DaprSaveStateAsyncCollector : IAsyncCollector<SaveStateOptions>
    {
        private DaprStateAttribute attr;

        public DaprSaveStateAsyncCollector(DaprStateAttribute attr)
        {
            this.attr = attr;
        }

        public Task AddAsync(SaveStateOptions item, CancellationToken cancellationToken = default)
        {
            throw new System.NotImplementedException();
        }

        public Task FlushAsync(CancellationToken cancellationToken = default)
        {
            throw new System.NotImplementedException();
        }
    }
}