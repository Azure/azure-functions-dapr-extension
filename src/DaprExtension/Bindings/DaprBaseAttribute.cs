// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// ------------------------------------------------------------

namespace Dapr.AzureFunctions.Extension
{
    using System;
    using Microsoft.Azure.WebJobs.Description;

    /// <summary>
    /// Abstract base class for Dapr binding attributes.
    /// </summary>
    public abstract class DaprBaseAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the address of the Dapr runtime endpoint.
        /// </summary>
        /// <remarks>
        /// If not specified, the default value of <c>http://localhost:{daprPort}</c> is used.
        /// If the <c>DAPR_HTTP_PORT</c> environment variable is present, that value is used
        /// for <c>{daprPort}</c>. Otherwise port 3500 is assumed.
        /// </remarks>
        [AutoResolve]
        public string? DaprAddress { get; set; }
    }
}