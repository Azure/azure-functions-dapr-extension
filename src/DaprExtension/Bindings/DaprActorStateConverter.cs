// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.Azure.WebJobs.Extensions.Dapr
{
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.WebJobs;
    using Newtonsoft.Json.Linq;

    class DaprActorStateConverter :
        IAsyncConverter<DaprActorStateAttribute, string>,
        IAsyncConverter<DaprActorStateAttribute, JObject>
    {
        readonly DaprServiceClient daprClient;

        public DaprActorStateConverter(DaprServiceClient daprClient)
        {
            this.daprClient = daprClient;
        }

        Task<JObject> IAsyncConverter<DaprActorStateAttribute, JObject>.ConvertAsync(
            DaprActorStateAttribute input,
            CancellationToken cancellationToken)
        {
            return this.GetActorStateAsync(input, cancellationToken);
        }

        async Task<string> IAsyncConverter<DaprActorStateAttribute, string>.ConvertAsync(
            DaprActorStateAttribute input,
            CancellationToken cancellationToken)
        {
            JObject json = await this.GetActorStateAsync(input, cancellationToken);
            return json.ToString();
        }

        Task<JObject> GetActorStateAsync(DaprActorStateAttribute input, CancellationToken cancellationToken)
        {
            return this.daprClient.GetActorStateAsync(
                input.DaprAddress,
                input.ActorType,
                input.ActorId,
                input.Key,
                cancellationToken);
        }
    }
}