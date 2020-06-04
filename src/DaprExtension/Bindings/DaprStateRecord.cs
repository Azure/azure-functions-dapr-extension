// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// ------------------------------------------------------------

namespace Dapr.AzureFunctions.Extension
{
    using System;
    using System.IO;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

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
        public DaprStateRecord(string key, JToken? value, string? eTag = null)
        {
            this.Key = key ?? throw new ArgumentNullException(nameof(key));
            this.Value = value;
            this.ETag = eTag;
        }

        // Internal constructor used only by the binding code.
        internal DaprStateRecord(JToken? value)
        {
            this.Value = value;
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
        [JsonProperty("key")]
        public string? Key { get; internal set; }

        /// <summary>
        /// Gets the value of the state record.
        /// </summary>
        [JsonProperty("value")]
        public JToken? Value { get; internal set; }

        /// <summary>
        /// Gets the etag value of the state record.
        /// </summary>
        [JsonProperty("etag", NullValueHandling = NullValueHandling.Ignore)]
        public string? ETag { get; }

        // Populated when reading state from the dapr state store.
        internal Stream ContentStream { get; set; } = Stream.Null;
    }
}