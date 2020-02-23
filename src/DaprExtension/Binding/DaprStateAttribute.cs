// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using Microsoft.Azure.WebJobs.Description;

namespace Microsoft.Azure.WebJobs.Extensions.Dapr
{
    /// <summary>Attribute to specify parameters for the Event Grid output binding</summary>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
    [Binding]
    public sealed class DaprStateAttribute : DaprBaseAttribute
    {
        /// <summary>The name of the state store to retrieve or store state.  
        /// Is found in the `metadata.name` of the component
        /// </summary>
        [AutoResolve]
        public string StateStore { get; set; }

        /// <summary>The key name to get or set state. </summary>
        [AutoResolve]
        public string Key { get; set; }
    }
}
