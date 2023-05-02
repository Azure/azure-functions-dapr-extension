﻿// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// ------------------------------------------------------------

namespace Microsoft.Azure.WebJobs.Extensions.Dapr.Services
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Net.Sockets;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.WebJobs.Extensions.Dapr.Exceptions;
    using Microsoft.Azure.WebJobs.Extensions.Dapr.Utils;

    /// <summary>
    /// Dapr client.
    /// </summary>
    public class DaprHttpClient : IDaprClient
    {
        readonly HttpClient httpClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="DaprHttpClient"/> class.
        /// </summary>
        /// <param name="clientFactory">Client factory.</param>
        public DaprHttpClient(IHttpClientFactory clientFactory)
        {
            this.httpClient = clientFactory.CreateClient("DaprServiceClient");
        }

        /// <inheritdoc/>
        public async Task<HttpResponseMessage> PostAsync(string uri, StringContent stringContent, CancellationToken cancellationToken)
        {
            return await DaprHttpCall(async () =>
            {
                var response = await this.httpClient.PostAsync(uri, stringContent, cancellationToken);

                return response;
            });
        }

        /// <inheritdoc/>
        public async Task<HttpResponseMessage> GetAsync(string uri, CancellationToken cancellationToken)
        {
            return await DaprHttpCall(async () =>
            {
                var response = await this.httpClient.GetAsync(uri, cancellationToken);

                return response;
            });
        }

        /// <inheritdoc/>
        public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage httpRequestMessage, CancellationToken cancellationToken)
        {
            return await DaprHttpCall(async () =>
            {
                HttpResponseMessage response = await this.httpClient.SendAsync(httpRequestMessage, cancellationToken);

                return response;
            });
        }

        private static async Task ThrowIfDaprFailure(HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
            {
                string errorCode = string.Empty;
                string errorMessage = string.Empty;

                if (response.Content != null && response.Content.Headers.ContentLength != 0)
                {
                    JsonElement daprError;

                    try
                    {
                        string content = await response.Content.ReadAsStringAsync();
                        daprError = JsonDocument.Parse(content).RootElement;
                    }
                    catch (Exception e)
                    {
                        throw new DaprException(
                            response.StatusCode,
                            ErrorCodes.ErrUnknown,
                            "The returned error message from Dapr Service is not a valid JSON Object.",
                            e);
                    }

                    if (daprError.TryGetProperty("message", out JsonElement errorMessageToken))
                    {
                        errorMessage = errorMessageToken.GetRawText();
                    }

                    if (daprError.TryGetProperty("errorCode", out JsonElement errorCodeToken))
                    {
                        errorCode = errorCodeToken.GetRawText();
                    }
                }

                // avoid potential overrides: specific 404 error messages can be returned from Dapr
                // ex: https://docs.dapr.io/reference/api/actors_api/#get-actor-state
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    throw new DaprException(
                        response.StatusCode,
                        string.IsNullOrEmpty(errorCode) ? ErrorCodes.ErrDaprResourceDoesNotExist : errorCode,
                        string.IsNullOrEmpty(errorMessage) ? "The requested Dapr resource is not properly configured." : errorMessage);
                }

                throw new DaprException(
                    response.StatusCode,
                    string.IsNullOrEmpty(errorCode) ? ErrorCodes.ErrUnknown : errorCode,
                    string.IsNullOrEmpty(errorMessage) ? "No meaningful error message is returned." : errorMessage);
            }

            return;
        }

        private static async Task<HttpResponseMessage> DaprHttpCall(Func<Task<HttpResponseMessage>> httpCall)
        {
            try
            {
                var response = await httpCall();
                await ThrowIfDaprFailure(response);

                return response;
            }
            catch (HttpRequestException ex) when (ex.InnerException is SocketException socketException)
            {
                if (socketException.SocketErrorCode == SocketError.ConnectionRefused)
                {
                    throw new DaprSidecarNotPresentException(HttpStatusCode.ServiceUnavailable, ErrorCodes.ErrDaprSidecarDoesNotExist, "Dapr sidecar is not present. Please follow this link (https://aka.ms/azure-functions-dapr-sidecar-missing) to debug the issue with dapr.", ex);
                }

                throw new DaprException(HttpStatusCode.InternalServerError, ErrorCodes.ErrDaprRequestFailed, ex.Message, ex);
            }
            catch (DaprException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new DaprException(HttpStatusCode.InternalServerError, ErrorCodes.ErrDaprRequestFailed, ex.Message, ex);
            }
        }
    }
}