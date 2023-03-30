// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// ------------------------------------------------------------

namespace Microsoft.Azure.WebJobs.Extension.Dapr
{
    using System;
    using Microsoft.Azure.WebJobs.Description;

    /// <summary>
    /// Attribute to specify parameters for the Dapr-invoke output binding.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
    [Binding]
    public sealed class DaprInvokeAttribute : DaprBaseAttribute
    {
        /// <summary>
        /// Gets or sets the dapr app name to invoke.
        /// </summary>
        [AutoResolve]
        public string? AppId { get; set; }

        /// <summary>
        /// Gets or sets the method name of the app to invoke.
        /// </summary>
        [AutoResolve]
        public string? MethodName { get; set; }

        /// <summary>
        /// Gets or sets the http verb of the app to invoke.
        /// </summary>
        [AutoResolve]
        public string HttpVerb { get; set; } = "POST";
    }
}