// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using Microsoft.Azure.WebJobs.Description;

namespace Microsoft.Azure.WebJobs.Extensions.Dapr
{
    public abstract class DaprBaseAttribute : Attribute
    {
        [AutoResolve]
        public string DaprAddress { get; set; } = "http://localhost:3500";
    }
}