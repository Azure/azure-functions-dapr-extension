// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// ------------------------------------------------------------

namespace Microsoft.Azure.Functions.Worker.Extensions.Dapr
{
    using System;
    using Microsoft.Azure.Functions.Worker.Extensions.Abstractions;

    /// <summary>
    /// Trigger attribute used for Dapr service invocation.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter)]
    public class DaprServiceInvocationTriggerAttribute : TriggerBindingAttribute
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