// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// ------------------------------------------------------------

namespace Microsoft.Azure.WebJobs.Extensions.Dapr.Services
{
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// DaprServiceListener interface.
    /// </summary>
    internal interface IDaprServiceListener
    {
        /// <summary>
        /// Add function listener.
        /// </summary>
        /// <param name="daprListener">Dapr listener.</param>
        void AddFunctionListener(DaprListenerBase daprListener);

        /// <summary>
        /// Deregister listener.
        /// </summary>
        /// <param name="listener">Listener.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        Task DeregisterListenerAsync(DaprListenerBase listener, CancellationToken cancellationToken);

        /// <summary>
        /// Ensure server is started.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        Task EnsureStartedAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Register topic.
        /// </summary>
        /// <param name="topic">Topic.</param>
        void RegisterTopic(DaprTopicSubscription topic);
    }
}