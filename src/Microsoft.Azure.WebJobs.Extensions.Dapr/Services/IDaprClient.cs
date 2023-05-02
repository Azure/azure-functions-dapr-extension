// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// ------------------------------------------------------------

namespace Microsoft.Azure.WebJobs.Extensions.Dapr.Services
{
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Dapr client interface.
    /// </summary>
    public interface IDaprClient
    {
        /// <summary>
        /// Dapr Http Get call.
        /// </summary>
        /// <param name="uri">Dapr endpoint.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task<HttpResponseMessage> GetAsync(string uri, CancellationToken cancellationToken);

        /// <summary>
        /// Dapr Http Post call.
        /// </summary>
        /// <param name="uri">Dapr endpoint.</param>
        /// <param name="stringContent">String content.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task<HttpResponseMessage> PostAsync(string uri, StringContent stringContent, CancellationToken cancellationToken);

        /// <summary>
        /// Dapr Http Send call.
        /// </summary>
        /// <param name="httpRequestMessage">HttpRequestMessage.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task<HttpResponseMessage> SendAsync(HttpRequestMessage httpRequestMessage, CancellationToken cancellationToken);
    }
}