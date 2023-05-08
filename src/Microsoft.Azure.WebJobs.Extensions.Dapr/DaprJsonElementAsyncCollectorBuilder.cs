// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// ------------------------------------------------------------

namespace Microsoft.Azure.WebJobs.Extensions.Dapr
{
    using System.Text.Json;
    using Microsoft.Azure.WebJobs.Extensions.Dapr.Services;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    internal class DaprJsonElementAsyncCollectorBuilder : IConverter<DaprStateAttribute, IAsyncCollector<JsonElement>>
    {
        private readonly IDaprServiceClient daprServiceClient;

        public DaprJsonElementAsyncCollectorBuilder(IDaprServiceClient daprServiceClient)
        {
            this.daprServiceClient = daprServiceClient;
        }

        IAsyncCollector<JsonElement> IConverter<DaprStateAttribute, IAsyncCollector<JsonElement>>.Convert(DaprStateAttribute attribute)
        {
            return new DaprJsonElementAsyncCollector(this.daprServiceClient, attribute);
        }
    }
}