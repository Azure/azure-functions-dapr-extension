// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// ------------------------------------------------------------

namespace Microsoft.Azure.WebJobs.Extensions.Dapr
{
    using System;
    using Microsoft.Azure.WebJobs.Description;

    /// <summary>
    /// Attribute to specify parameters for the Dapr-publish output binding.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
    [Binding]
    public class DaprPublishAttribute : DaprBaseAttribute
    {
        /// <summary>
        /// Gets or sets the name of the topic to publish to.
        /// </summary>
        public string? Topic { get; set; }
    }
}
