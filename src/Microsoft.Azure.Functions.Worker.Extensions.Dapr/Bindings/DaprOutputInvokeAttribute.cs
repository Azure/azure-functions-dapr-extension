// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// ------------------------------------------------------------

namespace Microsoft.Azure.Functions.Worker.Extensions.Dapr
{
    using System;

    /// <summary>
    /// Attribute to specify parameters for the Dapr invoke output binding.
    /// </summary>
    public class DaprOutputInvokeAttribute : DaprOutputBaseAttribute
    {
        /// <summary>
        /// Gets or sets the dapr app name to invoke.
        /// </summary>
        public string? AppId { get; set; }

        /// <summary>
        /// Gets or sets the method name of the app to invoke.
        /// </summary>
        public string? MethodName { get; set; }

        /// <summary>
        /// Gets or sets the http verb of the app to invoke.
        /// </summary>
        public string? HttpVerb { get; set; } = "POST";
    }
}