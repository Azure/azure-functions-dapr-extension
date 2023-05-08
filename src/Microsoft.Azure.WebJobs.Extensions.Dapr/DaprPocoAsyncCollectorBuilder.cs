// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// ------------------------------------------------------------

namespace Microsoft.Azure.WebJobs.Extensions.Dapr
{
    using Microsoft.Azure.WebJobs.Extensions.Dapr.Services;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    internal class DaprPocoAsyncCollectorBuilder<T> : IConverter<DaprStateAttribute, IAsyncCollector<T>>
    {
        private readonly IDaprServiceClient daprServiceClient;

        public DaprPocoAsyncCollectorBuilder(IDaprServiceClient daprServiceClient)
        {
            this.daprServiceClient = daprServiceClient;
        }

        IAsyncCollector<T> IConverter<DaprStateAttribute, IAsyncCollector<T>>.Convert(DaprStateAttribute attribute)
        {
            return new DaprPocoAsyncCollector<T>(this.daprServiceClient, attribute);
        }
    }
}