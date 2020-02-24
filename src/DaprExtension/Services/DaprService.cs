// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
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

        internal async Task SaveStateAsync(string daprAddress, string stateStore, IList<StateContent> stateContent)
        {
            await _client.PostAsJsonAsync($"{daprAddress}/v1.0/state/{stateStore}", stateContent);
        }

        internal async Task InvokeMethodAsync(string daprAddress, string appId, string methodName, string httpVerb, JToken body)
        {
            var req = new HttpRequestMessage(new HttpMethod(httpVerb), $"{daprAddress}/v1.0/invoke/{appId}/method/{methodName}")
            {
                Content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json")
            };
            await _client.SendAsync(req);
        }

        internal async Task<Stream> GetStateAsync(string daprAddress, string stateStore, string key)
        {
            var res = await _client.GetAsync($"{daprAddress}/v1.0/state/{stateStore}/{key}");
            var resStream = await res.Content.ReadAsStreamAsync();
            return resStream;
        }
    }
}