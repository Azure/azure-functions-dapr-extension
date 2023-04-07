// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// ------------------------------------------------------------

namespace Microsoft.Azure.Functions.Worker.Extensions.Dapr
{
    using System;
    using Microsoft.Azure.Functions.Worker.Extensions.Abstractions;

    /// <summary>
    /// Trigger attribute used for Dapr actor functions.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter)]
    public class DaprBindingTriggerAttribute : TriggerBindingAttribute
    {
        /// <summary>
        /// Gets or sets the name of the Dapr trigger.
        /// </summary>
        /// <remarks>
        /// If not specified, the name of the function is used as the trigger name.
        /// </remarks>
        public string? BindingName { get; set; }
    }
}