// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Microsoft.Azure.WebJobs.Extensions.Dapr
{
    internal class DaprService
    {
        private readonly HttpClient _client;
        public DaprService(IHttpClientFactory clientFactory)
        {
            _client = clientFactory.CreateClient("DaprServiceClient");
        }

        internal async Task SaveStateAsync(string daprAddress, string stateStore, StateContent stateContent)
        {
            await _client.PostAsJsonAsync($"{daprAddress}/v1.0/state/{stateStore}", stateContent);
        }
    }
}