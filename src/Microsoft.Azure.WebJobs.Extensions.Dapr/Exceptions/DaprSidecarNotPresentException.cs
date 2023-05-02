// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// ------------------------------------------------------------

namespace Microsoft.Azure.WebJobs.Extensions.Dapr.Services
{
    using System;
    using System.Net;
    using Microsoft.Azure.WebJobs.Extensions.Dapr.Exceptions;

    /// <summary>
    /// Dapr sidecar not present exception.
    /// </summary>
    [Serializable]
    public class DaprSidecarNotPresentException : DaprException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DaprSidecarNotPresentException"/> class.
        /// </summary>
        /// <param name="statusCode">Status code.</param>
        /// <param name="errorCode">Error code.</param>
        /// <param name="message">Message.</param>
        public DaprSidecarNotPresentException(HttpStatusCode statusCode, string errorCode, string message)
            : base(statusCode, errorCode, message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DaprSidecarNotPresentException"/> class.
        /// </summary>
        /// <param name="statusCode">Status code.</param>
        /// <param name="errorCode">Error code.</param>
        /// <param name="message">Message.</param>
        /// <param name="innerException">Inner exception.</param>
        public DaprSidecarNotPresentException(HttpStatusCode statusCode, string errorCode, string message, Exception innerException)
            : base(statusCode, errorCode, message, innerException)
        {
        }
    }
}