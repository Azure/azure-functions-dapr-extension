// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// ------------------------------------------------------------

namespace Microsoft.Azure.WebJobs.Extension.Dapr
{
    using System;
    using System.IO;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.WebJobs;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    class DaprStateConverter :
        IAsyncConverter<DaprStateAttribute, DaprStateRecord>,
        IAsyncConverter<DaprStateAttribute, byte[]>,
        IAsyncConverter<DaprStateAttribute, string>,
        IAsyncConverter<DaprStateAttribute, Stream>,
        IAsyncConverter<DaprStateAttribute, JToken>,
        IAsyncConverter<DaprStateAttribute, JObject>,
        IAsyncConverter<DaprStateAttribute, object?>
    {
        readonly DaprServiceClient daprClient;

        public DaprStateConverter(DaprServiceClient daprClient)
        {
            this.daprClient = daprClient;
        }

        public async Task<byte[]> ConvertAsync(DaprStateAttribute input, CancellationToken cancellationToken)
        {
            string content = await this.GetStringContentAsync(input, cancellationToken);
            if (string.IsNullOrEmpty(content))
            {
                return Array.Empty<byte>();
            }

            // Per Yaron, Dapr only supports JSON payloads over HTTP.
            // By default we assume that the payload is a JSON-serialized base64 string of bytes
            JToken json = JToken.Parse(content);
            byte[]? bytes;

            try
            {
                bytes = json.ToObject<byte[]>();
            }
            catch (JsonException)
            {
                // Looks like it's not actually JSON - just return the raw bytes
                bytes = Encoding.UTF8.GetBytes(json.ToString());
            }

            return bytes ?? Array.Empty<byte>();
        }

        Task<string> IAsyncConverter<DaprStateAttribute, string>.ConvertAsync(
            DaprStateAttribute input,
            CancellationToken cancellationToken)
        {
            return this.GetStringContentAsync(input, cancellationToken);
        }

        async Task<Stream> IAsyncConverter<DaprStateAttribute, Stream>.ConvertAsync(
            DaprStateAttribute input,
            CancellationToken cancellationToken)
        {
            DaprStateRecord record = await this.GetStateRecordAsync(input, cancellationToken);
            return record.ContentStream;
        }

        async Task<JToken> IAsyncConverter<DaprStateAttribute, JToken>.ConvertAsync(
            DaprStateAttribute input,
            CancellationToken cancellationToken)
        {
            string content = await this.GetStringContentAsync(input, cancellationToken);
            return JToken.Parse(content);
        }

        async Task<JObject> IAsyncConverter<DaprStateAttribute, JObject>.ConvertAsync(
            DaprStateAttribute input,
            CancellationToken cancellationToken)
        {
            string content = await this.GetStringContentAsync(input, cancellationToken);
            return JObject.Parse(content);
        }

        async Task<object?> IAsyncConverter<DaprStateAttribute, object?>.ConvertAsync(
            DaprStateAttribute input,
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

        async Task<DaprStateRecord> IAsyncConverter<DaprStateAttribute, DaprStateRecord>.ConvertAsync(
            DaprStateAttribute input,
            CancellationToken cancellationToken)
        {
            DaprStateRecord record = await this.GetStateRecordAsync(input, cancellationToken);
            using StreamReader reader = new StreamReader(record.ContentStream);
            string content = await reader.ReadToEndAsync();
            if (!string.IsNullOrEmpty(content))
            {
                record.Value = JToken.Parse(content);
            }

            return record;
        }

        async Task<string> GetStringContentAsync(DaprStateAttribute input, CancellationToken cancellationToken)
        {
            DaprStateRecord stateRecord = await this.GetStateRecordAsync(input, cancellationToken);
            using StreamReader reader = new StreamReader(stateRecord.ContentStream);
            return await reader.ReadToEndAsync();
        }

        async Task<DaprStateRecord> GetStateRecordAsync(DaprStateAttribute input, CancellationToken cancellationToken)
        {
            DaprStateRecord stateRecord = await this.daprClient.GetStateAsync(
                input.DaprAddress,
                input.StateStore ?? throw new ArgumentException("No state store name was specified.", nameof(input.StateStore)),
                input.Key ?? throw new ArgumentException("No state store key was specified.", nameof(input.Key)),
                cancellationToken);
            return stateRecord;
        }
    }
}