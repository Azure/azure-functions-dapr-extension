// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// ------------------------------------------------------------

namespace Microsoft.Azure.WebJobs.Extension.Dapr
{
    using System.Threading.Tasks;
    using Microsoft.Azure.WebJobs.Host.Triggers;

    static class Utils
    {
        public static readonly Task<ITriggerBinding?> NullTriggerBindingTask =
            Task.FromResult<ITriggerBinding?>(null);
    }
}