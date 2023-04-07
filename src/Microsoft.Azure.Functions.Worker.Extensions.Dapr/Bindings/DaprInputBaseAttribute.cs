// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// ------------------------------------------------------------

namespace Microsoft.Azure.Functions.Worker.Extensions.Dapr
{
    using Microsoft.Azure.Functions.Worker.Extensions.Abstractions;

    /// <summary>
    /// Abstract base class for Dapr input binding attributes.
    /// </summary>
    public abstract class DaprInputBaseAttribute : InputBindingAttribute
    {
        /// <summary>
        /// Gets or sets the address of the Dapr runtime endpoint.
        /// </summary>
        /// <remarks>
        /// If not specified, the default value of <c>http://localhost:{daprPort}</c> is used.
        /// If the <c>DAPR_HTTP_PORT</c> environment variable is present, that value is used
        /// for <c>{daprPort}</c>. Otherwise port 3500 is assumed.
        /// </remarks>
        public string? DaprAddress { get; set; }
    }
}