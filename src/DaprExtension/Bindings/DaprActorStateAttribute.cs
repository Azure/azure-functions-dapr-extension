// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.Azure.WebJobs.Extensions.Dapr
{
    using System;
    using Microsoft.Azure.WebJobs.Description;

    /// <summary>
    /// Parameter attribute for the Dapr actor state input binding.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
    [Binding]
    public sealed class DaprActorStateAttribute : DaprBaseAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DaprActorStateAttribute"/> class.
        /// </summary>
        /// <param name="actorType">The actor type to get the state from.</param>
        /// <param name="actorId">The actor id to get the state from.</param>
        public DaprActorStateAttribute(string actorType, string actorId)
        {
            this.ActorType = actorType ?? throw new ArgumentNullException(nameof(actorType));
            this.ActorId = actorId ?? throw new ArgumentNullException(nameof(actorId));
        }

        /// <summary>
        /// Gets the type of the Dapr actor.
        /// </summary>
        [AutoResolve]
        public string ActorType { get; private set; }

        /// <summary>
        /// Gets the id of the Dapr actor.
        /// </summary>
        [AutoResolve]
        public string ActorId { get; private set; }

        /// <summary>
        /// Gets or sets the key for the state of an Dapr actor.
        /// </summary>
        [AutoResolve]
        public string? Key { get; set; }
    }
}
