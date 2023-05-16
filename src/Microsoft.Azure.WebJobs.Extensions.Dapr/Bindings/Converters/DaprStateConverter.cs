// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// ------------------------------------------------------------

namespace Microsoft.Azure.WebJobs.Extensions.Dapr.Bindings.Converters
{
    using System;
    using System.IO;
    using System.Net;
    using System.Text;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Functions.Extensions.Dapr.Core.Utils;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.Dapr.Exceptions;
    using Microsoft.Azure.WebJobs.Extensions.Dapr.Services;
    using Microsoft.Azure.WebJobs.Extensions.Dapr.Utils;
    using Newtonsoft.Json.Linq;

    class DaprStateConverter :
        IAsyncConverter<DaprStateAttribute, DaprStateRecord>,
        IAsyncConverter<DaprStateAttribute, byte[]>,
        IAsyncConverter<DaprStateAttribute, string>,
        IAsyncConverter<DaprStateAttribute, Stream>,
        IAsyncConverter<DaprStateAttribute, JsonElement>,
        IAsyncConverter<DaprStateAttribute, JObject>,
        IAsyncConverter<DaprStateAttribute, JToken>
    {
        readonly IDaprServiceClient daprClient;

        public DaprStateConverter(IDaprServiceClient daprClient)
        {
            this.daprClient = daprClient;
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
                record.Value = JsonDocument.Parse(content).RootElement;
            }

            return record;
        }

        public async Task<byte[]> ConvertAsync(
            DaprStateAttribute input,
            CancellationToken cancellationToken)
        {
            string content = await this.GetStringContentAsync(input, cancellationToken);
            if (string.IsNullOrEmpty(content))
            {
                return Array.Empty<byte>();
            }

            // Per Yaron, Dapr only supports JSON payloads over HTTP.
            // By default we assume that the payload is a JSON-serialized base64 string of bytes
            JsonElement json = JsonDocument.Parse(content).RootElement;
            byte[]? bytes;

            try
            {
                bytes = JsonSerializer.Deserialize<byte[]>(json);
            }
            catch (JsonException)
            {
                // Looks like it's not actually JSON - just return the raw bytes
                bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(json, JsonUtils.DefaultSerializerOptions));
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

        async Task<JsonElement> IAsyncConverter<DaprStateAttribute, JsonElement>.ConvertAsync(
            DaprStateAttribute input,
            CancellationToken cancellationToken)
        {
            string content = await this.GetStringContentAsync(input, cancellationToken);
            if (string.IsNullOrEmpty(content))
            {
                return default;
            }

            return JsonDocument.Parse(content).RootElement;
        }

        async Task<JObject> IAsyncConverter<DaprStateAttribute, JObject>.ConvertAsync(
            DaprStateAttribute input,
            CancellationToken cancellationToken)
        {
            string content = await this.GetStringContentAsync(input, cancellationToken);
            if (string.IsNullOrEmpty(content))
            {
                return default!;
            }

            return JObject.Parse(content);
        }

        async Task<JToken> IAsyncConverter<DaprStateAttribute, JToken>.ConvertAsync(
            DaprStateAttribute input,
            CancellationToken cancellationToken)
        {
            string content = await this.GetStringContentAsync(input, cancellationToken);
            if (string.IsNullOrEmpty(content))
            {
                return default!;
            }

            return JToken.Parse(content);
        }

        private async Task<string> GetStringContentAsync(DaprStateAttribute input, CancellationToken cancellationToken)
        {
            DaprStateRecord stateRecord = await this.GetStateRecordAsync(input, cancellationToken);

            if (stateRecord.ContentStream.Length == 0)
            {
                throw new DaprException(
                HttpStatusCode.NotFound,
                ErrorCodes.ErrNoContent,
                $"Failed getting state with key {input.Key} from state store {input.StateStore}: state {input.Key} not found.");
            }

            var contentJson = await JsonDocument.ParseAsync(stateRecord.ContentStream);
            return JsonSerializer.Serialize(contentJson, JsonUtils.DefaultSerializerOptions);
        }

        private async Task<DaprStateRecord> GetStateRecordAsync(DaprStateAttribute input, CancellationToken cancellationToken)
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