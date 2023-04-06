// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// ------------------------------------------------------------

namespace Microsoft.Azure.WebJobs.Extensions.Dapr
{
    using System;
    using System.Collections.Generic;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using Microsoft.Azure.WebJobs.Extensions.Dapr.Utils;

    /// <summary>
    /// Parameters for invoking a Dapr binding.
    /// </summary>
    public class DaprBindingMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DaprBindingMessage"/> class.
        /// </summary>
        /// <param name="data">The data of the message sent to the Dapr binding.</param>
        /// <param name="metadata">The bag of key value pairs for binding-specific metadata.</param>
        /// <param name="binding">The name of binding.</param>
        /// <param name="operation">The operation to do with the Dapr binding.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="data"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="data"/> is not serializable to JSON.</exception>
        public DaprBindingMessage(object data, Dictionary<string, object>? metadata = null, string? binding = null, string? operation = null)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            string serializedData = string.Empty;
            try
            {
                serializedData = JsonSerializer.Serialize(data, JsonUtils.DefaultSerializerOptions);
            }
            catch (Exception e)
            {
                throw new ArgumentException("The data object must be serializable to JSON.", nameof(data), e);
            }

            this.Data = JsonDocument.Parse(serializedData).RootElement;
            this.Metadata = metadata;
            this.BindingName = binding;
            this.Operation = operation;
        }

        /// <summary>
        /// Gets or sets the data .
        /// </summary>
        [JsonPropertyName("data")]
        [JsonConverter(typeof(JsonUtils.JsonElementConverter))]
        public JsonElement Data { get; set; }

        /// <summary>
        /// Gets or sets the operation.
        /// </summary>
        [JsonPropertyName("operation")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Operation { get; set; }

        /// <summary>
        /// Gets or sets the metadata required for this operation.
        /// </summary>
        [JsonPropertyName("metadata")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Dictionary<string, object>? Metadata { get; set; }

        /// <summary>
        /// Gets or sets the name of the binding.
        /// </summary>
        [JsonIgnore]
        public string? BindingName { get; set; }
    }
}