// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Microsoft.Azure.WebJobs.Extensions.Dapr
{
    class DaprStateConverter :
        IAsyncConverter<DaprStateAttribute, byte[]>,
        IAsyncConverter<DaprStateAttribute, string>,
        IAsyncConverter<DaprStateAttribute, Stream>,
        IAsyncConverter<DaprStateAttribute, JToken>,
        IAsyncConverter<DaprStateAttribute, JObject>
    {
        readonly DaprServiceClient daprService;

        public DaprStateConverter(DaprServiceClient daprService)
        {
            this.daprService = daprService;
        }

        // TODO: Review this - it doesn't seem right to assume JSON-formatted string content when converting
        //       to a byte array. We probably need to return the stream bytes directly.
        //       More discussion: https://github.com/dapr/dapr/issues/235
        public async Task<byte[]> ConvertAsync(DaprStateAttribute input, CancellationToken cancellationToken)
        {
            var content = await this.GetStringContentAsync(input, cancellationToken);
            var json = JToken.Parse(content);
            byte[] bytes;

            try
            {
                bytes = json.ToObject<byte[]>();
            }
            catch (Exception)
            {
                bytes = Encoding.UTF8.GetBytes(json.ToString());
            }

            return bytes;
        }

        async Task<string> IAsyncConverter<DaprStateAttribute, string>.ConvertAsync(DaprStateAttribute input, CancellationToken cancellationToken)
        {
            return await this.GetStringContentAsync(input, cancellationToken);
        }

        async Task<Stream> IAsyncConverter<DaprStateAttribute, Stream>.ConvertAsync(DaprStateAttribute input, CancellationToken cancellationToken)
        {
            var stateStream = await this.daprService.GetStateAsync(input.DaprAddress, input.StateStore, input.Key);
            return stateStream;
        }

        async Task<JToken> IAsyncConverter<DaprStateAttribute, JToken>.ConvertAsync(DaprStateAttribute input, CancellationToken cancellationToken)
        {
            var content = await this.GetStringContentAsync(input, cancellationToken);
            return JToken.Parse(content);
        }

        async Task<JObject> IAsyncConverter<DaprStateAttribute, JObject>.ConvertAsync(DaprStateAttribute input, CancellationToken cancellationToken)
        {
            var content = await this.GetStringContentAsync(input, cancellationToken);
            return JObject.Parse(content);
        }

        async Task<string> GetStringContentAsync(DaprStateAttribute input, CancellationToken cancellationToken)
        {
            var stateStream = await this.daprService.GetStateAsync(input.DaprAddress, input.StateStore, input.Key);
            StreamReader sr = new StreamReader(stateStream);
            return await sr.ReadToEndAsync();
        }
    }
}