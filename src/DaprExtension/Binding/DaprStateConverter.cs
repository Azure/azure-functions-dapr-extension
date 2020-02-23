// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Azure.WebJobs.Extensions.Dapr
{
    internal class DaprStateConverter : IAsyncConverter<DaprStateAttribute, byte[]>,
        IAsyncConverter<DaprStateAttribute, string>,
        IAsyncConverter<DaprStateAttribute, Stream>
    {
        private readonly DaprService _daprService;

        public DaprStateConverter(DaprService daprService)
        {
            _daprService = daprService;
        }

        public Task<byte[]> ConvertAsync(DaprStateAttribute input, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

        Task<string> IAsyncConverter<DaprStateAttribute, string>.ConvertAsync(DaprStateAttribute input, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

        Task<Stream> IAsyncConverter<DaprStateAttribute, Stream>.ConvertAsync(DaprStateAttribute input, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }
    }
}