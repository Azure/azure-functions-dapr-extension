// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.Azure.WebJobs.Extensions.Dapr
{
    using System;
    using Microsoft.Azure.WebJobs.Description;

    /// <summary>
    /// Attribute to specify parameters for the dapr-invoke output binding.
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
