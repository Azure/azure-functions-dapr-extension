// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// ------------------------------------------------------------

namespace Microsoft.Azure.WebJobs.Extensions.Dapr.Utils
{
    using System;
    using System.Text.Encodings.Web;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    /// <summary>
    /// Utility methods for JSON serialization.
    /// </summary>
    public static class JsonUtils
    {
        /// <summary>
        /// Default <see cref="JsonSerializerOptions"/> used for serialization.
        /// </summary>
        public static readonly JsonSerializerOptions DefaultSerializerOptions = new JsonSerializerOptions()
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        };

        /// <summary>
        /// A <see cref="JsonConverter{T}"/> for <see cref="JsonElement"/>.
        /// </summary>
        public class JsonElementConverter : JsonConverter<JsonElement>
        {
            /// <inheritdoc/>
            public override JsonElement Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                using var document = JsonDocument.ParseValue(ref reader);
                return document.RootElement.Clone();
            }

            /// <inheritdoc/>
            public override void Write(Utf8JsonWriter writer, JsonElement value, JsonSerializerOptions options)
            {
                JsonSerializer.Serialize(writer, value, JsonUtils.DefaultSerializerOptions);
            }
        }
    }
}