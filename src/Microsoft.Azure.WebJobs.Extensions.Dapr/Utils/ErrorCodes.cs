﻿// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// ------------------------------------------------------------

namespace Microsoft.Azure.WebJobs.Extensions.Dapr.Utils
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// Contains DAPR error codes.
    /// </summary>
    public static class ErrorCodes
    {
        /// <summary>
        /// Dapr sidecar does not exist.
        /// </summary>
        public const string ErrDaprSidecarDoesNotExist = "ERR_DAPR_SIDECAR_DOES_NOT_EXIST";

        /// <summary>
        /// Dapr request failed.
        /// </summary>
        public const string ErrDaprRequestFailed = "ERR_DAPR_REQUEST_FAILED";

        /// <summary>
        /// Dapr resource does not exist.
        /// </summary>
        public const string ErrDaprResourceDoesNotExist = "ERR_DAPR_RESOURCE_DOES_NOT_EXIST";

        /// <summary>
        /// Bad request.
        /// </summary>
        public const string ErrDaprBadRequest = "ERR_DAPR_BAD_REQUEST";

        /// <summary>
        /// Unknown Error.
        /// </summary>
        public const string ErrUnknown = "ERR_UNKNOWN";

        /// <summary>
        /// No Content.
        /// </summary>
        public const string ErrNoContent = "ERR_NO_CONTENT";
    }
}