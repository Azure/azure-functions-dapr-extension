// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// ------------------------------------------------------------

namespace Microsoft.Azure.WebJobs.Extensions.Dapr
{
    using System;
    using Microsoft.Azure.WebJobs.Description;

    /// <summary>
    /// Trigger attribute used for Dapr pub/sub topics.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter)]
    [Binding]
    public class DaprTopicTriggerAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the name of the topic.
        /// </summary>
        public string? TopicName { get; set; }
    }
}
