// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Microsoft.Azure.WebJobs.Extensions.Dapr
{
    internal class DaprStateConverter : IAsyncConverter<DaprStateAttribute, byte[]>,
        IAsyncConverter<DaprStateAttribute, string>,
        IAsyncConverter<DaprStateAttribute, Stream>,
        IAsyncConverter<DaprStateAttribute, JToken>,
        IAsyncConverter<DaprStateAttribute, JObject>
    {
        private readonly DaprService _daprService;

        public DaprStateConverter(DaprService daprService)
        {
            _daprService = daprService;
        }

        public async Task<byte[]> ConvertAsync(DaprStateAttribute input, CancellationToken cancellationToken)
        {
            var content = await GetStringContentAsync(input, cancellationToken);
            return Encoding.UTF8.GetBytes(content);
        }

        async Task<string> IAsyncConverter<DaprStateAttribute, string>.ConvertAsync(DaprStateAttribute input, CancellationToken cancellationToken)
        {
            return await GetStringContentAsync(input, cancellationToken);
        }

        async Task<Stream> IAsyncConverter<DaprStateAttribute, Stream>.ConvertAsync(DaprStateAttribute input, CancellationToken cancellationToken)
        {
            var stateStream = await _daprService.GetStateAsync(input.DaprAddress, input.StateStore, input.Key);
            return stateStream;
        }

        async Task<JToken> IAsyncConverter<DaprStateAttribute, JToken>.ConvertAsync(DaprStateAttribute input, CancellationToken cancellationToken)
        {
            var content = await GetStringContentAsync(input, cancellationToken);
            return JToken.Parse(content);
        }

        async Task<JObject> IAsyncConverter<DaprStateAttribute, JObject>.ConvertAsync(DaprStateAttribute input, CancellationToken cancellationToken)
        {
            var content = await GetStringContentAsync(input, cancellationToken);
            return JObject.Parse(content);
        }

        async Task<string> GetStringContentAsync(DaprStateAttribute input, CancellationToken cancellationToken)
        {
            var stateStream = await _daprService.GetStateAsync(input.DaprAddress, input.StateStore, input.Key);
            StreamReader sr = new StreamReader(stateStream);
            return await sr.ReadToEndAsync();
        }
    }
}