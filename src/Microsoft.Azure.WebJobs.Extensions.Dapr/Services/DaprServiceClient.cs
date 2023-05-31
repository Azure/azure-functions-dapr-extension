// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// ------------------------------------------------------------

namespace Microsoft.Azure.WebJobs.Extensions.Dapr.Services
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Functions.Extensions.Dapr.Core;
    using Microsoft.Azure.Functions.Extensions.Dapr.Core.Utils;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.Dapr.Exceptions;
    using Microsoft.Azure.WebJobs.Extensions.Dapr.Utils;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Dapr service client.
    /// </summary>
    public partial class DaprServiceClient : IDaprServiceClient
    {
        private const int DefaultDaprPort = 3500;

        readonly ILogger logger;
        readonly string defaultDaprAddress;
        readonly IDaprClient daprClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="DaprServiceClient"/> class.
        /// </summary>
        /// <param name="loggerFactory">Logger factory.</param>
        /// <param name="daprClient">Dapr client.</param>
        /// <param name="nameResolver">Name resolver.</param>
        public DaprServiceClient(ILoggerFactory loggerFactory, IDaprClient daprClient, INameResolver nameResolver)
        {
            this.logger = loggerFactory.CreateLogger(LoggingUtils.CreateDaprBindingCategory());
            this.daprClient = daprClient;
            this.defaultDaprAddress = this.GetDefaultDaprAddress(nameResolver);
        }

        private string GetDefaultDaprAddress(INameResolver resolver)
        {
            if (!int.TryParse(resolver.Resolve("DAPR_HTTP_PORT"), out int daprPort))
            {
                daprPort = DefaultDaprPort;
                this.logger.LogDebug("DAPR_HTTP_PORT environment variable not found. Using port {daprPort} as default.", daprPort);
            }

            var daprHost = resolver.Resolve("DAPR_HTTP_HOST");
            if (!string.IsNullOrEmpty(daprHost))
            {
                return $"http://{daprHost}:{daprPort}";
            }

            return $"http://localhost:{daprPort}";
        }

        /// <inheritdoc/>
        public async Task SaveStateAsync(
            string? daprAddress,
            string? stateStore,
            IEnumerable<DaprStateRecord> values,
            CancellationToken cancellationToken)
        {
            if (stateStore == null)
            {
                throw new ArgumentNullException(nameof(stateStore));
            }

            try
            {
                this.EnsureDaprAddress(ref daprAddress);

                var stringContent = new StringContent(
                    JsonSerializer.Serialize(values, JsonUtils.DefaultSerializerOptions),
                    Encoding.UTF8,
                    "application/json");
                var uri = $"{daprAddress}/v1.0/state/{Uri.EscapeDataString(stateStore)}";

                await this.daprClient.PostAsync(uri, stringContent, cancellationToken);
            }
            catch (JsonException ex)
            {
                throw new DaprException(HttpStatusCode.BadRequest, ErrorCodes.ErrDaprBadRequest, "Failed to serialize. Reason: " + ex.Message, ex);
            }
            catch (Exception ex)
            {
                if (ex is DaprException || ex is DaprSidecarNotPresentException)
                {
                    throw;
                }

                throw new DaprException(HttpStatusCode.InternalServerError, ErrorCodes.ErrDaprRequestFailed, "An error occurred while saving state.", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<DaprStateRecord> GetStateAsync(
            string? daprAddress,
            string stateStore,
            string key,
            CancellationToken cancellationToken)
        {
            try
            {
                this.EnsureDaprAddress(ref daprAddress);

                var uri = $"{daprAddress}/v1.0/state/{stateStore}/{key}";

                var response = await this.daprClient.GetAsync(uri, cancellationToken);

                Stream contentStream = await response.Content.ReadAsStreamAsync();
                string? eTag = response.Headers.ETag?.Tag;

                return new DaprStateRecord(key, contentStream, eTag);
            }
            catch (Exception ex)
            {
                if (ex is DaprException || ex is DaprSidecarNotPresentException)
                {
                    throw;
                }

                throw new DaprException(HttpStatusCode.InternalServerError, ErrorCodes.ErrDaprRequestFailed, "An error occurred while getting state.", ex);
            }
        }

        /// <inheritdoc/>
        public async Task InvokeMethodAsync(
            string? daprAddress,
            string appId,
            string methodName,
            string httpVerb,
            object? body,
            CancellationToken cancellationToken)
        {
            try
            {
                this.EnsureDaprAddress(ref daprAddress);

                var req = new HttpRequestMessage(new HttpMethod(httpVerb), $"{daprAddress}/v1.0/invoke/{appId}/method/{methodName}");
                if (body != null)
                {
                    req.Content = new StringContent(
                        JsonSerializer.Serialize(body, JsonUtils.DefaultSerializerOptions),
                        Encoding.UTF8,
                        "application/json");
                    req.Content.Headers.ContentType.CharSet = string.Empty;
                }

                await this.daprClient.SendAsync(req, cancellationToken);
            }
            catch (JsonException ex)
            {
                throw new DaprException(HttpStatusCode.BadRequest, ErrorCodes.ErrDaprBadRequest, "Failed to serialize. Reason: " + ex.Message, ex);
            }
            catch (Exception ex)
            {
                if (ex is DaprException || ex is DaprSidecarNotPresentException)
                {
                    throw;
                }

                throw new DaprException(HttpStatusCode.InternalServerError, ErrorCodes.ErrDaprRequestFailed, "An error occurred while invoking method.", ex);
            }
        }

        /// <inheritdoc/>
        public async Task SendToDaprBindingAsync(
            string? daprAddress,
            DaprBindingMessage message,
            CancellationToken cancellationToken)
        {
            try
            {
                this.EnsureDaprAddress(ref daprAddress);

                var stringContent = new StringContent(
                    JsonSerializer.Serialize(message, JsonUtils.DefaultSerializerOptions),
                    Encoding.UTF8,
                    "application/json");
                string uri = $"{daprAddress}/v1.0/bindings/{message.BindingName}";

                await this.daprClient.PostAsync(uri, stringContent, cancellationToken);
            }
            catch (JsonException ex)
            {
                throw new DaprException(HttpStatusCode.BadRequest, ErrorCodes.ErrDaprBadRequest, "Failed to serialize. Reason: " + ex.Message, ex);
            }
            catch (Exception ex)
            {
                if (ex is DaprException || ex is DaprSidecarNotPresentException)
                {
                    throw;
                }

                throw new DaprException(HttpStatusCode.InternalServerError, ErrorCodes.ErrDaprRequestFailed, "An error occurred while sending request to dapr binding.", ex);
            }
        }

        /// <inheritdoc/>
        public async Task PublishEventAsync(
            string? daprAddress,
            string name,
            string topicName,
            JsonElement? payload,
            CancellationToken cancellationToken)
        {
            try
            {
                this.EnsureDaprAddress(ref daprAddress);

                var req = new HttpRequestMessage(HttpMethod.Post, $"{daprAddress}/v1.0/publish/{name}/{topicName}");
                if (payload != null)
                {
                    req.Content = new StringContent(payload?.GetRawText(), Encoding.UTF8, "application/json");
                }

                await this.daprClient.SendAsync(req, cancellationToken);
            }
            catch (Exception ex)
            {
                if (ex is DaprException || ex is DaprSidecarNotPresentException)
                {
                    throw;
                }

                throw new DaprException(HttpStatusCode.InternalServerError, ErrorCodes.ErrDaprRequestFailed, "An error occurred while publishing event.", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<JsonDocument> GetSecretAsync(
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

            try
            {
                this.EnsureDaprAddress(ref daprAddress);

                string metadataQuery = string.Empty;
                if (!string.IsNullOrEmpty(metadata))
                {
                    metadataQuery = "?" + metadata;
                }

                string uri = $"{daprAddress}/v1.0/secrets/{secretStoreName}/{key}{metadataQuery}";

                var response = await this.daprClient.GetAsync(uri, cancellationToken);

                string secretPayload = await response.Content.ReadAsStringAsync();

                // The response is always expected to be a JSON object
                return JsonDocument.Parse(secretPayload);
            }
            catch (Exception ex)
            {
                if (ex is DaprException || ex is DaprSidecarNotPresentException)
                {
                    throw;
                }

                throw new DaprException(HttpStatusCode.InternalServerError, ErrorCodes.ErrDaprRequestFailed, "An error occurred while getting secret.", ex);
            }
        }

        private void EnsureDaprAddress(ref string? daprAddress)
        {
            (daprAddress ??= this.defaultDaprAddress).TrimEnd('/');
        }
    }
}