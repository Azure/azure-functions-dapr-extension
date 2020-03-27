// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.Azure.WebJobs.Extensions.Dapr
{
    using System;
    using Microsoft.Azure.WebJobs.Description;

    /// <summary>
    /// Trigger attribute used for Dapr service methods.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter)]
#pragma warning disable CS0618 // Type or member is obsolete
    [Binding(TriggerHandlesReturnValue = true)]
#pragma warning restore CS0618 // Type or member is obsolete
    public class DaprMethodTriggerAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the name of the Dapr service method.
        /// </summary>
        /// <remarks>
        /// If not specified, the name of the function is used as the method name.
        /// </remarks>
        public string? MethodName { get; set; }
    }
}
