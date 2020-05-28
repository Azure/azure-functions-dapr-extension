// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// ------------------------------------------------------------

namespace Microsoft.Azure.WebJobs.Extensions.Dapr
{
    using System;
    using Microsoft.Azure.WebJobs.Description;

    /// <summary>
    /// Trigger attribute used for Dapr actor functions.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter)]
#pragma warning disable CS0618 // Type or member is obsolete
    [Binding(TriggerHandlesReturnValue = true)]
#pragma warning restore CS0618 // Type or member is obsolete
    public class DaprBindingTriggerAttribute : Attribute
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
