// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// ------------------------------------------------------------

namespace Microsoft.Azure.WebJobs.Extensions.Dapr.Bindings.Converters
{
    using System;
    using System.IO;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Functions.Extensions.Dapr.Core.Utils;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.Dapr.Services;
    using Newtonsoft.Json.Linq;

    class DaprStateConverter :
        IAsyncConverter<DaprStateAttribute, DaprStateRecord>,
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