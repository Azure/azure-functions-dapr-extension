// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.IO;
using System.Text;
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

        public async Task<byte[]> ConvertAsync(DaprStateAttribute input, CancellationToken cancellationToken)
        {
            var stateStream = await _daprService.GetStateAsync(input.DaprAddress, input.StateStore, input.Key);
            StreamReader sr = new StreamReader(stateStream);
            return Encoding.UTF8.GetBytes(await sr.ReadToEndAsync());
        }

        async Task<string> IAsyncConverter<DaprStateAttribute, string>.ConvertAsync(DaprStateAttribute input, CancellationToken cancellationToken)
        {
            var stateStream = await _daprService.GetStateAsync(input.DaprAddress, input.StateStore, input.Key);
            StreamReader sr = new StreamReader(stateStream);
            return await sr.ReadToEndAsync();
        }

        async Task<Stream> IAsyncConverter<DaprStateAttribute, Stream>.ConvertAsync(DaprStateAttribute input, CancellationToken cancellationToken)
        {
            var stateStream = await _daprService.GetStateAsync(input.DaprAddress, input.StateStore, input.Key);
            return stateStream;
        }
    }
}