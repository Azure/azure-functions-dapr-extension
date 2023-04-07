﻿// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// ------------------------------------------------------------

namespace Microsoft.Azure.Functions.Worker.Extensions.Dapr
{
    using System;

    /// <summary>
    /// Attribute to specify parameters for the Dapr output bindings.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
    public class DaprOutputBindingAttribute : DaprOutputBaseAttribute
    {
        /// <summary>
        /// Gets or sets the configured name of the binding.
        /// </summary>
        public string? BindingName { get; set; }

        /// <summary>
        /// Gets or sets the configured operation.
        /// </summary>
        public string? Operation { get; set; }
    }
}