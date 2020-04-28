// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.Azure.WebJobs.Extensions.Dapr
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
    }
}
