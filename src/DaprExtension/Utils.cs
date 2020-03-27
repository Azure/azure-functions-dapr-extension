// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.Azure.WebJobs.Extensions.Dapr
{
    using System.Threading.Tasks;
    using Microsoft.Azure.WebJobs.Host.Triggers;

    static class Utils
    {
        public static readonly Task<ITriggerBinding?> NullTriggerBindingTask =
            Task.FromResult<ITriggerBinding?>(null);
    }
}
