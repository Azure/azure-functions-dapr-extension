// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// ------------------------------------------------------------

namespace Microsoft.Azure.WebJobs.Extension.Dapr
{
    using System;
    using System.Text.Encodings.Web;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    internal class JsonElementConverter : JsonConverter<JsonElement>
    {
        private static readonly JsonSerializerOptions SerializerOptions = new JsonSerializerOptions()
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        };

        /// <inheritdoc/>
        public override JsonElement Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using var document = JsonDocument.ParseValue(ref reader);
            return document.RootElement.Clone();
        }

        /// <inheritdoc/>
        public override void Write(Utf8JsonWriter writer, JsonElement value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, value, SerializerOptions);
        }
    }
}