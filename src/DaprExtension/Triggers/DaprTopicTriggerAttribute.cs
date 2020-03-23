// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Azure.WebJobs.Description;

namespace Microsoft.Azure.WebJobs.Extensions.Dapr
{
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
