// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// ------------------------------------------------------------

namespace Dapr.AzureFunctions.Extension
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
        [AutoResolve]
        public string? Topic { get; set; }
    }
}
