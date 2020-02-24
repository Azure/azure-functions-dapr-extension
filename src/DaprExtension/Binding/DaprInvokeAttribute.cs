// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Net.Http;
using Microsoft.Azure.WebJobs.Description;

namespace Microsoft.Azure.WebJobs.Extensions.Dapr
{
    /// <summary>Attribute to specify parameters for the Event Grid output binding</summary>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
    [Binding]
    public sealed class DaprInvokeAttribute : DaprBaseAttribute
    {
        /// <summary>The dapr app name to invoke.
        /// </summary>
        [AutoResolve]
        public string AppId { get; set; }

        /// <summary>The method name of the app to invoke. </summary>
        [AutoResolve]
        public string MethodName { get; set; }

        /// <summary>The http verb of the app to invoke. </summary>
        [AutoResolve]
        public string HttpVerb { get; set; }
    }
}
