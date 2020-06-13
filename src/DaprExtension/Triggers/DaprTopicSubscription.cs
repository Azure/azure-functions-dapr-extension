// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// ------------------------------------------------------------

namespace Dapr.AzureFunctions.Extension
{
    using Newtonsoft.Json;

    /// <summary>
    /// Dapr Topic Subscription returned when Dapr discover the topic subscriptions.
    /// https://github.com/dapr/docs/blob/master/reference/api/pubsub_api.md#provide-a-route-for-dapr-to-discover-topic-subscriptions.
    /// </summary>
    internal class DaprTopicSubscription
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DaprTopicSubscription"/> class.
        /// </summary>
        /// <param name="topic">The name of the topic subscription.</param>
        /// <param name="route">The route corresponds to this topic subscription.</param>
        public DaprTopicSubscription(string topic, string route)
        {
            this.Topic = topic;
            this.Route = route;
        }

        /// <summary>
        /// Gets topic name.
        /// </summary>
        [JsonProperty("topic")]
        public string Topic { get; }

        /// <summary>
        /// Gets topic route.
        /// </summary>
        [JsonProperty("route")]
        public string Route { get; }
    }
}
