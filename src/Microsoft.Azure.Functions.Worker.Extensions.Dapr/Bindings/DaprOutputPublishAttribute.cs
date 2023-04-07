// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// ------------------------------------------------------------

namespace Microsoft.Azure.Functions.Worker.Extensions.Dapr
{
    using System;

    /// <summary>
    /// Attribute to specify parameters for the Dapr publish output binding.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
    public class DaprOutputPublishAttribute : DaprOutputBaseAttribute
    {
        /// <summary>
        /// Gets or sets the pub/sub name to publish to.
        /// </summary>
        public string? PubSubName { get; set; }

        /// <summary>
        /// Gets or sets the name of the topic to publish to.
        /// </summary>
        public string? Topic { get; set; }
    }
}