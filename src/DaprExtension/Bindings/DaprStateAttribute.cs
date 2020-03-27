// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.Azure.WebJobs.Extensions.Dapr
{
    using System;
    using Microsoft.Azure.WebJobs.Description;

    /// <summary>
    /// Attribute to specify parameters for the dapr-state output binding.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
    [Binding]
    public sealed class DaprStateAttribute : DaprBaseAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DaprStateAttribute"/> class.
        /// </summary>
        /// <param name="stateStore">The name of the state store.</param>
        public DaprStateAttribute(string stateStore)
        {
            this.StateStore = stateStore ?? throw new ArgumentNullException(nameof(stateStore));
        }

        /// <summary>
        /// Gets or sets the name of the state store to retrieve or store state.
        /// Is found in the `metadata.name` of the component.
        /// </summary>
        [AutoResolve]
        public string? StateStore { get; set; }

        /// <summary>
        /// Gets or sets the key name to get or set state.
        /// </summary>
        [AutoResolve]
        public string? Key { get; set; }
    }
}
