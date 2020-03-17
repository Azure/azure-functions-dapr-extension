// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Azure.WebJobs.Description;

namespace Microsoft.Azure.WebJobs.Extensions.Dapr
{
    /// <summary>
    /// Abstract base class for Dapr binding attributes.
    /// </summary>
    public abstract class DaprBaseAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the address of the Dapr runtime endpoint.
        /// </summary>
        /// <remarks>
        /// If not specified, the default value of <c>http://localhost:3500</c> is used.
        /// </remarks>
        [AutoResolve]
        public string DaprAddress { get; set; } = "http://localhost:3500";
    }
}