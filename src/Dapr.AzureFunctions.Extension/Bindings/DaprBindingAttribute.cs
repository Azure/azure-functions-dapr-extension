// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// ------------------------------------------------------------

namespace Dapr.AzureFunctions.Extension
{
    using System;
    using Microsoft.Azure.WebJobs.Description;

    /// <summary>
    /// Attribute to specify parameters for the Dapr Bindings.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
    [Binding]
    public sealed class DaprBindingAttribute : DaprBaseAttribute
    {
        /// <summary>
        /// Gets or sets the configured name of the binding.
        /// </summary>
        [AutoResolve]
        public string? BindingName { get; set; }

        /// <summary>
        /// Gets or sets the configured operation.
        /// </summary>
        [AutoResolve]
        public string? Operation { get; set; }
    }
}