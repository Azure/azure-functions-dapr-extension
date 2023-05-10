// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// ------------------------------------------------------------

namespace Microsoft.Azure.Functions.Extensions.Dapr.Core
{
    using System;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using Microsoft.Azure.Functions.Extensions.Dapr.Core.Utils;

    /// <summary>
    /// Payload for outbound Dapr pub/sub events.
    /// </summary>
    public class DaprPubSubEvent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DaprPubSubEvent"/> class.
        /// </summary>
        /// <param name="payload">The payload of the outbound pub/sub event.</param>
        /// <param name="pubSubName">The pub/sub name of the outbound pub/sub event.</param>
        /// <param name="topic">The topic of the outbound pub/sub event.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="payload"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="payload"/> is not serializable to JSON.</exception>
        public DaprPubSubEvent(object payload, string? pubSubName = null, string? topic = null)
        {
            if (payload == null)
            {
                throw new ArgumentNullException(nameof(payload));
            }

            // TODO: This is a temporary workaround for converter, where isolated worker is sending byte[] instead of actual type defined in azure functions.
            // This will be removed once we have a fix for converter
            if (payload.GetType().Name == "Byte[]")
            {
                var data = (byte[])payload;

                var stringData = System.Text.Encoding.UTF8.GetString(data, 0, data.Length);

                try
                {
                    this.Payload = JsonDocument.Parse(stringData).RootElement;
                }
                catch (JsonException)
                {
                    this.Payload = JsonDocument.Parse("\"" + stringData + "\"").RootElement;
                }

                return;
            }

            string serializedData = string.Empty;
            try
            {
                serializedData = JsonSerializer.Serialize(payload, JsonUtils.DefaultSerializerOptions);
            }
            catch (Exception e)
            {
                throw new ArgumentException("The payload object must be serializable to JSON.", nameof(payload), e);
            }

            this.Payload = JsonDocument.Parse(serializedData).RootElement;
            this.PubSubName = pubSubName;
            this.Topic = topic;
        }

        /// <summary>
        /// Gets or sets the name of the pub/sub.
        /// </summary>
        /// <remarks>
        /// If the pub/sub name is not specified, it is inferred from the DaprPublishOutput attribute.
        /// </remarks>
        [JsonPropertyName("pubsubname")]
        public string? PubSubName { get; set; }

        /// <summary>
        /// Gets or sets the name of the topic.
        /// </summary>
        /// <remarks>
        /// If the topic name is not specified, it is inferred from the DaprPublishOutput attribute.
        /// </remarks>
        [JsonPropertyName("topic")]
        public string? Topic { get; set; }

        /// <summary>
        /// Gets the payload of the pub/sub event.
        /// </summary>
        /// <remarks>
        /// The subscribers will receive this payload as the body of a Cloud Event envelope.
        /// </remarks>
        [JsonPropertyName("payload")]
        [JsonConverter(typeof(JsonUtils.JsonElementConverter))]
        public JsonElement Payload { get; }
    }
}