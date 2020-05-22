// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// ------------------------------------------------------------

namespace Microsoft.Azure.WebJobs.Extensions.Dapr
{
    using System;
    using Microsoft.Azure.WebJobs.Description;

    /// <summary>
    /// Trigger attribute used for Dapr service invocation.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter)]
#pragma warning disable CS0618 // Type or member is obsolete
    [Binding(TriggerHandlesReturnValue = true)]
#pragma warning restore CS0618 // Type or member is obsolete
    public class DaprServiceInvocationTriggerAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the name of the method on a remote Dapr App.
        /// </summary>
        /// <remarks>
        /// If not specified, the name of the function is used as the method name.
        /// </remarks>
        public string? MethodName { get; set; }
    }
}
