// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Azure.WebJobs.Description;

namespace Microsoft.Azure.WebJobs.Extensions.Dapr
{
    /// <summary>
    /// Abstract base class for Dapr binding attributes.
    /// </summary>
    public abstract class DaprBaseAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the address of the Dapr runtime endpoint.
        /// </summary>
        /// <remarks>
        /// If not specified, the default value of <c>http://localhost:{daprPort}</c> is used.
        /// If the <c>DAPR_HTTP_PORT</c> environment variable is present, that value is used
        /// for <c>{daprPort}</c>. Otherwise port 3500 is assumed.
        /// </remarks>
        [AutoResolve]
        public string DaprAddress { get; set; } = GetDaprAddress();

        static string GetDaprAddress()
        {
            if (!int.TryParse(Environment.GetEnvironmentVariable("DAPR_HTTP_PORT"), out int daprPort))
            {
                daprPort = 3500;
            }

            return $"http://localhost:{daprPort}";
        }
    }
}