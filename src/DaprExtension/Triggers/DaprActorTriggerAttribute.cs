// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

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
    public class DaprActorTriggerAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the type of the Dapr actor.
        /// </summary>
        /// <remarks>
        /// If not specified, the name of the function is used as the actor type name.
        /// </remarks>
        [AutoResolve]
        public string? ActorType { get; set; }
    }
}
