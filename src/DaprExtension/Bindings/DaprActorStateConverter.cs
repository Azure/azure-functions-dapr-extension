// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.Azure.WebJobs.Extensions.Dapr
{
    using System;
    using System.IO;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.WebJobs;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    class DaprActorStateConverter :
        IAsyncConverter<DaprActorStateAttribute, string>,
        IAsyncConverter<DaprActorStateAttribute, JObject>,
        IAsyncConverter<DaprActorStateAttribute, DaprActorStateRecord>,
        IAsyncConverter<DaprActorStateAttribute, JToken>,
        IAsyncConverter<DaprActorStateAttribute, Stream>,
        IAsyncConverter<DaprActorStateAttribute, byte[]>,
        IAsyncConverter<DaprActorStateAttribute, object?>
    {
        readonly DaprServiceClient daprClient;

        public DaprActorStateConverter(DaprServiceClient daprClient)
        {
            this.daprClient = daprClient;
        }

        public async Task<byte[]> ConvertAsync(DaprActorStateAttribute input, CancellationToken cancellationToken)
        {
            string content = await this.GetStringContentAsync(input, cancellationToken);
            if (string.IsNullOrEmpty(content))
            {
                return Array.Empty<byte>();
            }

            // Per Yaron, Dapr only supports JSON payloads over HTTP.
            // By default we assume that the payload is a JSON-serialized base64 string of bytes
            JToken json = JToken.Parse(content);
            byte[] bytes;

            try
            {
                bytes = json.ToObject<byte[]>();
            }
            catch (JsonException)
            {
                // Looks like it's not actually JSON - just return the raw bytes
                bytes = Encoding.UTF8.GetBytes(json.ToString());
            }

            return bytes;
        }

        async Task<JObject> IAsyncConverter<DaprActorStateAttribute, JObject>.ConvertAsync(
            DaprActorStateAttribute input,
            CancellationToken cancellationToken)
        {
            string content = await this.GetStringContentAsync(input, cancellationToken);
            return JObject.Parse(content);
        }

        async Task<object?> IAsyncConverter<DaprActorStateAttribute, object?>.ConvertAsync(
            DaprActorStateAttribute input,
            CancellationToken cancellationToken)
        {
            string content = await this.GetStringContentAsync(input, cancellationToken);
            if (string.IsNullOrEmpty(content))
            {
                return null; // TODO: This will cause a null-ref for value types!
            }
            else
            {
                return JToken.Parse(content);
            }
        }

        async Task<Stream> IAsyncConverter<DaprActorStateAttribute, Stream>.ConvertAsync(
            DaprActorStateAttribute input,
            CancellationToken cancellationToken)
        {
            DaprActorStateRecord record = await this.GetActorStateAsync(input, cancellationToken);
            return record.ContentStream;
        }

        async Task<JToken> IAsyncConverter<DaprActorStateAttribute, JToken>.ConvertAsync(
            DaprActorStateAttribute input,
            CancellationToken cancellationToken)
        {
            string content = await this.GetStringContentAsync(input, cancellationToken);
            return JToken.Parse(content);
        }

        Task<string> IAsyncConverter<DaprActorStateAttribute, string>.ConvertAsync(
            DaprActorStateAttribute input,
            CancellationToken cancellationToken)
        {
            return this.GetStringContentAsync(input, cancellationToken);
        }

        async Task<DaprActorStateRecord> IAsyncConverter<DaprActorStateAttribute, DaprActorStateRecord>.ConvertAsync(
            DaprActorStateAttribute input,
            CancellationToken cancellationToken)
        {
            DaprActorStateRecord record = await this.GetActorStateAsync(input, cancellationToken);
            using StreamReader reader = new StreamReader(record.ContentStream);
            string content = await this.GetStringContentAsync(input, cancellationToken);
            if (!string.IsNullOrEmpty(content))
            {
                record.Value = JToken.Parse(content);
            }

            return record;
        }

        async Task<string> GetStringContentAsync(DaprActorStateAttribute input, CancellationToken cancellationToken)
        {
            DaprActorStateRecord stateRecord = await this.GetActorStateAsync(input, cancellationToken);
            using StreamReader reader = new StreamReader(stateRecord.ContentStream);
            var stringContent = await reader.ReadToEndAsync();
            return stringContent;
        }

        Task<DaprActorStateRecord> GetActorStateAsync(DaprActorStateAttribute input, CancellationToken cancellationToken)
        {
            return this.daprClient.GetActorStateAsync(
                input.DaprAddress,
                input.ActorType ?? throw new ArgumentException("No actor type was specified.", nameof(input.ActorType)),
                input.ActorId ?? throw new ArgumentException("No actor id key was specified.", nameof(input.ActorId)),
                input.Key ?? throw new ArgumentException("No actor state key was specified.", nameof(input.Key)),
                cancellationToken);
        }
    }
}