﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.Azure.WebJobs.Extensions.Dapr
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    class DaprServiceClient
    {
        readonly HttpClient httpClient;
        readonly string defaultDaprAddress;

        public DaprServiceClient(
            IHttpClientFactory clientFactory,
            INameResolver nameResolver)
        {
            this.httpClient = clientFactory.CreateClient("DaprServiceClient");

            // "daprAddress" is an environment variable created by the Dapr process
            this.defaultDaprAddress = GetDefaultDaprAddress(nameResolver);
        }

        static string GetDefaultDaprAddress(INameResolver resolver)
        {
            if (!int.TryParse(resolver.Resolve("DAPR_HTTP_PORT"), out int daprPort))
            {
                daprPort = 3500;
            }

            return $"http://localhost:{daprPort}";
        }

        internal async Task SaveStateAsync(
            string? daprAddress,
            string? stateStore,
            IEnumerable<DaprStateRecord> values,
            CancellationToken cancellationToken)
        {
            if (stateStore == null)
            {
                throw new ArgumentNullException(nameof(stateStore));
            }

            this.EnsureDaprAddress(ref daprAddress);

            // TODO: Error handling
            await this.httpClient.PostAsJsonAsync(
                $"{daprAddress}/v1.0/state/{stateStore}",
                values,
                cancellationToken);
        }

        internal async Task<DaprStateRecord> GetStateAsync(
            string? daprAddress,
            string stateStore,
            string key,
            CancellationToken cancellationToken)
        {
            this.EnsureDaprAddress(ref daprAddress);

            // TODO: Error handling
            HttpResponseMessage response = await this.httpClient.GetAsync(
                $"{daprAddress}/v1.0/state/{stateStore}/{key}",
                cancellationToken);
            Stream contentStream = await response.Content.ReadAsStreamAsync();
            string? eTag = response.Headers.ETag?.Tag;
            return new DaprStateRecord(key, contentStream, eTag);
        }

        internal async Task InvokeMethodAsync(
            string? daprAddress,
            string appId,
            string methodName,
            string httpVerb,
            JToken? body,
            CancellationToken cancellationToken)
        {
            this.EnsureDaprAddress(ref daprAddress);

            var req = new HttpRequestMessage(new HttpMethod(httpVerb), $"{daprAddress}/v1.0/invoke/{appId}/method/{methodName}");
            if (body != null)
            {
                req.Content = new StringContent(body.ToString(Formatting.None), Encoding.UTF8, "application/json");
            }

            // TODO: Error handling
            await this.httpClient.SendAsync(req, cancellationToken);
        }

        internal Task PublishEventAsync(
            string? daprAddress,
            string? topicName,
            JToken? payload,
            CancellationToken cancellationToken)
        {
            this.EnsureDaprAddress(ref daprAddress);

            var req = new HttpRequestMessage(HttpMethod.Post, $"{daprAddress}/v1.0/publish/{topicName}");
            if (payload != null)
            {
                req.Content = new StringContent(payload.ToString(Formatting.None), Encoding.UTF8, "application/json");
            }

            return this.httpClient.SendAsync(req, cancellationToken);
        }

        internal async Task<JObject> GetSecretAsync(
            string? daprAddress,
            string secretStoreName,
            string? key,
            string? metadata,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(secretStoreName))
            {
                throw new ArgumentNullException(nameof(secretStoreName));
            }

            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            this.EnsureDaprAddress(ref daprAddress);

            string metadataQuery = string.Empty;
            if (!string.IsNullOrEmpty(metadata))
            {
                metadataQuery = "?" + metadata;
            }

            HttpResponseMessage response = await this.httpClient.GetAsync(
                $"{daprAddress}/v1.0/secrets/{secretStoreName}/{key}{metadataQuery}",
                cancellationToken);

            // TODO: Error handling (404 Not Found, etc.)
            string secretPayload = await response.Content.ReadAsStringAsync();

            // The response is always expected to be a JSON object
            return JObject.Parse(secretPayload);
        }

        internal async Task<DaprActorStateRecord> GetActorStateAsync(
            string? daprAddress,
            string actorType,
            string actorId,
            string key,
            CancellationToken cancellationToken)
        {
            this.EnsureDaprAddress(ref daprAddress);

            HttpResponseMessage response = await this.httpClient.GetAsync(
                $"{daprAddress}/v1.0/actors/{actorType}/{actorId}/state/{key}",
                cancellationToken);

            // TODO: Error handling (404 Not Found for actor not found etc.)
            Stream contentStream = await response.Content.ReadAsStreamAsync();
            return new DaprActorStateRecord(key, contentStream);
        }

        internal async Task SaveActorStateAsync(
            string? daprAddress,
            string? actorType,
            string? actorId,
            string? key,
            DaprActorStateRecord record,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(actorType))
            {
                throw new ArgumentNullException(nameof(actorType));
            }

            if (string.IsNullOrEmpty(actorId))
            {
                throw new ArgumentNullException(nameof(actorId));
            }

            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            this.EnsureDaprAddress(ref daprAddress);

            // TODO: Error handling
            await this.httpClient.PostAsJsonAsync(
                $"{daprAddress}/v1.0/actors/{actorType}/{actorId}/state/{key}",
                record.Value,
                cancellationToken);
        }

        void EnsureDaprAddress(ref string? daprAddress)
        {
            (daprAddress ??= this.defaultDaprAddress).TrimEnd('/');
        }
    }
}