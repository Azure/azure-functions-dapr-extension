// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

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
    class DaprServiceClient
    {
        readonly HttpClient httpClient;

        public DaprServiceClient(IHttpClientFactory clientFactory)
        {
            this.httpClient = clientFactory.CreateClient("DaprServiceClient");
        }

        internal async Task SaveStateAsync(string daprAddress, string? stateStore, IList<StateContent> stateContent)
        {
            if (stateStore == null)
            {
                throw new ArgumentNullException(nameof(stateStore));
            }

            await this.httpClient.PostAsJsonAsync($"{daprAddress}/v1.0/state/{stateStore}", stateContent);
        }

        internal async Task InvokeMethodAsync(string daprAddress, string appId, string methodName, string httpVerb, JToken? body)
        {
            var req = new HttpRequestMessage(new HttpMethod(httpVerb), $"{daprAddress}/v1.0/invoke/{appId}/method/{methodName}");
            if (body != null)
            {
                req.Content = new StringContent(body.ToString(Formatting.None), Encoding.UTF8, "application/json");
            }

            await this.httpClient.SendAsync(req);
        }

        internal async Task<Stream> GetStateAsync(string? daprAddress, string? stateStore, string? key)
        {
            var res = await this.httpClient.GetAsync($"{daprAddress}/v1.0/state/{stateStore}/{key}");
            var resStream = await res.Content.ReadAsStreamAsync();
            return resStream;
        }
    }
}