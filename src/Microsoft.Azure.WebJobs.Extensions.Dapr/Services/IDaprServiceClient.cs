// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// ------------------------------------------------------------

namespace Microsoft.Azure.WebJobs.Extensions.Dapr.Services
{
    using System.Collections.Generic;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// DaprServiceClient interface.
    /// </summary>
    public interface IDaprServiceClient
    {
        /// <summary>
        /// Save state to a Dapr state store.
        /// </summary>
        /// <param name="daprAddress">Dapr address.</param>
        /// <param name="stateStore">State store name.</param>
        /// <param name="values">Values.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        Task SaveStateAsync(
            string? daprAddress,
            string? stateStore,
            IEnumerable<DaprStateRecord> values,
            CancellationToken cancellationToken);

        /// <summary>
        /// Get state from a Dapr state store.
        /// </summary>
        /// <param name="daprAddress">Dapr address.</param>
        /// <param name="stateStore">State store name.</param>
        /// <param name="key">Key.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<DaprStateRecord> GetStateAsync(
            string? daprAddress,
            string stateStore,
            string key,
            CancellationToken cancellationToken);

        /// <summary>
        /// Invoke a Dapr service method.
        /// </summary>
        /// <param name="daprAddress">Dapr address.</param>
        /// <param name="appId">AppId.</param>
        /// <param name="methodName">Method name.</param>
        /// <param name="httpVerb">Http verb.</param>
        /// <param name="body">Body.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        Task InvokeMethodAsync(
            string? daprAddress,
            string appId,
            string methodName,
            string httpVerb,
            object? body,
            CancellationToken cancellationToken);

        /// <summary>
        /// Send message to Dapr binding.
        /// </summary>
        /// <param name="daprAddress">Dapr address.</param>
        /// <param name="message">Message.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        Task SendToDaprBindingAsync(
           string? daprAddress,
           DaprBindingMessage message,
           CancellationToken cancellationToken);

        /// <summary>
        /// Publish event to Dapr pub/sub.
        /// </summary>
        /// <param name="daprAddress">Dapr address.</param>
        /// <param name="name">Name.</param>
        /// <param name="topicName">Topic name.</param>
        /// <param name="payload">Payload.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        Task PublishEventAsync(
            string? daprAddress,
            string name,
            string topicName,
            JsonElement? payload,
            CancellationToken cancellationToken);

        /// <summary>
        /// Get secret from Dapr secret store.
        /// </summary>
        /// <param name="daprAddress">Dapr address.</param>
        /// <param name="secretStoreName">Secret store name.</param>
        /// <param name="key">Key.</param>
        /// <param name="metadata">Metadata.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<JsonDocument> GetSecretAsync(
            string? daprAddress,
            string secretStoreName,
            string? key,
            string? metadata,
            CancellationToken cancellationToken);
    }
}