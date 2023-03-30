// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// ------------------------------------------------------------

namespace Microsoft.Azure.WebJobs.Extension.Dapr
{
    using Newtonsoft.Json;

    /// <summary>
    /// Dapr Topic Subscription returned when Dapr discover the topic subscriptions.
    /// https://docs.dapr.io/reference/api/pubsub_api/#provide-a-route-for-dapr-to-discover-topic-subscriptions.
    /// </summary>
    internal class DaprTopicSubscription
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DaprTopicSubscription"/> class.
        /// </summary>
        /// <param name="pubSubName">The name of the pub/sub.</param>
        /// <param name="topic">The topic of the topic subscription.</param>
        /// <param name="route">The route corresponds to this topic subscription.</param>
        public DaprTopicSubscription(string pubSubName, string topic, string route)
        {
            this.PubSubName = pubSubName;
            this.Topic = topic;
            this.Route = route;
        }

        /// <summary>
        /// Gets the pub/sub name.
        /// </summary>
        [JsonProperty("pubsubname")]
        public string PubSubName { get; }

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