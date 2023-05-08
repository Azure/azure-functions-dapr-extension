﻿// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// ------------------------------------------------------------

namespace Microsoft.Azure.Functions.Worker.Extensions.Dapr
{
    using System;

    /// <summary>
    /// Attribute to specify parameters for the dapr state input binding.
    /// </summary>
    public class DaprStateInputAttribute : DaprBaseInputAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DaprStateInputAttribute"/> class.
        /// </summary>
        /// <param name="stateStore">The name of the state store.</param>
        public DaprStateInputAttribute(string stateStore)
        {
            this.StateStore = stateStore ?? throw new ArgumentNullException(nameof(stateStore));
        }

        /// <summary>
        /// Gets or sets the name of the state store to retrieve or store state.
        /// Is found in the `metadata.name` of the component.
        /// </summary>
        public string? StateStore { get; set; }

        /// <summary>
        /// Gets or sets the key name to get or set state.
        /// </summary>
        public string? Key { get; set; }
    }
}