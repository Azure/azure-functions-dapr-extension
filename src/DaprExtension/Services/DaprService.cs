// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Net.Http;

namespace Microsoft.Azure.WebJobs.Extensions.Dapr
{
    internal class DaprService
    {
        private readonly HttpClient _client;
        public DaprService(IHttpClientFactory clientFactory)
        {
            _client = clientFactory.CreateClient("DaprServiceClient");
        }
    }
}