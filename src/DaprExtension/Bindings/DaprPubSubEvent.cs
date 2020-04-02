// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.Azure.WebJobs.Extensions.Dapr
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Payload for outbound Dapr pub/sub events.
    /// </summary>
    public class DaprPubSubEvent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DaprPubSubEvent"/> class.
        /// </summary>
        /// <param name="payload">The payload of the outbound pub/sub event.</param>
        /// <param name="topic">The topic of the outbound pub/sub event.</param>
        public DaprPubSubEvent(JToken payload, string? topic = null)
        {
            this.Payload = payload;
            this.Topic = topic;
        }

        /// <summary>
        /// Gets the name of the topic.
        /// </summary>
        /// <remarks>
        /// If the topic name is not specified, it is inferred from the
        /// <see cref="DaprPublishAttribute"/> binding attribute.
        /// </remarks>
        [JsonProperty("topic")]
        public string? Topic { get; internal set; }

        /// <summary>
        /// Gets the payload of the pub/sub event.
        /// </summary>
        /// <remarks>
        /// The subscribers will receive this payload as the body of a Cloud Event envelope.
        /// </remarks>
        [JsonProperty("payload")]
        public JToken Payload { get; }
    }
}
