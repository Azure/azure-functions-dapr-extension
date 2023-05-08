// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// ------------------------------------------------------------

namespace Microsoft.Azure.WebJobs.Extensions.Dapr
{
    using System;
    using System.IO;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using Microsoft.Azure.Functions.Extensions.Dapr.Core.Utils;
    using Microsoft.Azure.WebJobs.Extensions.Dapr.Utils;

    /// <summary>
    /// The parameters for a Dapr save-state operation.
    /// </summary>
    public class DaprStateRecord
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DaprStateRecord"/> class.
        /// </summary>
        /// <param name="key">The key of the state record.</param>
        /// <param name="value">The value of the state record.</param>
        /// <param name="eTag">The state record eTag (optional).</param>
        public DaprStateRecord(string key, object value, string? eTag = null)
        {
            this.Key = key ?? throw new ArgumentNullException(nameof(key));
            this.Value = JsonDocument.Parse(JsonSerializer.Serialize(value, JsonUtils.DefaultSerializerOptions)).RootElement;
            this.ETag = eTag;
        }

        // Internal constructor used only by the binding code.
        internal DaprStateRecord(object value)
        {
            if (value.GetType().Name == "Byte[]")
            {
                var data = (byte[])value;

                var stringData = System.Text.Encoding.UTF8.GetString(data, 0, data.Length);

                try
                {
                    this.Value = JsonDocument.Parse(stringData).RootElement;
                }
                catch (JsonException)
                {
                    this.Value = JsonDocument.Parse("\"" + stringData + "\"").RootElement;
                }

                return;
            }

            this.Value = JsonDocument.Parse(JsonSerializer.Serialize(value, JsonUtils.DefaultSerializerOptions)).RootElement;
        }

        internal DaprStateRecord(string key, Stream valueStream, string? eTag)
        {
            this.Key = key;
            this.ContentStream = valueStream;
            this.ETag = eTag;
        }

        /// <summary>
        /// Gets the key of the state record.
        /// </summary>
        [JsonPropertyName("key")]
        public string? Key { get; internal set; }

        /// <summary>
        /// Gets the value of the state record.
        /// </summary>
        [JsonPropertyName("value")]
        [JsonConverter(typeof(JsonUtils.JsonElementConverter))]
        public JsonElement? Value { get; internal set; }

        /// <summary>
        /// Gets the etag value of the state record.
        /// </summary>
        [JsonPropertyName("etag")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? ETag { get; }

        // Populated when reading state from the dapr state store.
        internal Stream ContentStream { get; set; } = Stream.Null;
    }
}