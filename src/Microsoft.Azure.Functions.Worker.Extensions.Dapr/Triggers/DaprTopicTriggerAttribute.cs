// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// ------------------------------------------------------------

namespace Microsoft.Azure.Functions.Worker.Extensions.Dapr
{
    using System;
    using Microsoft.Azure.Functions.Worker.Extensions.Abstractions;

    /// <summary>
    /// Trigger attribute used for Dapr pub/sub topics.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter)]
    public class DaprTopicTriggerAttribute : TriggerBindingAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DaprTopicTriggerAttribute"/> class.
        /// </summary>
        /// <param name="pubSubName">The pub/sub name.</param>
        public DaprTopicTriggerAttribute(string pubSubName)
        {
            if (pubSubName is null)
            {
                throw new ArgumentNullException(nameof(pubSubName));
            }

            this.PubSubName = pubSubName;
        }

        /// <summary>
        /// Gets the pub/sub name.
        /// </summary>
        public string? PubSubName { get; }

        /// <summary>
        /// Gets or sets the topic. If unspecified the function name will be used.
        /// </summary>
        public string? Topic { get; set; }

        /// <summary>
        /// Gets or sets the route for the trigger. If unspecified the topic name will be used.
        /// </summary>
        public string? Route { get; set; }
    }
}