// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// ------------------------------------------------------------

namespace Microsoft.Azure.WebJobs.Extensions.Dapr.Exceptions
{
    using System;
    using System.Net;

    /// <summary>
    /// Dapr exception.
    /// </summary>
    public class DaprException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DaprException"/> class.
        /// </summary>
        /// <param name="statusCode">Status code.</param>
        /// <param name="errorCode">Error code.</param>
        /// <param name="message">Message.</param>
        public DaprException(HttpStatusCode statusCode, string errorCode, string message)
            : base(message)
        {
            this.StatusCode = statusCode;
            this.ErrorCode = errorCode;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DaprException"/> class.
        /// </summary>
        /// <param name="statusCode">Status code.</param>
        /// <param name="errorCode">Error code.</param>
        /// <param name="message">Message.</param>
        /// <param name="innerException">Inner exception.</param>
        public DaprException(HttpStatusCode statusCode, string errorCode, string message, Exception innerException)
            : base(message, innerException)
        {
            this.StatusCode = statusCode;
            this.ErrorCode = errorCode;
        }

        HttpStatusCode StatusCode { get; set; }

        string ErrorCode { get; set; }

        /// <inheritdoc/>
        public override string ToString()
        {
            if (this.InnerException != null)
            {
                return string.Format(
                    "Status Code: {0}; Error Code: {1} ; Message: {2}; Inner Exception: {3}",
                    this.StatusCode,
                    this.ErrorCode,
                    this.Message,
                    this.InnerException);
            }

            return string.Format(
                "Status Code: {0}; Error Code: {1} ; Message: {2}",
                this.StatusCode,
                this.ErrorCode,
                this.Message);
        }
    }
}